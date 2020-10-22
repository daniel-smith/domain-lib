using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using Shopping.Domain.Events;

namespace Shopping.Infrastructure.JsonSerialization
{
    // HACK: Add a temporary manual converter. .NET Core 3.1 doesn't support constructors with parameters
    public class ItemAddedToShoppingCartConverter : JsonConverter<ItemAddedToShoppingCart>
    {
        public override ItemAddedToShoppingCart Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            reader.Read(); // Id property name
            reader.Read(); // Id property value
            var id = reader.GetGuid();

            reader.Read(); // Item property name
            reader.Read(); // Item property value
            var item = reader.GetString();

            // Read the rest of the object. We don't need any of this
            while (reader.Read())
            {
            }

            return new ItemAddedToShoppingCart(id, item);
        }

        public override void Write(Utf8JsonWriter writer, ItemAddedToShoppingCart value, JsonSerializerOptions options)
        {
            writer.WriteStartObject();
            writer.WriteString("Id",value.Id);
            writer.WriteString("Item", value.Item);
            writer.WriteEndObject();
        }
    }
}