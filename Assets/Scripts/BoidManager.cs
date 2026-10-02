using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidManager : MonoBehaviour
{
    [Header("Spawn")]
    [SerializeField] private BoidAgent boidPrefab;
    [SerializeField] private int boidCount = 100;
    [SerializeField] private Vector3 spawnArea = new Vector3(20f, 10f, 20f);

    [Header("Movement")]
    [SerializeField] private float minSpeed = 3f;
    [SerializeField] private float startSpeed = 4f;
    [SerializeField] private float maxSpeed = 8f;

    [Header("Boids Rules")]

    [SerializeField] private float separationRadius = 1.5f;

    [SerializeField] private float separationWeight = 2f;

    [SerializeField] private float maxSteerForce = 3f;

    [SerializeField] private float perceptionRadius = 4f;
    [SerializeField] private float alignmentWeight = 1f;

    [SerializeField] private float cohesionWeight = 1f;

    [Header("Tank Bounds")]

    [SerializeField] private BoxCollider swimVolume;
    [SerializeField] private float wallAvoidDistance = 3f;
    [SerializeField] private float boundsWeight = 3f;
    [SerializeField] private float boundsLookAheadDistance = 3f;

    [Header("Obstacle Avoidance")]

    [SerializeField] private LayerMask obstacleMask;

    [SerializeField] private float collisionCheckRadius = 0.4f;

    [SerializeField] private float collisionAvoidDistance = 6f;

    [SerializeField] private float obstacleAvoidanceWeight = 8f;

    [Header("Wander")]

    [SerializeField] private float wanderWeight = 0.8f;

    [SerializeField] private float wanderChangeInterval = 1.5f;

    [SerializeField] private float wanderVerticalFactor = 0.35f;

    [Header("Vortex Field")]
    [SerializeField] private Transform vortexCenter;
    [SerializeField] private float vortexRadius = 10f;
    [SerializeField] private float vortexWeight = 0.5f;
    [SerializeField] private float vortexShapeWeight = 1f;

    [Header("Target")]
    [SerializeField] private Transform target;
    [SerializeField] private float targetWeight = 0.25f;

    public float MinSpeed => minSpeed;
    public float StartSpeed => startSpeed;
    public float MaxSpeed => maxSpeed;

    public float SeparationRadius => separationRadius;
    public float SeparationWeight => separationWeight;
    public float MaxSteerForce => maxSteerForce;

    public float PerceptionRadius => perceptionRadius;
    public float AlignmentWeight => alignmentWeight;

    public float CohesionWeight => cohesionWeight;

    public BoxCollider SwimVolume => swimVolume;
    public float WallAvoidDistance => wallAvoidDistance;
    public float BoundsWeight => boundsWeight;
    public float BoundsLookAheadDistance => boundsLookAheadDistance;

    public LayerMask ObstacleMask => obstacleMask;
    public float CollisionCheckRadius => collisionCheckRadius;
    public float CollisionAvoidDistance => collisionAvoidDistance;
    public float ObstacleAvoidanceWeight => obstacleAvoidanceWeight;

    public float WanderWeight => wanderWeight;
    public float WanderChangeInterval => wanderChangeInterval;
    public float WanderVerticalFactor => wanderVerticalFactor;

    public Transform VortexCenter => vortexCenter;
    public float VortexRadius => vortexRadius;
    public float VortexWeight => vortexWeight;
    public float VortexShapeWeight => vortexShapeWeight;

    public Transform Target => target;
    public float TargetWeight => targetWeight;

    public List<BoidAgent> Boids {  get; private set; } = new List<BoidAgent>();

    void Start()
    {
        SpawnBoids();
    }

    private void SpawnBoids()
    {
        for(int i = 0; i < boidCount; i++)
        {

            Vector3 spawnPosition = transform.position + new Vector3(
                Random.Range(-spawnArea.x, spawnArea.x),
                Random.Range(-spawnArea.y, spawnArea.y),
                Random.Range(-spawnArea.z, spawnArea.z)
                );

            BoidAgent boid = Instantiate(
                boidPrefab,
                spawnPosition,
                Random.rotation,
                transform
                );

            boid.Initialize(this);
            Boids.Add(boid);
        }
    }

    void Update()
    {

    }
}
