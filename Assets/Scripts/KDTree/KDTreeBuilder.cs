// Thanks to https://www.youtube.com/watch?v=XqXSGSKc8NU

using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public static class KDTreeBuilder
{
    public static int k = 6;
    
    public static KDTree BuildKDTree(List<NavMeshAgent> agents, int depth = 0)
    {
        var n = agents.Count;

        if (n <= 0)
        {
            // Empty list
            return null;
        }

        if (n == 1)
        {
            // Leaf node
            return new KDTree(agents[0], null, null);
        }

        // Alternate sorting by X and Z depending on current tree depth
        var splittingAxis = depth % k;

        // Sort agents by distance
        var sortedPoints = Sort(agents, splittingAxis);
        
        // Identify mid index and split tree into left and right
        var half = (int)Mathf.Floor(n * 0.5f);
        var midNode = sortedPoints[half];
        var leftTree = BuildKDTree(sortedPoints.GetRange(0, half), depth + 1);
        var rightTree = BuildKDTree(sortedPoints.GetRange(half, sortedPoints.Count - half), depth + 1);
        
        return new KDTree(midNode, leftTree, rightTree);
    }        

    // private Point NearestNeighbour(KDTree tree, Point point, int depth = 0)
    // {        
    //     if (tree == null)
    //     {
    //         return null;
    //     }
    //
    //     var splittingAxis = depth % k;
    //     var pointValue = splittingAxis == 0 ? point.x : point.y;
    //     var rootValue = splittingAxis == 0 ? tree.root.x : tree.root.y;
    //
    //     KDTree nextBranch = null;
    //     KDTree oppositeBranch = null;
    //
    //     if (pointValue < rootValue)
    //     {
    //         nextBranch = tree.leftTree;
    //         oppositeBranch = tree.rightTree;
    //     }
    //     else
    //     {
    //         nextBranch = tree.rightTree;
    //         oppositeBranch = tree.leftTree;
    //     }
    //
    //     var best = ClosestDistance(point,
    //                                     NearestNeighbour(nextBranch, point, depth + 1),
    //                                     tree.root);
    //
    //     var distanceFromBest = Vector2.Distance(new Vector2(point.x, point.y), new Vector2(best.x, best.y));
    //     if (distanceFromBest > Mathf.Abs(pointValue - rootValue))
    //     {
    //         best = ClosestDistance(point,
    //                             NearestNeighbour(oppositeBranch, point, depth + 1),
    //                             best);
    //     }
    //
    //     return best;
    // }

    private static List<NavMeshAgent> Sort(List<NavMeshAgent> agents, int key)
    {
        if (key == 0)
        {
            agents.Sort(new ComparerByX());
        }
        else
        {
            agents.Sort(new ComparerByZ());
        }

        return agents;
    }

    // private Point ClosestDistance(Point pivot, Point p1, Point p2)
    // {
    //     if (p1 == null)
    //     {
    //         return p2;
    //     }
    //
    //     if (p2 == null)
    //     {
    //         return p1;
    //     }
    //
    //     var d1 = Vector2.Distance(new Vector2(pivot.x, pivot.y), new Vector2(p1.x, p1.y));
    //     var d2 = Vector2.Distance(new Vector2(pivot.x, pivot.y), new Vector2(p2.x, p2.y));
    //
    //     if (d1 < d2)
    //     {
    //         return p1;
    //     }
    //     else
    //     {
    //         return p2;
    //     }
    // }
}