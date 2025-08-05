namespace DynamoSharp.DynamoDb.QueryBuilder.PartiQL.Filter;

// Only use this class in filter expressions
public static class InExtensions
{
    public static bool In(this short a, params short[] list)
    {
        return true;
    }

    public static bool In(this ushort a, params ushort[] list)
    {
        return true;
    }

    public static bool In(this int a, params int[] list)
    {
        return true;
    }

    public static bool In(this uint a, params uint[] list)
    {
        return true;
    }

    public static bool In(this long a, params long[] list)
    {
        return true;
    }

    public static bool In(this ulong a, params ulong[] list)
    {
        return true;
    }

    public static bool In(this float a, params float[] list)
    {
        return true;
    }

    public static bool In(this double a, params double[] list)
    {
        return true;
    }

    public static bool In(this decimal a, params decimal[] list)
    {
        return true;
    }

    public static bool In(this string a, params string[] list)
    {
        return true;
    }
}
