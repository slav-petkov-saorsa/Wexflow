using System.Net.Http;
using System.Text;
using System.Text.Json;

namespace Wexflow.CommandLineParserClient.Common.Contents
{
    public sealed class JsonContent : StringContent
    {
        public static readonly JsonContent Empty = new JsonContent(new { });
        
        public JsonContent(object payload) : base(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json")
        {
            
        }
    }
}
