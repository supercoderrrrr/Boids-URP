using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace BoidsUnderwaterScene
{
    public sealed class BoidsSpatialGrid
    {
        private readonly Dictionary<Vector3Int, int> heads;
        private readonly int[] next;
        private readonly float inverseCell;
        public BoidsSpatialGrid(int capacity, float cellSize)
        {
            heads = new Dictionary<Vector3Int, int>(capacity);
            next = new int[capacity];
            inverseCell = 1f / Mathf.Max(cellSize, 0.01f);
        }
        public Vector3Int Cell(Vector3 p) => new Vector3Int(Mathf.FloorToInt(p.x * inverseCell), Mathf.FloorToInt(p.y * inverseCell), Mathf.FloorToInt(p.z * inverseCell));
        public void Rebuild(Vector3[] positions)
        {
            heads.Clear();
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3Int cell = Cell(positions[i]);
                next[i] = heads.TryGetValue(cell, out int first) ? first : -1;
                heads[cell] = i;
            }
        }
        public int First(Vector3Int cell) => heads.TryGetValue(cell, out int first) ? first : -1;
        public int Next(int index) => next[index];
    }

    public sealed class BoidsSchool : MonoBehaviour
    {
        [Header("Rendering")]
        [SerializeField] private Mesh fishMesh;
        [SerializeField] private Material fishMaterial;
        [SerializeField, Range(1, 1023)] private int count = 240;
        [SerializeField] private float fishLength = 0.65f;
        [SerializeField] private int seed = 71;
        [Header("Closed swim corridor")]
        [SerializeField] private Vector3[] waypoints;
        [SerializeField] private float corridorRadius = 2.2f;
        [SerializeField] private float schoolSpread = 0.24f;
        [SerializeField] private float initialProgress;
        [SerializeField] private float cruiseSpeed = 3.2f;
        [SerializeField] private float lookAhead = 4f;
        [SerializeField] private bool reprojectToCorridor;
        [Header("Vortex field")]
        [SerializeField] private bool vortex;
        [SerializeField] private Vector3 vortexCenter = new Vector3(0, 14, 86);
        [SerializeField] private float vortexHeight = 21f;
        [SerializeField] private float vortexRadius = 9f;
        [SerializeField] private float vortexVerticalFrequency = .075f;
        [Header("Local flocking")]
        [SerializeField] private float perceptionRadius = 3.2f;
        [SerializeField] private float separationRadius = 0.85f;
        [SerializeField] private float separationWeight = 3.2f;
        [SerializeField] private float alignmentWeight = 0.65f;
        [SerializeField] private float cohesionWeight = 0.08f;
        [SerializeField] private float maxAcceleration = 5f;
        [SerializeField] private LayerMask obstacles = 1 << 6;
        private Vector3[] positions, previous, velocities, nextVelocities, avoidance;
        private float[] progress, scales, speedFactors, vortexPhases;
        private Vector2[] offsets;
        private Quaternion[] rotations;
        private Matrix4x4[] matrices;
        private Vector3[] path;
        private float[] distances;
        private float length, accumulator;
        private float gridCellSize;
        private int tick;
        private BoidsSpatialGrid grid;
        private readonly System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
        public int AgentCount => positions == null ? 0 : positions.Length;
        public long CandidateTests { get; private set; }
        public double LastSimulationMilliseconds { get; private set; }
        public float MaximumCorridorError { get; private set; }
        public int ObstacleQueries { get; private set; }
        public int ArchPassages { get; private set; }
        public Vector3[] Positions => positions;
        public bool IsVortex => vortex;
        public float AverageSpeed
        {
            get
            {
                if(velocities==null||velocities.Length==0)return 0;
                float total=0;foreach(var velocity in velocities)total+=velocity.magnitude;
                return total/velocities.Length;
            }
        }
        public Vector3 VortexCenter => transform.TransformPoint(vortexCenter);
        public Vector3[] SampleRouteForValidation()
        {
            if (!vortex) { BuildPath(); return path; }
            var samples = new Vector3[288];
            for (int i = 0; i < samples.Length; i++)
            {
                float height = Mathf.Lerp(-.5f, .5f, (i / 48) / 5f) * vortexHeight;
                float angle = i % 48 * Mathf.PI / 24f;
                samples[i] = VortexCenter + Vector3.up * height + new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle)) * VortexRadiusAt(height);
            }
            return samples;
        }
        private const float Step = 1f / 30f;

        private void OnEnable()
        {
            if ((!vortex && (waypoints == null || waypoints.Length < 4)) || fishMesh == null || fishMaterial == null) return;
            count = Mathf.Clamp(count, 1, 1023);
            if (!vortex) BuildPath();
            positions = new Vector3[count]; previous = new Vector3[count]; velocities = new Vector3[count];
            nextVelocities = new Vector3[count]; avoidance = new Vector3[count]; progress = new float[count];
            scales = new float[count]; speedFactors = new float[count]; offsets = new Vector2[count];
            vortexPhases = new float[count];
            rotations = new Quaternion[count]; matrices = new Matrix4x4[count];
            gridCellSize = Mathf.Max(perceptionRadius, separationRadius);
            grid = new BoidsSpatialGrid(count, gridCellSize);
            tick = 0; accumulator = 0f; ArchPassages = 0;
            var random = new System.Random(seed);
            for (int i = 0; i < positions.Length; i++)
            {
                progress[i] = (initialProgress + (float)random.NextDouble() * schoolSpread) * length;
                float angle = (float)random.NextDouble() * Mathf.PI * 2f;
                float radius = Mathf.Sqrt((float)random.NextDouble()) * corridorRadius;
                offsets[i] = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius * 0.55f);
                Vector3 tangent;
                if (vortex)
                {
                    // Stratified heights and golden-angle sampling fill the whole vortex at spawn
                    float normalizedHeight = 2f * (i + .25f + (float)random.NextDouble() * .5f) / count - 1f;
                    float phase = Mathf.Asin(normalizedHeight);
                    vortexPhases[i] = random.NextDouble() < .5 ? phase : Mathf.PI - phase;
                    float height = normalizedHeight * vortexHeight * .5f;
                    angle = i * 2.39996323f + seed * .17f + ((float)random.NextDouble() - .5f) * .12f;
                    offsets[i].x = ((float)random.NextDouble() - .5f) * corridorRadius;
                    Vector3 radial = new Vector3(Mathf.Cos(angle), 0, Mathf.Sin(angle));
                    positions[i] = VortexCenter + Vector3.up * height + radial * (VortexRadiusAt(height) + offsets[i].x * .4f);
                    tangent = Vector3.Cross(Vector3.up, radial);
                }
                else
                {
                    Vector3 center = Evaluate(progress[i], out tangent);
                    positions[i] = center + Lateral(tangent, offsets[i]);
                }
                previous[i] = positions[i];
                velocities[i] = tangent * cruiseSpeed;
                rotations[i] = Quaternion.LookRotation(tangent);
                scales[i] = fishLength * Mathf.Lerp(0.78f, 1.15f, (float)random.NextDouble());
                speedFactors[i] = Mathf.Lerp(0.93f, 1.07f, (float)random.NextDouble());
                if (vortex)
                {
                    velocities[i] = VortexVelocity(i, cruiseSpeed * speedFactors[i], out _);
                    rotations[i] = Quaternion.LookRotation(velocities[i]);
                }
                matrices[i] = Matrix4x4.TRS(positions[i], rotations[i], Vector3.one * scales[i]);
            }
        }

        private void BuildPath()
        {
            int segments = waypoints.Length * 48;
            path = new Vector3[segments + 1]; distances = new float[segments + 1];
            for (int i = 0; i <= segments; i++)
            {
                float u = i / 48f; int k = Mathf.FloorToInt(u); float t = u - k; int n = waypoints.Length;
                Vector3 a = waypoints[(k - 1 + n) % n], b = waypoints[k % n];
                Vector3 c = waypoints[(k + 1) % n], d = waypoints[(k + 2) % n];
                Vector3 p = 0.5f * (2f * b + (-a + c) * t + (2f * a - 5f * b + 4f * c - d) * t * t + (-a + 3f * b - 3f * c + d) * t * t * t);
                path[i] = transform.TransformPoint(p);
                if (i > 0) distances[i] = distances[i - 1] + Vector3.Distance(path[i - 1], path[i]);
            }
            length = distances[segments];
        }

        public Vector3 Evaluate(float distance, out Vector3 tangent)
        {
            distance = Mathf.Repeat(distance, length);
            int lo = 0, hi = distances.Length - 1;
            while (hi - lo > 1) { int mid = (lo + hi) / 2; if (distances[mid] > distance) hi = mid; else lo = mid; }
            tangent = (path[hi] - path[lo]).normalized;
            return Vector3.Lerp(path[lo], path[hi], Mathf.InverseLerp(distances[lo], distances[hi], distance));
        }

        private float NearestProgress(Vector3 position, out float squaredDistance)
        {
            squaredDistance=float.PositiveInfinity;
            float nearest=0;
            for(int i=1;i<path.Length;i++)
            {
                Vector3 segment=path[i]-path[i-1];
                float t=Mathf.Clamp01(Vector3.Dot(position-path[i-1],segment)/Mathf.Max(segment.sqrMagnitude,.000001f));
                float sqr=(position-path[i-1]-segment*t).sqrMagnitude;
                if(sqr>=squaredDistance)continue;
                squaredDistance=sqr;nearest=Mathf.Lerp(distances[i-1],distances[i],t);
            }
            return nearest;
        }

        public float DistanceToGuidance(Vector3 position)
        {
            if(vortex)
            {
                Vector3 relative=position-VortexCenter;
                return Mathf.Abs(new Vector2(relative.x,relative.z).magnitude-VortexRadiusAt(relative.y));
            }
            NearestProgress(position,out float sqr);
            return Mathf.Sqrt(sqr);
        }

        private static Vector3 Lateral(Vector3 tangent, Vector2 offset)
        {
            Vector3 right = Vector3.Cross(Vector3.up, tangent).normalized;
            return right * offset.x + Vector3.up * offset.y;
        }

        private float VortexRadiusAt(float height) => vortexRadius + height * .18f;

        private Vector3 VortexVelocity(int i, float speed, out float error)
        {
            Vector3 relative = positions[i] - VortexCenter;
            Vector3 radial = new Vector3(relative.x, 0, relative.z);
            float radius = radial.magnitude;
            radial = radius > .001f ? radial / radius : Vector3.right;
            float phase = vortexPhases[i] + tick * Step * vortexVerticalFrequency;
            float targetHeight = Mathf.Sin(phase) * vortexHeight * .5f;
            float verticalSpeed = Mathf.Cos(phase) * vortexHeight * .5f * vortexVerticalFrequency;
            float targetRadius = VortexRadiusAt(relative.y) + offsets[i].x * .4f;
            error = Mathf.Abs(targetRadius - radius);
            // Tangential flow creates rotation while radial feedback keeps a hollow column
            return Vector3.Cross(Vector3.up, radial) * speed
                + radial * Mathf.Clamp((targetRadius - radius) * 1.8f, -speed, speed)
                + Vector3.up * Mathf.Clamp(verticalSpeed + (targetHeight - relative.y) * 1.2f, -1.6f, 1.6f);
        }

        private void Update()
        {
            if (positions == null) return;
            accumulator += Mathf.Min(Time.deltaTime, 0.1f);
            int steps = 0;
            while (accumulator >= Step && steps++ < 3) { Simulate(); accumulator -= Step; }
            float blend = Mathf.Clamp01(accumulator / Step);
            for (int i = 0; i < positions.Length; i++)
            {
                Quaternion desired = Quaternion.LookRotation(velocities[i]);
                rotations[i] = Quaternion.Slerp(rotations[i], desired, 1f - Mathf.Exp(-7f * Time.deltaTime));
                matrices[i] = Matrix4x4.TRS(Vector3.Lerp(previous[i], positions[i], blend), rotations[i], Vector3.one * scales[i]);
            }
            Graphics.DrawMeshInstanced(fishMesh, 0, fishMaterial, matrices, positions.Length, null,
                ShadowCastingMode.Off, true, gameObject.layer, null, LightProbeUsage.Off);
        }

        private void Simulate()
        {
            watch.Restart();
            float requestedCellSize = Mathf.Max(perceptionRadius, separationRadius, .01f);
            if (!Mathf.Approximately(requestedCellSize, gridCellSize)) { gridCellSize = requestedCellSize; grid = new BoidsSpatialGrid(positions.Length, gridCellSize); }
            grid.Rebuild(positions); CandidateTests = 0; ObstacleQueries = 0; MaximumCorridorError = 0;
            float perceptionSqr = perceptionRadius * perceptionRadius, separationSqr = separationRadius * separationRadius;
            for (int i = 0; i < positions.Length; i++)
            {
                Vector3 separation = Vector3.zero, heading = Vector3.zero, center = Vector3.zero;
                int neighbors = 0;
                Vector3Int cell = grid.Cell(positions[i]);
                for (int z = -1; z <= 1; z++) for (int y = -1; y <= 1; y++) for (int x = -1; x <= 1; x++)
                {
                    int j = grid.First(cell + new Vector3Int(x, y, z));
                    while (j >= 0)
                    {
                        if (i != j)
                        {
                            CandidateTests++;
                            Vector3 delta = positions[i] - positions[j]; float sqr = delta.sqrMagnitude;
                            if (sqr < perceptionSqr) { center += positions[j]; heading += velocities[j]; neighbors++; }
                            if (sqr > 0.00001f && sqr < separationSqr) separation += delta / Mathf.Max(sqr, 0.04f);
                        }
                        j = grid.Next(j);
                    }
                }
                float speed = cruiseSpeed * speedFactors[i];
                Vector3 desiredVelocity, tangent;
                float error;
                if (vortex)
                {
                    desiredVelocity = VortexVelocity(i, speed, out error);
                    tangent = desiredVelocity.normalized;
                }
                else
                {
                    // Stagger projection to correct progress drift without snapping fish positions
                    if(reprojectToCorridor&&(tick+i)%15==0)progress[i]=NearestProgress(positions[i],out _);
                    Vector3 onPath = Evaluate(progress[i], out tangent);
                    progress[i] += Mathf.Max(cruiseSpeed * .35f, Vector3.Dot(velocities[i], tangent)) * Step;
                    Vector3 goal = Evaluate(progress[i] + lookAhead, out tangent) + Lateral(tangent, offsets[i]);
                    desiredVelocity = (goal - positions[i]).normalized * speed;
                    error = Vector3.Distance(positions[i], onPath);
                }
                Vector3 acceleration = (desiredVelocity - velocities[i]) * 1.9f;
                acceleration += Vector3.ClampMagnitude(separation, 2f) * separationWeight;
                if (neighbors > 0)
                {
                    acceleration += (heading / neighbors - velocities[i]) * alignmentWeight;
                    acceleration += Vector3.ClampMagnitude(center / neighbors - positions[i], 2f) * cohesionWeight;
                }
                if ((tick + i) % 5 == 0) avoidance[i] = AvoidObstacle(i);
                acceleration += avoidance[i];
                MaximumCorridorError = Mathf.Max(error, MaximumCorridorError);
                Vector3 v = velocities[i] + Vector3.ClampMagnitude(acceleration, maxAcceleration) * Step;
                nextVelocities[i] = (v.sqrMagnitude > .00001f ? v.normalized : tangent) * Mathf.Clamp(v.magnitude, speed * .62f, speed * 1.22f);
            }
            // All agents read the same snapshot before any position is integrated
            for (int i = 0; i < positions.Length; i++)
            {
                previous[i] = positions[i]; velocities[i] = nextVelocities[i]; positions[i] += velocities[i] * Step;
                if (previous[i].z < 29f && positions[i].z >= 29f && Mathf.Abs(positions[i].x - 3f) < 6f && positions[i].y > 2f && positions[i].y < 10f) ArchPassages++;
            }
            tick++; watch.Stop(); LastSimulationMilliseconds = watch.Elapsed.TotalMilliseconds;
        }

        private Vector3 AvoidObstacle(int i)
        {
            Vector3 forward = velocities[i].normalized;
            float radius = fishLength * 0.32f;
            float reach = cruiseSpeed * 1.3f + 1f;
            ObstacleQueries++;
            if (!Physics.SphereCast(positions[i], radius, forward, out RaycastHit hit, reach, obstacles, QueryTriggerInteraction.Ignore)) return Vector3.zero;
            Quaternion basis = Quaternion.LookRotation(forward);
            for (int sample = 0; sample < 12; sample++)
            {
                float angle = sample * 2.39996323f;
                float cone = Mathf.Lerp(0.3f, 1.25f, sample / 11f);
                Vector3 direction = basis * new Vector3(Mathf.Cos(angle) * cone, Mathf.Sin(angle) * cone, 1f).normalized;
                ObstacleQueries++;
                if (!Physics.SphereCast(positions[i], radius, direction, out _, reach, obstacles, QueryTriggerInteraction.Ignore))
                    return (direction * cruiseSpeed - velocities[i]) * 3.5f + hit.normal * 2f;
            }
            return hit.normal * maxAcceleration * 2f;
        }

        private void OnDrawGizmosSelected()
        {
            if (vortex)
            {
                Gizmos.color = new Color(.4f, .75f, 1f, .6f);
                var samples = SampleRouteForValidation();
                for (int i = 0; i < samples.Length; i++) Gizmos.DrawLine(samples[i], samples[i / 48 * 48 + (i + 1) % 48]);
                return;
            }
            if (waypoints == null || waypoints.Length < 4) return;
            BuildPath(); Gizmos.color = new Color(.2f, .9f, .8f, .65f);
            for (int i = 1; i < path.Length; i++) Gizmos.DrawLine(path[i - 1], path[i]);
        }
    }
}
