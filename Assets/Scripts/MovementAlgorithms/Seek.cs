using UnityEngine;
using UnityEngine.AI;

public class Seek
{
    protected Vector3 targetPosition;

    protected float maxAcceleration;

    public struct SteeringOutput
    {
        public Vector3 linear;
        public float angular;
    }
    
    public SteeringOutput GetSteering(NavMeshAgent character)
    {
        var steering = new SteeringOutput();

        steering.linear = targetPosition - character.transform.position;
        
        steering.linear.Normalize();
        steering.linear *= maxAcceleration;

        steering.angular = 0;
        return steering;
    }
}
