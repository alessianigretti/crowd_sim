using UnityEngine;
using UnityEngine.AI;

public class AgentMovement : MonoBehaviour
{
    public float movementRange = 20f;
    
    private NavMeshAgent agent;
    private float additionalRemainingDistance = 0.001f;    // avoid characters getting stuck
    private MovementAlgorithm movementAlgorithm = MovementAlgorithm.Random;

    public enum MovementAlgorithm
    {
        Random,
        Steering,
        Flocking
    }

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            movementAlgorithm = MovementAlgorithm.Random;
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            movementAlgorithm = MovementAlgorithm.Steering;
        }
        else
        {
            movementAlgorithm = MovementAlgorithm.Flocking;
        }
        
        if (agent.remainingDistance <= agent.stoppingDistance + additionalRemainingDistance)
        {
            Vector3 destination = Vector3.zero;
            
            switch (movementAlgorithm)
            {
                case MovementAlgorithm.Steering:
                    //destination =
                    break;
                case MovementAlgorithm.Flocking:
                    //destination =
                    break;
                default:
                    destination = Helpers.PickRandomDestination(Vector3.zero, movementRange);
                    break;
            }
            
            agent.SetDestination(destination);
        }
    }
}