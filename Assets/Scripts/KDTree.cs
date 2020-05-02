// Thanks to https://www.youtube.com/watch?v=XqXSGSKc8NU

using System;
using System.Collections.Generic;
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

        return new KDTree(sortedPoints[n / 2],
            BuildKDTree(sortedPoints.GetRange(0, (n-1) / 2), depth + 1),
            BuildKDTree(sortedPoints.GetRange((n-1) / 2 + 1, sortedPoints.Count - 1), depth + 1));
    }

    public Point NearestNeighbour(KDTree tree, Point point, int depth = 0)
    {
        if (tree == null)
        {
            return null;
        }

        var splittingAxis = depth % k;
        var pointValue = splittingAxis == 0 ? point.x : point.y;
        var rootValue = splittingAxis == 0 ? tree.root.x : tree.root.y;

        KDTree nextBranch = null;
        KDTree oppositeBranch = null;

        if (pointValue < rootValue)
        {
            nextBranch = tree.leftTree;
            oppositeBranch = tree.rightTree;
        }
        else
        {
            nextBranch = tree.rightTree;
            oppositeBranch = tree.leftTree;
        }

        var best = ClosestDistance(point,
                                        NearestNeighbour(nextBranch, point, depth + 1),
                                        tree.root);

        var distanceFromBest = Vector2.Distance(new Vector2(point.x, point.y), new Vector2(best.x, best.y));
        if (distanceFromBest > Mathf.Abs(pointValue - rootValue))
        {
            best = ClosestDistance(point,
                                NearestNeighbour(oppositeBranch, point, depth + 1),
                                best);
        }

        return best;
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

    private Point ClosestDistance(Point pivot, Point p1, Point p2)
    {
        if (p1 == null)
        {
            return p2;
        }

        if (p2 == null)
        {
            return p1;
        }

        var d1 = Vector2.Distance(new Vector2(pivot.x, pivot.y), new Vector2(p1.x, p1.y));
        var d2 = Vector2.Distance(new Vector2(pivot.x, pivot.y), new Vector2(p2.x, p2.y));

        if (d1 < d2)
        {
            return p1;
        }
        else
        {
            return p2;
        }
    }
}