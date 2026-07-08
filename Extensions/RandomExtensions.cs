using System.Collections.Generic;
using UnityEngine;

public static class MyRandom
{
    /// <returns>-1 or 1</returns>
    public static int Sign()
    {
        return Random.Range(0, 2) * 2 - 1;
    }

    public static T LotteryT<T>(List<T> list)
    {
        var r = Random.Range(0, list.Count);
        return list[r];
    }

    public static Vector3 RandomVec(Vector3 min, Vector3 max)
    {
        return new Vector3(
            Random.Range(min.x, max.x),
            Random.Range(min.y, max.y),
            Random.Range(min.z, max.z)
        );
    }
}