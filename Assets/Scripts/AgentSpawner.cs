using System;
using System.Collections.Generic;
using UnityEditor;
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

    List<SteeringBehaviour.VelocityObstacleData> reusedAgentToVelocityVectors = new List<SteeringBehaviour.VelocityObstacleData>();
    void Update()
    {
        // Build KD-Tree and use it to identify N nearest neighbours (wip)
        var tree = KDTreeBuilder.BuildKDTree(agents);

        reusedAgentToVelocityVectors.Clear();
        for (int i = 0; i < agents.Count; i++)
        {
            var agent = agents[i];
            // Identify nNearest neighbours
            //var nearestNeighbours = NearestNeighbour.Compute(nNearest, tree, agent);
            var nearestNeighbour = NearestNeighbour.ComputeSimple(agent);

            // Collect 2 velocity vectors (left and right sides of the cone) per nearest neighbour
            // for (int i = 1; i <= nearestNeighbours.Count - 1; i++)
            // {
            //     var nearestNeighbour = nearestNeighbours[i];
            steeringBehaviour.DrawVelocityObstacles(reusedAgentToVelocityVectors, agent, nearestNeighbour.Agent);
            // }
        }

        // After having computed all velocity vectors, find all intersections and what agents they belong to, and adjust their velocities
        steeringBehaviour.DoSteering(reusedAgentToVelocityVectors);
    }

    private void OnDrawGizmos()
    {
        foreach (var agent in agents)
        {
            Gizmos.DrawWireSphere(agent.transform.position, agent.radius * steeringBehaviour.obstacleWidthMultiplier);
        }
        Gizmos.DrawWireSphere(steeringBehaviour.intersectionPoint, 1f);
    }
}