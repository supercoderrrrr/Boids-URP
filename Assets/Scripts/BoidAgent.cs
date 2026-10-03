using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoidAgent : MonoBehaviour
{

    public Vector3 Velocity { get; private set; }

    private BoidManager manager;

    private Vector3 wanderDirection;

    private float nextWanderChangeTime;

    public void Initialize(BoidManager boidManager)
    {
        manager = boidManager;

        Vector3 randomDirection = Random.insideUnitSphere.normalized;

        Velocity = randomDirection * manager.StartSpeed;

        wanderDirection = randomDirection;
        nextWanderChangeTime = Time.time + Random.Range(0f, manager.WanderChangeInterval);
    }

    void Update()
    {
        if (manager == null)
        {
            return;
        }

        Vector3 acceleration = Vector3.zero;

        CalculateFlockingForces(
            out Vector3 separationForce,
            out Vector3 alignmentForce,
            out Vector3 cohesionForce
            );

        Vector3 boundsForce = CalculateBoundsForce();
        Vector3 wanderForce = CalculateWander();
        Vector3 vortexForce = CalculateVortexForce();
        Vector3 targetForce = CalculateTargetForce();

        acceleration += separationForce * manager.SeparationWeight;
        acceleration += alignmentForce * manager.AlignmentWeight;
        acceleration += cohesionForce * manager.CohesionWeight;
        acceleration += boundsForce * manager.BoundsWeight;
        acceleration += wanderForce * manager.WanderWeight;
        acceleration += vortexForce;
        acceleration += targetForce * manager.TargetWeight;

        if (IsHeadingForCollision())
        {
            Vector3 avoidDirection = FindClearDirection();
            Vector3 obstacleAvoidanceForce = SteerTowards(avoidDirection);

            acceleration += obstacleAvoidanceForce * manager.ObstacleAvoidanceWeight;
        }

        Velocity += acceleration * Time.deltaTime;

        float speed = Velocity.magnitude;

        Vector3 direction = speed > 0.0001f
            ? Velocity / speed
            : transform.forward;

        speed = Mathf.Clamp(speed, manager.MinSpeed, manager.MaxSpeed);
        Velocity = direction * speed;

        transform.position += Velocity * Time.deltaTime;

        if(Velocity.sqrMagnitude > 0.001f)
        {

            transform.rotation = Quaternion.LookRotation(Velocity);
        }
    }

    private Vector3 CalculateSeparation()
    {
        Vector3 separationDirection = Vector3.zero;
        int neighboursInSeparationRange = 0;

        foreach(BoidAgent otherBoid in manager.Boids)
        {
            if(otherBoid == this)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, otherBoid.transform.position);

            if(distance > 0.0001f && distance < manager.SeparationRadius)
            {

                Vector3 awayFromOther = transform.position - otherBoid.transform.position;

                separationDirection += awayFromOther.normalized / distance;
                neighboursInSeparationRange++;
            }
        }

        if (neighboursInSeparationRange == 0)
        {
            return Vector3.zero;
        }

        separationDirection /= neighboursInSeparationRange;

        Vector3 desiredVelocity = separationDirection.normalized * manager.MaxSpeed;

        Vector3 steeringForce = desiredVelocity - Velocity;

        return Vector3.ClampMagnitude(steeringForce, manager.MaxSteerForce);
    }

    private Vector3 CalculateAlignment()
    {
        Vector3 averageDirection = Vector3.zero;
        int neighboursInPerceptionRange = 0;

        foreach(BoidAgent otherBoid in manager.Boids)
        {
            if(otherBoid == this)
            {
                continue;
            }

            Vector3 offset = transform.position - otherBoid.transform.position;
            float distanceSqr = offset.sqrMagnitude;
            float perceptionRadiusSqr = manager.PerceptionRadius * manager.PerceptionRadius;

            if(distanceSqr < perceptionRadiusSqr )
            {
                if(otherBoid.Velocity.sqrMagnitude > 0.0001f)
                {
                    averageDirection += otherBoid.Velocity.normalized;
                    neighboursInPerceptionRange++;
                }
            }
        }

        if(neighboursInPerceptionRange == 0)
        {
            return Vector3.zero;
        }

        averageDirection /= neighboursInPerceptionRange;

        return SteerTowards(averageDirection);
    }

    private Vector3 CalculateCohesion()
    {
        Vector3 centerPosition = Vector3.zero;
        int neighboursInPerceptionRange = 0;

        foreach(BoidAgent otherBoid in manager.Boids )
        {
            if( otherBoid == this)
            {
                continue;
            }

            float distance = Vector3.Distance(transform.position, otherBoid.transform.position);

            if(distance < manager.PerceptionRadius)
            {
                centerPosition += otherBoid.transform.position;
                neighboursInPerceptionRange++;
            }
        }

        if(neighboursInPerceptionRange == 0)
        {
            return Vector3.zero;
        }

        centerPosition /= neighboursInPerceptionRange;

        Vector3 directionToCenter = centerPosition - transform.position;

        if(directionToCenter.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        Vector3 desiredVelocity = directionToCenter.normalized * manager.MaxSpeed;
        Vector3 steeringForce = desiredVelocity - Velocity;

        return Vector3.ClampMagnitude(steeringForce, manager.MaxSteerForce);
    }

    // Predict the next position before steering away from the six box faces
    private Vector3 CalculateBoundsForce()
    {
        if (manager.SwimVolume == null)
        {
            return Vector3.zero;
        }

        BoxCollider volume = manager.SwimVolume;

        Vector3 moveDirection = Velocity.sqrMagnitude > 0.0001f
            ? Velocity.normalized
            : transform.forward;

        Vector3 predictedPosition = transform.position + moveDirection * manager.BoundsLookAheadDistance;

        Vector3 localPosition = volume.transform.InverseTransformPoint(predictedPosition);
        localPosition -= volume.center;

        Vector3 halfSize = volume.size * 0.5f;
        Vector3 steerDirection = Vector3.zero;

        float distanceToRightWall = halfSize.x - localPosition.x;
        float distanceToLeftWall = localPosition.x + halfSize.x;

        float distanceToTopWall = halfSize.y - localPosition.y;
        float distanceToBottomWall = localPosition.y + halfSize.y;

        float distanceToFrontWall = halfSize.z - localPosition.z;
        float distanceToBackWall = localPosition.z + halfSize.z;

        if (distanceToRightWall < manager.WallAvoidDistance)
        {
            steerDirection.x -= 1f - distanceToRightWall / manager.WallAvoidDistance;
        }

        if (distanceToLeftWall < manager.WallAvoidDistance)
        {
            steerDirection.x += 1f - distanceToLeftWall / manager.WallAvoidDistance;
        }

        if (distanceToTopWall < manager.WallAvoidDistance)
        {
            steerDirection.y -= 1f - distanceToTopWall / manager.WallAvoidDistance;
        }

        if (distanceToBottomWall < manager.WallAvoidDistance)
        {
            steerDirection.y += 1f - distanceToBottomWall / manager.WallAvoidDistance;
        }

        if (distanceToFrontWall < manager.WallAvoidDistance)
        {
            steerDirection.z -= 1f - distanceToFrontWall / manager.WallAvoidDistance;
        }

        if (distanceToBackWall < manager.WallAvoidDistance)
        {
            steerDirection.z += 1f - distanceToBackWall / manager.WallAvoidDistance;
        }

        if (steerDirection.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        Vector3 worldDirection = volume.transform.TransformDirection(steerDirection.normalized);
        Vector3 desiredVelocity = worldDirection * manager.MaxSpeed;
        Vector3 steeringForce = desiredVelocity - Velocity;

        return Vector3.ClampMagnitude(steeringForce, manager.MaxSteerForce);
    }

    private bool IsHeadingForCollision()
    {

        Vector3 direction = Velocity.sqrMagnitude > 0.0001f
            ? Velocity.normalized
            : transform.forward;

        return Physics.SphereCast(
            transform.position,
            manager.CollisionCheckRadius,
            direction,
            out RaycastHit hit,
            manager.CollisionAvoidDistance,
            manager.ObstacleMask
            );
    }

    // Test precomputed local directions until a sphere cast finds a clear route
    private Vector3 FindClearDirection()
    {

        foreach(Vector3 localDirection in BoidHelper.Directions)
        {

            Vector3 worldDirection = transform.TransformDirection(localDirection);

            bool hitObstacle = Physics.SphereCast(
                transform.position,
                manager.CollisionCheckRadius,
                worldDirection,
                out RaycastHit hit,
                manager.CollisionAvoidDistance,
                manager.ObstacleMask
                );

            if (!hitObstacle)
            {
                return worldDirection;
            }
        }
        return Velocity.normalized;
    }

    private Vector3 SteerTowards(Vector3 direction)
    {
        if (direction.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        Vector3 desiredVelocity = direction.normalized * manager.MaxSpeed;
        Vector3 steeringForce = desiredVelocity - Velocity;

        return Vector3.ClampMagnitude(steeringForce, manager.MaxSteerForce);
    }

    // Stagger direction changes so the entire school does not turn together
    private Vector3 CalculateWander()
    {
        if (Time.time >= nextWanderChangeTime)
        {

            Vector3 randomDirection = Random.insideUnitSphere;

            randomDirection.y *= manager.WanderVerticalFactor;

            if (randomDirection.sqrMagnitude > 0.0001f)
            {
                wanderDirection = randomDirection.normalized;
            }

            nextWanderChangeTime = Time.time + Random.Range(
                manager.WanderChangeInterval * 0.75f,
                manager.WanderChangeInterval * 1.25f
            );
        }

        return SteerTowards(wanderDirection);
    }

    // Blend a horizontal tangent with radial correction around the desired ring
    private Vector3 CalculateVortexForce()
    {
        if (manager.VortexCenter == null)
        {
            return Vector3.zero;
        }

        Vector3 center = manager.VortexCenter.position;
        Vector3 radial = transform.position - center;

        radial.y = 0f;

        if (radial.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        Vector3 radialDirection = radial.normalized;

        Vector3 tangentDirection = Vector3.Cross(Vector3.up, radialDirection).normalized;

        float radiusError = manager.VortexRadius - radial.magnitude;
        Vector3 shapeDirection = radialDirection * radiusError;

        Vector3 mixedDirection =
            tangentDirection * manager.VortexWeight +
            shapeDirection.normalized * manager.VortexShapeWeight;

        return SteerTowards(mixedDirection);
    }

    private Vector3 CalculateTargetForce()
    {
        if (manager.Target == null)
        {
            return Vector3.zero;
        }

        Vector3 offsetToTarget = manager.Target.position - transform.position;

        if (offsetToTarget.sqrMagnitude < 0.0001f)
        {
            return Vector3.zero;
        }

        return SteerTowards(offsetToTarget);
    }

    // Accumulate all three local rules in one neighbor traversal
    private void CalculateFlockingForces(
        out Vector3 separationForce,
        out Vector3 alignmentForce,
        out Vector3 cohesionForce)
    {
        Vector3 myPosition = transform.position;

        Vector3 separationDirection = Vector3.zero;
        Vector3 averageDirection = Vector3.zero;
        Vector3 centerPosition = Vector3.zero;

        int separationCount = 0;
        int perceptionCount = 0;

        // Compare squared distances to avoid square roots during neighbor filtering
        float separationRadiusSqr = manager.SeparationRadius * manager.SeparationRadius;
        float perceptionRadiusSqr = manager.PerceptionRadius * manager.PerceptionRadius;

        foreach(BoidAgent otherBoid in manager.Boids)
        {
            if (otherBoid == this)
            {
                continue;
            }

            Vector3 otherPosition = otherBoid.transform.position;
            Vector3 offsetFromOther = myPosition - otherPosition;
            float distanceSqr = offsetFromOther.sqrMagnitude;

            if (distanceSqr < perceptionRadiusSqr)
            {
                if (otherBoid.Velocity.sqrMagnitude > 0.0001f)
                {
                    averageDirection += otherBoid.Velocity.normalized;
                }

                centerPosition += otherPosition;
                perceptionCount++;
            }

            if (distanceSqr > 0.0001f && distanceSqr < separationRadiusSqr)
            {
                separationDirection += offsetFromOther / distanceSqr;
                separationCount++;
            }
        }

        separationForce = Vector3.zero;
        alignmentForce = Vector3.zero;
        cohesionForce = Vector3.zero;

        if (separationCount > 0)
        {
            separationDirection /= separationCount;
            separationForce = SteerTowards(separationDirection);
        }

        if (perceptionCount > 0)
        {
            averageDirection /= perceptionCount;
            centerPosition /= perceptionCount;

            if (averageDirection.sqrMagnitude > 0.0001f)
            {
                alignmentForce = SteerTowards(averageDirection);
            }

            Vector3 directionToCenter = centerPosition - myPosition;

            if (directionToCenter.sqrMagnitude > 0.0001f)
            {
                cohesionForce = SteerTowards(directionToCenter);
            }
        }
    }
}
