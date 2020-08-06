using UnityEngine;

public static class Utils
{
    public static Vector3 PickRandomPointInCircle(Vector3 centralPosition, float range)
    {
        var randomPointInCircle = Random.insideUnitCircle * range;
        return new Vector3(centralPosition.x + randomPointInCircle.x, centralPosition.y,
            centralPosition.z + randomPointInCircle.y);
    }
}
