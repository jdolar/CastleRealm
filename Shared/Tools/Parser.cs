using Microsoft.Extensions.Logging;
using Shared.Requests;
using System.Reflection;
namespace Shared.Tools;
public sealed class Parser
{
    private readonly ILogger _logger;
    private readonly RandomGenerator _randomGenerator = new();
    public Parser(ILogger logger)
    {
        _logger = logger;
    }
    public Type? GetDtoType(string operationId)
    {
        Type? interfaceType = typeof(IRequest);
        Assembly? assembly = interfaceType.Assembly;

        Type? handlerType = assembly.GetTypes()
            .Where(t => t.IsClass && !t.IsAbstract && !t.IsInterface)
            .FirstOrDefault(t =>
                interfaceType.IsAssignableFrom(t) &&
                t.Name.Equals(operationId, StringComparison.OrdinalIgnoreCase));

        return handlerType;
    }
    public object GetPayLoad(Type type)
    {
        if (type == typeof(string)) return _randomGenerator.NextString(5);
        if (type == typeof(int)) return 1;
        if (type == typeof(DateTime)) return DateTime.UtcNow;

        object? instance = Activator.CreateInstance(type)!;
        foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
        {
            if (!prop.CanWrite) continue;

            object? value = GetDefaultValue(prop.PropertyType);
            prop.SetValue(instance, value);
        }

        return instance;
    }
    private object? GetDefaultValue(Type type)
    {
        if (type == typeof(string)) return _randomGenerator.NextString(10);
        if (type == typeof(int)) return 1;
        if (type == typeof(DateTime)) return DateTime.UtcNow;
        if (type == typeof(bool)) return true;
        if (type.IsEnum) return Enum.GetValues(type).GetValue(0);
        if (type.IsClass && type != typeof(string)) return GetPayLoad(type);
        if (Nullable.GetUnderlyingType(type) is { } innerType)
            return GetDefaultValue(innerType);

        return null;
    }
}