using System.Diagnostics.CodeAnalysis;

namespace DynamoSharp.Exceptions;

public static class Thrower
{
    public static void ThrowIfNull<TException>([NotNull] object? argument, string? message = null) where TException : Exception
    {
        if (argument is not null) return;

        var exceptionType = typeof(TException);

        if (message is null)
        {
            var parameterlessCtor = exceptionType.GetConstructor(Type.EmptyTypes);
            if (parameterlessCtor is not null) throw (TException)parameterlessCtor.Invoke(null);
        }

        var constructor = exceptionType.GetConstructor(new[] { typeof(string) });

        if (constructor is not null) throw (TException)constructor.Invoke(new object?[] { message });

        throw new InvalidOperationException($"The exception type {exceptionType.FullName} does not have a constructor that accepts a single string argument.");
    }
}
