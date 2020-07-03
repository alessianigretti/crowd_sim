// Thanks to https://www.youtube.com/watch?v=XqXSGSKc8NU

using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class ComparerByX : Comparer<NavMeshAgent>
{
    public override int Compare(NavMeshAgent first, NavMeshAgent second)
    {
        if (first.transform.position.x.CompareTo(second.transform.position.x) != 0)
        {
            return first.transform.position.x.CompareTo(second.transform.position.x);
        }
        else if (first.transform.position.y.CompareTo(second.transform.position.y) != 0)
        {
            return first.transform.position.y.CompareTo(second.transform.position.y);
        }
        else
        {
            return 0;
        }
    }
}

public class ComparerByY : Comparer<NavMeshAgent>
{
    public override int Compare(NavMeshAgent first, NavMeshAgent second)
    {
        if (first.transform.position.y.CompareTo(second.transform.position.y) != 0)
        {
            return first.transform.position.y.CompareTo(second.transform.position.y);
        }
        else if (first.transform.position.x.CompareTo(second.transform.position.x) != 0)
        {
            return first.transform.position.x.CompareTo(second.transform.position.x);
        }
        else
        {
            return 0;
        }
    }
}

public class KDTree
{
    public NavMeshAgent root;
    public KDTree leftTree;
    public KDTree rightTree;

    public KDTree(NavMeshAgent root, KDTree leftTree, KDTree rightTree)
    {
        this.root = root;
        this.leftTree = leftTree;
        this.rightTree = rightTree;
    }
}

public class KDTreeBuilder
{
    public int k = 6;
    
    private List<Neighbour> nearestNeighbours = new List<Neighbour>();

    public struct Neighbour
    {
        public NavMeshAgent Agent;
        public float Distance;
    }
    
    public KDTree BuildKDTree(List<NavMeshAgent> agents, int depth = 0)
    {
        var n = agents.Count - 1;

        if (n <= 0)
        {
            return null;
        }

        if (n == 1)
        {
            // leaf
            return new KDTree(agents[0], null, null);
        }

        var splittingAxis = depth % k;

        var sortedPoints = Sort(agents, splittingAxis);

        var half = (int)Mathf.Floor(n * 0.5f);
        var midNode = sortedPoints[half];
        var leftTree = BuildKDTree(GetRangeBetweenIndicesInclusive(sortedPoints, 0, half - 1), depth + 1);
        var rightTree = BuildKDTree(GetRangeBetweenIndicesInclusive(sortedPoints, half + 1, sortedPoints.Count - 1), depth + 1);
        
        return new KDTree(midNode, leftTree, rightTree);
    }

    public List<Neighbour> NearestNeighbours(int n, KDTree tree, NavMeshAgent agent)
    {
        nearestNeighbours.Clear();
        
        ComputeAllDistances(tree, agent);
        
        nearestNeighbours.Sort((x, y) => x.Distance.CompareTo(y.Distance));

        return nearestNeighbours.GetRange(0, n);
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

    private void ComputeAllDistances(KDTree tree, NavMeshAgent agent)
    {
        if (tree.leftTree != null)
        {
            ComputeAllDistances(tree.leftTree, agent);
        }

        if (tree.rightTree != null)
        {
            ComputeAllDistances(tree.rightTree, agent);
        }
        
        Vector2 rootVec = new Vector2(tree.root.transform.position.x, tree.root.transform.position.y);
        Vector2 pointVec = new Vector2(agent.transform.position.x, agent.transform.position.y);
        nearestNeighbours.Add(new Neighbour {Agent = tree.root, Distance = Vector2.Distance(rootVec, pointVec)});
    }

    private List<NavMeshAgent> Sort(List<NavMeshAgent> array, int key)
    {
        if (key == 0)
        {
            array.Sort(new ComparerByX());
        }
        else
        {
            array.Sort(new ComparerByY());
        }

        return array;
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

    private List<NavMeshAgent> GetRangeBetweenIndicesInclusive(List<NavMeshAgent> agents, int startIndex, int endIndex)
    {
        List<NavMeshAgent> pointsRange = new List<NavMeshAgent>();
        
        for (int i = 0; i < agents.Count; i++)
        {
            if (i >= startIndex && i <= endIndex)
            {
                pointsRange.Add(agents[i]);
            }
        }

        return pointsRange;
    }
}