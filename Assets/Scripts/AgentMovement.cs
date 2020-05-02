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

    private RandomMovement randomMovement;
    private ObstacleAvoidance obstacleAvoidance;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        
        randomMovement = new RandomMovement();
        obstacleAvoidance = new ObstacleAvoidance();
    }
    
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Random");
            movementAlgorithm = MovementAlgorithm.Random;
            agent.enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.S))
        {
            Debug.Log("Steering");
            movementAlgorithm = MovementAlgorithm.Steering;
            agent.enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.F))
        {
            Debug.Log("Flocking");
            movementAlgorithm = MovementAlgorithm.Flocking;
            agent.enabled = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape))
        {
            Debug.Log("Stop");
            agent.enabled = false;
        }

        if (!agent.enabled)
        {
            return;
        }
        
        if (agent.remainingDistance <= agent.stoppingDistance + additionalRemainingDistance)
        {
            Vector3 destination = Vector3.zero;
            
            switch (movementAlgorithm)
            {
                case MovementAlgorithm.Steering:
                    destination = obstacleAvoidance.GetSteering(agent).HasValue ?
                                  obstacleAvoidance.GetSteering(agent).Value.linear :
                                  randomMovement.PickRandomDestination(Vector3.zero, movementRange);
                    break;
                case MovementAlgorithm.Flocking:
                    //destination =
                    break;
                default:
                    destination = randomMovement.PickRandomDestination(Vector3.zero, movementRange);
                    break;
            }
            
            agent.velocity = destination;
        }
    }
}