using System;
using Bekk.Pact.Common.Extensions;
using Bekk.Pact.Consumer.Contracts;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace Bekk.Pact.Consumer.Rendering
{
    class BodyRenderer
    {
        private readonly IPactResponseDefinition pact;
        private object body;

        public BodyRenderer(IPactResponseDefinition pact)
        {
            this.pact = pact;
        }

        public JObject Render()
        {
            body = pact?.ResponseBody;
            if (body == null) return null;
            var contentType = HeaderExtensions.ContentType(pact.ResponseHeaders);
            if (contentType != null && contentType != "application-json; charset=utf-8") throw new NotSupportedException(
                $"Only content-type: application-json; charset=utf-8 is supported. Found: {contentType}");
            switch (body)
            {
                case string serialized:
                    return JObject.Parse(serialized);
                default:
                    var settings = new JsonSerializerSettings()
                    {
                        Formatting = Formatting.Indented,
                        ContractResolver = new CamelCasePropertyNamesContractResolver()
                    };
                    return JObject.FromObject(body, JsonSerializer.Create(settings));
            }
        }
        
        public override string ToString() => Render().ToString();
    }
}