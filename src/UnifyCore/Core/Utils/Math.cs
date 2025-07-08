namespace UnifyCore.Core.Utils;

public class MathUtils
{
    public static float Lerp(float a, float b, float t)
    {
        return a * (1 - t) + b * t;
    }
}