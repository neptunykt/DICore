using System.Reflection;
using System.Runtime.CompilerServices;

namespace DICore3.Abstractions.Common.src.Extensions.ParameterDefaultValue;

internal static partial class ParameterDefaultValue
{
    public static bool TryGetDefaultValue(ParameterInfo parameter, out object? defaultValue)
    {
        bool hasDefaultValue = CheckHasDefaultValue(parameter, out bool tryToGetDefaultValue);
        defaultValue = null;

        if (hasDefaultValue)
        {
            if (tryToGetDefaultValue)
            {
                defaultValue = parameter.DefaultValue;
            }

            bool isNullableParameterType = parameter.ParameterType.IsGenericType &&
                                           parameter.ParameterType.GetGenericTypeDefinition() == typeof(Nullable<>);

            // Workaround for https://github.com/dotnet/runtime/issues/18599
            if (defaultValue == null && parameter.ParameterType.IsValueType
                                     && !isNullableParameterType) // Nullable types should be left null
            {
                defaultValue = CreateValueType(parameter.ParameterType);
            }

             
            static object? CreateValueType(Type t) =>

                RuntimeHelpers.GetUninitializedObject(t);

            // Handle nullable enums
            if (defaultValue != null && isNullableParameterType)
            {
                Type? underlyingType = Nullable.GetUnderlyingType(parameter.ParameterType);
                if (underlyingType != null && underlyingType.IsEnum)
                {
                    defaultValue = Enum.ToObject(underlyingType, defaultValue);
                }
            }
        }

        return hasDefaultValue;
    }
    
    public static bool CheckHasDefaultValue(ParameterInfo parameter, out bool tryToGetDefaultValue)
    {
        tryToGetDefaultValue = true;
        return parameter.HasDefaultValue;
    }
}