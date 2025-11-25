using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using System.Reflection;

namespace DataLayer.AuxComponents.DataAnnotations;

[AttributeUsage(AttributeTargets.Property)]
public class GenericPropertyTypeNameAttribute : Attribute
{
    public int TypeParameterPosition { get; }

    public GenericPropertyTypeNameAttribute(int position)
    {
        TypeParameterPosition = position;
    }
}

public class GenericTypeNameContractResolver : DefaultContractResolver
{
    protected override JsonProperty CreateProperty(MemberInfo member, MemberSerialization memberSerialization)
    {
        if (member == null)
            throw new ArgumentNullException(nameof(member), "Parameter null.");

        var prop = base.CreateProperty(member, memberSerialization);
        var attr = member.GetCustomAttribute<GenericPropertyTypeNameAttribute>();
        if (attr != null)
        {
            var type = member.DeclaringType;
            if (!type!.IsGenericType)
                throw new InvalidOperationException($"{type} is not a generic type");
            if (type.IsGenericTypeDefinition)
                throw new InvalidOperationException($"{type} is a generic type definition, it must be a constructed generic type");
            var typeArgs = type.GetGenericArguments();
            if (attr.TypeParameterPosition >= typeArgs.Length)
                throw new ArgumentException($"Can't get type argument at position {attr.TypeParameterPosition}; {type} has only {typeArgs.Length} type arguments");
            prop.PropertyName = typeArgs[attr.TypeParameterPosition].Name;
        }
        return prop;
    }
}