using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Shopping.Domain.Events;

namespace Shopping.Infrastructure.JsonSerialization
{
    // HACK: Add a temporary manual converter. .NET Core 3.1 doesn't support constructors with parameters
    public class ShoppingCartCreatedConverter : JsonConverter<ShoppingCartCreated>
    {
        public override ShoppingCartCreated Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read(); // Id property name
            reader.Read(); // Id property value
            var id = reader.GetGuid();

            // Read the rest of the object. We don't need any of this
            while (reader.Read())
            {
            }

            return new ShoppingCartCreated(id);
        }

        public override void Write(Utf8JsonWriter writer, ShoppingCartCreated value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Id",value.Id);
            writer.WriteEndObject();
        }
    }
}