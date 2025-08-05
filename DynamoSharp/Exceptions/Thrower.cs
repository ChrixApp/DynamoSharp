using System.Diagnostics.CodeAnalysis;

namespace DynamoSharp.Exceptions;

public static class Thrower
{
    public static void ThrowIfNull<TException>([NotNull] object? argument, string? message = null) where TException : Exception
    {
        if (argument is null)
        {
            var exceptionType = typeof(TException);
            var constructor = exceptionType.GetConstructor(new[] { typeof(string) });

            if (constructor is not null)
            {
                var exception = (TException)constructor.Invoke(new object?[] { message });
                throw exception;
            }
            else
            {
                throw new InvalidOperationException($"The exception type {exceptionType.FullName} does not have a constructor that accepts a single string argument.");
            }
        }
    }
}
