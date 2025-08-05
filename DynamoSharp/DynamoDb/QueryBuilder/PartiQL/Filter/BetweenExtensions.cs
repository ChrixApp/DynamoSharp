namespace DynamoSharp.DynamoDb.QueryBuilder.PartiQL.Filter;

// Only use this class in filter expressions
public static class BetweenExtensions
{
    public static bool Between(this short a, short b, short c)
    {
        return true;
    }

    public static bool Between(this ushort a, ushort b, ushort c)
    {
        return true;
    }

    public static bool Between(this int a, int b, int c)
    {
        return true;
    }

    public static bool Between(this uint a, uint b, uint c)
    {
        return true;
    }

    public static bool Between(this long a, long b, long c)
    {
        return true;
    }

    public static bool Between(this ulong a, ulong b, ulong c)
    {
        return true;
    }

    public static bool Between(this float a, float b, float c)
    {
        return true;
    }

    public static bool Between(this double a, double b, double c)
    {
        return true;
    }

    public static bool Between(this decimal a, decimal b, decimal c)
    {
        return true;
    }

    public static bool Between(this string a, string b, string c)
    {
        return true;
    }
}
