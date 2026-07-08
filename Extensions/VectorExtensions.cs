using UnityEngine;

public static class VectorExtensions
{
    public static Vector2 Add(this Vector2 v1, Vector3 v2)
    {
        return new Vector2(v1.x + v2.x, v1.y + v2.y);
    }

    public static Vector2 Subtract(this Vector2 v1, Vector3 v2)
    {
        return new Vector2(v1.x - v2.x, v1.y - v2.y);
    }
}