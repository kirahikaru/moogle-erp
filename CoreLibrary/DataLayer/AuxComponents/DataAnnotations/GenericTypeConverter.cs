using Newtonsoft.Json;
//using System.Text.Json.Serialization;

namespace DataLayer.AuxComponents.DataAnnotations;
public class GenericTypeConverter : JsonConverter
{
    readonly Type GenericTypeDefinition;

    public GenericTypeConverter(Type genericTypeDefinition)
    {
        ArgumentNullException.ThrowIfNull(genericTypeDefinition, nameof(genericTypeDefinition));
        this.GenericTypeDefinition = genericTypeDefinition;
    }

    public override bool CanConvert(Type typeToConvert)
    {
        if (typeToConvert == null || !typeToConvert.IsGenericType)
            return false;

        var type = typeToConvert;

        if (!type.IsGenericTypeDefinition)
            type = type.GetGenericTypeDefinition();

        return type == GenericTypeDefinition;
    }

    public override bool CanWrite { get { return false; } }

    public override object ReadJson(JsonReader reader, Type objectType, object? existingValue, JsonSerializer serializer)
    {
        ArgumentNullException.ThrowIfNull(objectType, nameof(objectType));
        ArgumentNullException.ThrowIfNull(serializer, nameof(serializer));

        return serializer.Deserialize(reader, MakeGenericType(objectType))!;
    }

    public override void WriteJson(JsonWriter writer, object? value, JsonSerializer? serializer)
    {
        throw new NotImplementedException();
    }

    Type MakeGenericType(Type objectType)
    {
        if (!GenericTypeDefinition.IsGenericTypeDefinition)
            return GenericTypeDefinition;
        try
        {
            var parameters = objectType.GetGenericArguments();
            return GenericTypeDefinition.MakeGenericType(parameters);
        }
        catch (Exception ex)
        {
            // Wrap the reflection exception in something more useful.
            throw new JsonSerializationException(string.Format("Unable to construct concrete type from generic {0} and desired type {1}", GenericTypeDefinition, objectType), ex);
        }
    }
}