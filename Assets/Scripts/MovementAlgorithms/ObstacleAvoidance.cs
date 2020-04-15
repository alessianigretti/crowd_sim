using UnityEngine;
using UnityEngine.AI;

public class ObstacleAvoidance : Seek
{
    private float avoidDistance = 1f;

    private float lookahead = 1f;

    public SteeringOutput? GetSteering(NavMeshAgent character)
    {
        var rayVector = character.velocity;
        rayVector.Normalize();
        rayVector *= lookahead;

        if (!Physics.Raycast(character.transform.position, rayVector, out var collision, lookahead))
        {
            return null;
        }

        targetPosition = collision.transform.position + collision.normal * avoidDistance;

        return base.GetSteering(character);
    }
}