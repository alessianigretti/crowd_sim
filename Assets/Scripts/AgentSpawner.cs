using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Initialises scene and begins navigation
/// </summary>
public class AgentSpawner : MonoBehaviour
{
    public enum CrowdSimType
    {
        RandomCrowd,
        EmergentLanes
    }
    public CrowdSimType crowdSimType;
    public GameObject agentPrefab;
    public GameObject agentGoingLeftPrefab;
    public GameObject agentGoingRightPrefab;
    public int amount = 30;
    public float spawnRange = 30f;
    public int nNearest = 5;

    private List<NavMeshAgent> agents = new List<NavMeshAgent>();
    private SteeringBehaviour steeringBehaviour = new SteeringBehaviour();
    
    void OnEnable()
    {
        if (crowdSimType == CrowdSimType.RandomCrowd)
        {
            for (int i = 0; i < amount; i++)
            {
                var agent = Instantiate(agentPrefab, Utils.PickRandomPointInCircle(Vector3.zero, spawnRange), Quaternion.identity);
                agents.Add(agent.GetComponent<NavMeshAgent>());
            }
        }
        else if (crowdSimType == CrowdSimType.EmergentLanes)
        {
            for (int i = 0; i < amount / 2; i++)
            {
                var agent = Instantiate(agentGoingLeftPrefab, Utils.PickRandomPointInCircle(new Vector3(40, 0, 0), spawnRange), Quaternion.identity);
                agents.Add(agent.GetComponent<NavMeshAgent>());
            }
        
            for (int i = 0; i < amount / 2; i++)
            {
                var agent = Instantiate(agentGoingRightPrefab, Utils.PickRandomPointInCircle(new Vector3(-40, 0, 0), spawnRange), Quaternion.identity);
                agents.Add(agent.GetComponent<NavMeshAgent>());
            }
        }
    }

    void Update()
    {
        // Build KD-Tree and use it to identify N nearest neighbours
        var tree = KDTreeBuilder.BuildKDTree(agents);

        foreach (var agent in agents)
        {
            var nearestNeighbours = NearestNeighbour.Compute(nNearest, tree, agent);
            var velocityVectors = new List<Ray>();
            
            // Orientate VO towards nearest neighbour
            for (int i = 0; i < nearestNeighbours.Count - 1; i++)
            {
                var nearestNeighbour = nearestNeighbours[i];
                
                velocityVectors.AddRange(steeringBehaviour.DrawVelocityObstacles(agent, nearestNeighbour.Agent));
            }

            steeringBehaviour.DoSteering(velocityVectors);
        }
    }
}