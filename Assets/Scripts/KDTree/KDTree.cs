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
}