// Thanks to https://www.youtube.com/watch?v=XqXSGSKc8NU

using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class Point : IComparable<Point>
{
    public float x { get; private set; }
    public float y { get; private set; }

    public Point(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public int CompareTo(Point other)
    {
        if (x.CompareTo(other.x) != 0)
        {
            return x.CompareTo(other.x);
        }
        else if (y.CompareTo(other.y) != 0)
        {
            return y.CompareTo(other.y);
        }
        else
        {
            return 0;
        }
    }
}

public class ComparerByY : Comparer<Point>
{
    public override int Compare(Point first, Point second)
    {
        if (first.y.CompareTo(second.y) != 0)
        {
            return first.y.CompareTo(second.y);
        }
        else if (first.x.CompareTo(second.x) != 0)
        {
            return first.x.CompareTo(second.x);
        }
        else
        {
            return 0;
        }
    }
}

public class KDTree
{
    public Point root;
    public KDTree leftTree;
    public KDTree rightTree;

    public KDTree(Point root, KDTree leftTree, KDTree rightTree)
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
        public Point Point;
        public float Distance;
    }
    
    public KDTree BuildKDTree(List<Point> points, int depth = 0)
    {
        var n = points.Count - 1;

        if (n <= 0)
        {
            return null;
        }

        if (n == 1)
        {
            // leaf
            return new KDTree(points[0], null, null);
        }

        var splittingAxis = depth % k;

        var sortedPoints = Sort(points, splittingAxis);

        var half = (int)Mathf.Floor(n * 0.5f);
        var midNode = sortedPoints[half];
        var leftTree = BuildKDTree(GetRangeBetweenIndicesInclusive(sortedPoints, 0, half - 1), depth + 1);
        var rightTree = BuildKDTree(GetRangeBetweenIndicesInclusive(sortedPoints, half + 1, sortedPoints.Count - 1), depth + 1);
        
        return new KDTree(midNode, leftTree, rightTree);
    }

    public List<Neighbour> NearestNeighbours(int n, KDTree tree, Point point)
    {
        nearestNeighbours.Clear();
        
        ComputeAllDistances(tree, point);
        
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

    private void ComputeAllDistances(KDTree tree, Point point)
    {
        if (tree.leftTree != null)
        {
            ComputeAllDistances(tree.leftTree, point);
        }

        if (tree.rightTree != null)
        {
            ComputeAllDistances(tree.rightTree, point);
        }
        
        Vector2 rootVec = new Vector2(tree.root.x, tree.root.y);
        Vector2 pointVec = new Vector2(point.x, point.y);
        nearestNeighbours.Add(new Neighbour {Point = tree.root, Distance = Vector2.Distance(rootVec, pointVec)});
    }

    private List<Point> Sort(List<Point> array, int key)
    {
        if (key == 0)
        {
            array.Sort();
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

    private List<Point> GetRangeBetweenIndicesInclusive(List<Point> points, int startIndex, int endIndex)
    {
        List<Point> pointsRange = new List<Point>();
        
        for (int i = 0; i < points.Count; i++)
        {
            if (i >= startIndex && i <= endIndex)
            {
                pointsRange.Add(points[i]);
            }
        }

        return pointsRange;
    }
}