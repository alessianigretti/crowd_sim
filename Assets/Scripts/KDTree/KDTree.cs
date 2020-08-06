using UnityEngine.AI;

/// <summary>
/// Data structure holding KDTree data
/// </summary>
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

    public int GetNodeCount()
    {
        nodeCountInt = 0;
        
        int nodeCountExt = CountNodes(this);

        return nodeCountExt;
    }

    private int nodeCountInt;
    private int CountNodes(KDTree tree)
    {
        // Account for root
        nodeCountInt += 1;

        if (tree.leftTree != null)
        {
            nodeCountInt += CountNodes(tree.leftTree);
        }

        if (tree.rightTree != null)
        {
            nodeCountInt += CountNodes(tree.rightTree);
        }

        return nodeCountInt;
    }
}