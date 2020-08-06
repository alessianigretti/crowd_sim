using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Sets random destinations for agents so they keep moving
/// </summary>
public class AgentMovement : MonoBehaviour
{
    public float movementRange = 20f;
    
    private NavMeshAgent agent;
    private float additionalRemainingDistance = 0.001f;    // avoid characters getting stuck
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
    }
    
    void Update()
    {
        if (!agent.enabled)
        {
            return;
        }
        
        if (agent.remainingDistance <= agent.stoppingDistance + additionalRemainingDistance)
        {
            Vector3 destination = Utils.PickRandomDestination(Vector3.zero, movementRange);
            
            agent.SetDestination(destination);
        }
    }
}