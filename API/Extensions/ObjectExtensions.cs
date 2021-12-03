using System.Text.Json;

namespace API.Extensions
{
    public static class ObjectExtensions
    {
        private static JsonSerializerOptions options = new JsonSerializerOptions 
        { 
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        public static string SerializeCamelCase(this object obj)
        {
            return JsonSerializer.Serialize(obj, options);
        }
    }
}
