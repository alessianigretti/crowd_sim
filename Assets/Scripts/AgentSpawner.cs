using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Initialises scene and begins navigation
/// </summary>
public class AgentSpawner : MonoBehaviour
{
    public GameObject agentPrefab;
    public int amount = 30;
    public float spawnRange = 30f;
    public int nNearest = 5;

    private NavMeshAgent mainAgent;
    private List<NavMeshAgent> agents = new List<NavMeshAgent>();
    
    void OnEnable()
    {
        mainAgent = GameObject.Find("Agent").GetComponent<NavMeshAgent>();
        
        for (int i = 0; i < amount; i++)
        {
            var agent = Instantiate(agentPrefab, Utils.PickRandomDestination(Vector3.zero, spawnRange), Quaternion.identity);
            agents.Add(agent.GetComponent<NavMeshAgent>());
        }
    }

    void Update()
    {
        // Build KD-Tree and use it to identify N nearest neighbours
        var tree = KDTreeBuilder.BuildKDTree(agents);
        var nearestNeighbours = NearestNeighbour.Compute(nNearest, tree, mainAgent);
        
        // Compute VO position depending on average of velocities of colliding agents
        var resultingVelocityObstaclePosition = (mainAgent.velocity + nearestNeighbours[0].Agent.velocity) * 0.5f;
        mainAgent.transform.GetChild(0).transform.localPosition = resultingVelocityObstaclePosition;
        nearestNeighbours[0].Agent.gameObject.transform.GetChild(0).transform.localPosition = resultingVelocityObstaclePosition;
    }
}