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
        nearestNeighbours.Add(new Neighbour {Agent = tree.root, Distance = Vector2.Distance(rootVec, pointVec)});
    }
}
