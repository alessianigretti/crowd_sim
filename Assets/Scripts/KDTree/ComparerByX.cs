using System.Collections.Generic;
using UnityEngine.AI;

public class ComparerByX : Comparer<NavMeshAgent>
{
    public override int Compare(NavMeshAgent first, NavMeshAgent second)
    {
        if (first.transform.position.x.CompareTo(second.transform.position.x) != 0)
        {
            return first.transform.position.x.CompareTo(second.transform.position.x);
        }
        else if (first.transform.position.z.CompareTo(second.transform.position.z) != 0)
        {
            return first.transform.position.z.CompareTo(second.transform.position.z);
        }
        else
        {
            return 0;
        }
    }
}