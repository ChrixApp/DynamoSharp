using System.Reflection;
using System.Collections.Concurrent;

namespace DynamoSharp.Converters.Objects;

public static class ReflectionUtils
{
    private static readonly ConcurrentDictionary<(Type, string), PropertyInfo?> PropertyCache = new();
    private static readonly ConcurrentDictionary<(Type, string), FieldInfo?> BackingFieldCache = new();
    private static readonly ConcurrentDictionary<(Type, string), MethodInfo?> GetterMethodCache = new();

    private interface ISetValueHandler
    {
        bool HandleSetValue(object? obj, Type entityType, string propertyName, object? value);
        ISetValueHandler? Next { get; set; }
    }

    private sealed class InheritanceSetterHandler : ISetValueHandler
    {
        public ISetValueHandler? Next { get; set; }

        public bool HandleSetValue(object? obj, Type entityType, string propertyName, object? value)
        {
            Type? entityTypeTemp = entityType;
            while (entityTypeTemp != null)
            {
                var propertyInfo = PropertyCache.GetOrAdd((entityTypeTemp, propertyName),
                    key => key.Item1.GetProperty(key.Item2, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public));

                var setMethod = propertyInfo?.GetSetMethod(true);
                if (setMethod != null)
                {
                    setMethod.Invoke(obj, new object?[] { value });
                    return true;
                }
                entityTypeTemp = entityTypeTemp.BaseType;
            }
            return Next?.HandleSetValue(obj, entityType, propertyName, value) ?? false;
        }
    }

    private sealed class BackingFieldSetterHandler : ISetValueHandler
    {
        public ISetValueHandler? Next { get; set; }

        public bool HandleSetValue(object? obj, Type entityType, string propertyName, object? value)
        {
            var propertyInfo = PropertyCache.GetOrAdd((entityType, propertyName),
                key => key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance));
            var getMethod = propertyInfo?.GetGetMethod();

            var backingField = BackingFieldCache.GetOrAdd((entityType, propertyName),
                key => GetBackingFieldFromGetterMethod(getMethod, entityType));

            if (backingField != null)
            {
                backingField.SetValue(obj, value);
                return true;
            }
            return Next?.HandleSetValue(obj, entityType, propertyName, value) ?? false;
        }
    }

    private sealed class GetterChainSetterHandler : ISetValueHandler
    {
        public ISetValueHandler? Next { get; set; }

        public bool HandleSetValue(object? obj, Type entityType, string propertyName, object? value)
        {
            var propertyInfo = PropertyCache.GetOrAdd((entityType, propertyName),
                key => key.Item1.GetProperty(key.Item2, BindingFlags.Public | BindingFlags.Instance));
            var getMethod = propertyInfo?.GetGetMethod();

            var getterMethod = GetterMethodCache.GetOrAdd((entityType, propertyName),
                key => GetCalledGetterMethodFromGetMethod(getMethod, entityType));
            var backingField = BackingFieldCache.GetOrAdd((entityType, propertyName + "_getter"),
                key => GetBackingFieldFromGetterMethod(getterMethod, entityType));

            if (backingField != null)
            {
                backingField.SetValue(obj, value);
                return true;
            }
            return false;
        }

        private static MethodInfo? GetCalledGetterMethodFromGetMethod(MethodInfo? getMethod, Type entityType)
        {
            byte[]? getterIL = getMethod?.GetMethodBody()?.GetILAsByteArray();

            for (int i = 0; i < getterIL?.Length; i++)
            {
                if (getterIL[i] == 0x28 && i + 4 <= getterIL.Length)
                {
                    int methodToken = BitConverter.ToInt32(getterIL, i + 1);
                    MethodInfo? calledMethod = getMethod?.Module?.ResolveMethod(methodToken) as MethodInfo;

                    if (calledMethod != null && calledMethod.DeclaringType == entityType)
                    {
                        return calledMethod;
                    }
                }
            }

            return null;
        }
    }

    private static readonly ISetValueHandler SetValueChain =
        new InheritanceSetterHandler
        {
            Next = new BackingFieldSetterHandler
            {
                Next = new GetterChainSetterHandler()
            }
        };

    public static void SetValue(object? obj, Type entityType, string propertyName, object? value)
    {
        if (!SetValueChain.HandleSetValue(obj, entityType, propertyName, value))
        {
            throw new InvalidOperationException($"Could not set the value of property '{propertyName}' on type '{entityType.FullName}'.");
        }
    }

    private static FieldInfo? GetBackingFieldFromGetterMethod(MethodInfo? getterMethod, Type entityType)
    {
        byte[]? getterIL = getterMethod?.GetMethodBody()?.GetILAsByteArray();
        FieldInfo? fieldInfo = null;

        for (int i = 0; i < getterIL?.Length; i++)
        {
            if (getterIL[i] == 0x7B && i + 4 <= getterIL.Length)
            {
                int token = BitConverter.ToInt32(getterIL, i + 1);
                fieldInfo = getterMethod?.Module?.ResolveField(token);

                if (fieldInfo != null && fieldInfo.DeclaringType == entityType)
                {
                    return fieldInfo;
                }
            }
        }

        return null;
    }
}
