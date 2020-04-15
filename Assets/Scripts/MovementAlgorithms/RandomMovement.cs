using UnityEngine;

public class RandomMovement
{
    public Vector3 PickRandomDestination(Vector3 centralPosition, float range)
    {
        var randomPointInCircle = Random.insideUnitCircle * range;
        return new Vector3(centralPosition.x + randomPointInCircle.x, centralPosition.y,
            centralPosition.z + randomPointInCircle.y);
    }
}