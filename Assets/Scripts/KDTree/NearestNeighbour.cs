using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class NearestNeighbour
{
    public struct Neighbour
    {
        public NavMeshAgent Agent;
        public float Distance;
    }
    
    private static List<Neighbour> nearestNeighbours = new List<Neighbour>();

    public static Neighbour ComputeSimple(NavMeshAgent agent)
    {
        var allAgents = GameObject.FindGameObjectsWithTag("Agent");
        var closestAgent = new Neighbour {Distance = Mathf.Infinity};
        foreach (var otherAgent in allAgents)
        {
            var otherAgentComponent = otherAgent.GetComponent<NavMeshAgent>();
            if (otherAgentComponent == agent)
            {
                continue;
            }
            
            var distance = Vector3.Distance(agent.transform.position, otherAgent.transform.position);
            if (distance < closestAgent.Distance)
            {
                closestAgent.Agent = otherAgentComponent;
                closestAgent.Distance = distance;
            }
        }

        return closestAgent;
    }
    
    public static List<Neighbour> Compute(int n, KDTree tree, NavMeshAgent agent)
    {
        nearestNeighbours.Clear();
        
        ComputeAllDistances(tree, agent);
        
        // Sort neighbours by distance from the root
        nearestNeighbours.Sort((neighbour1, neighbour2) => neighbour1.Distance.CompareTo(neighbour2.Distance));

        // Get N-nearest neighbours
        return nearestNeighbours.GetRange(0, n);
    }
    
    private static void ComputeAllDistances(KDTree tree, NavMeshAgent agent)
    {
        if (tree.leftTree != null)
        {
            ComputeAllDistances(tree.leftTree, agent);
        }

        if (tree.rightTree != null)
        {
            ComputeAllDistances(tree.rightTree, agent);
        }
        
        // Store distance between root and agent
        Vector2 rootVec = new Vector2(tree.root.transform.position.x, tree.root.transform.position.z);
        Vector2 pointVec = new Vector2(agent.transform.position.x, agent.transform.position.z);
        
        // todo: hacky - this shouldn't be needed and might be covering a bug
        var distance = Vector2.Distance(rootVec, pointVec);
        if (distance > 0)
        {
            nearestNeighbours.Add(new Neighbour {Agent = tree.root, Distance = Vector2.Distance(rootVec, pointVec)});
        }
    }
}
