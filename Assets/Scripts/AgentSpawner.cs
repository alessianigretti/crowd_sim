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
    // public int nNearest = 5;

    private List<NavMeshAgent> agents = new List<NavMeshAgent>();
    private SteeringBehaviour steeringBehaviour = new SteeringBehaviour();
    private List<SteeringBehaviour.VelocityObstacleData> reusedAgentToVelocityObstacles = new List<SteeringBehaviour.VelocityObstacleData>();
    
    void OnEnable()
    {
        // Generate agents depending on CrowdSimType selected (agents prefabs have different movement behaviours)
        if (crowdSimType == CrowdSimType.RandomCrowd)
        {
            for (int i = 0; i < amount; i++)
            {
                var agent = Instantiate(agentPrefab, Utils.PickRandomPointInCircle(Vector3.zero, spawnRange), Quaternion.identity);
                agent.name = $"Agent {i}";
                agents.Add(agent.GetComponent<NavMeshAgent>());
            }
        }
        else if (crowdSimType == CrowdSimType.EmergentLanes)
        {
            for (int i = 0; i < amount / 2; i++)
            {
                var agent = Instantiate(agentGoingLeftPrefab, Utils.PickRandomPointInCircle(new Vector3(20, 0, 0), spawnRange), Quaternion.identity);
                agent.name = $"AgentGoingLeft {i}";
                agents.Add(agent.GetComponent<NavMeshAgent>());
            }
        
            for (int i = 0; i < amount / 2; i++)
            {
                var agent = Instantiate(agentGoingRightPrefab, Utils.PickRandomPointInCircle(new Vector3(-20, 0, 0), spawnRange), Quaternion.identity);
                agent.name = $"AgentGoingRight {i}";
                agents.Add(agent.GetComponent<NavMeshAgent>());
            }
        }
    }

    void Update()
    {
        // TODO: build KD-Tree and use it to identify N nearest neighbours
        // var tree = KDTreeBuilder.BuildKDTree(agents);

        reusedAgentToVelocityObstacles.Clear();
        foreach (var agent in agents)
        {
            // Identify nNearest neighbours (TODO: use KD-Tree)
            // var nearestNeighbours = NearestNeighbour.Compute(nNearest, tree, agent);
            var nearestNeighbour = NearestNeighbour.ComputeUnoptimized(agent);

            // Compute velocity obstacles representing velocities that would lead to a collision
            steeringBehaviour.ComputeVelocityObstacles(reusedAgentToVelocityObstacles, agent, nearestNeighbour.Agent);
        }

        // After having computed all velocity obstacles for all agents, adjust their velocities
        steeringBehaviour.DoSteering(reusedAgentToVelocityObstacles);
    }
}