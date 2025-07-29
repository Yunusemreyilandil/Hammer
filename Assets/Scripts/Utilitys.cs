using UnityEngine;

public static class Utilitys
{
    public static Vector3 SetY(this Vector3 vec, float y)
    {
        return new Vector3(vec.x, y, vec.z);
    }

    public static Vector3 Multiply(this Vector3 vec, float x, float y, float z)
    {
        return new Vector3(vec.x * x, vec.y * y, vec.z * z);
    }

    public static float Remap(this float f, float fromMin, float fromMax, float toMin, float toMax)
    {
        float t = (f - fromMin) / (fromMax - fromMin);
        return Mathf.LerpUnclamped(toMin, toMax, t);
    }
}