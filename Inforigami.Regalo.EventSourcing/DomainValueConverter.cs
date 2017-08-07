using System;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Inforigami.Regalo.EventSourcing
{
    public class DomainValueConverter<T> : JsonConverter
    {
        private readonly Func<string, T> _parser;

        public DomainValueConverter(Func<string, T> parser)
        {
            if (parser == null) throw new ArgumentNullException(nameof(parser));
            _parser = parser;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var domainValue = value as DomainValue<T>;
            if (domainValue != null)
            {
                JToken.FromObject(domainValue.Value).WriteTo(writer);
            }
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var token = JToken.Load(reader);
            return Activator.CreateInstance(objectType, _parser(token.ToString()));
        }

        public override bool CanConvert(Type objectType)
        {
            return typeof(DomainValue<T>).IsAssignableFrom(objectType);
        }
    }
}