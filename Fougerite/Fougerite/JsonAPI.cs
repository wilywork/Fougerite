using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Schema;
using Newtonsoft.Json.Schema.Generation;

namespace Fougerite
{
    public class JsonAPI
    {
        private static JsonAPI _inst;

        public string SerializeObjectToJson(object target)
        {
            return JsonConvert.SerializeObject(target);
        }

        public object DeSerializeJsonToObject(string target)
        {
            return JsonConvert.DeserializeObject(target);
        }

        public T DeSerializeJsonToObject<T>(string target)
        {
            return JsonConvert.DeserializeObject<T>(target);
        }

        public object SerializeXmlNode(System.Xml.XmlNode target)
        {
            return JsonConvert.SerializeXmlNode(target);
        }

        public object DeserializeXmlNode(string target)
        {
            return JsonConvert.DeserializeXmlNode(target);
        }

        public Type DeserializeAnonymousType<T>(string target, Type t)
        {
            return JsonConvert.DeserializeAnonymousType(target, t);
        }

        public JArray CreateJsonArray()
        {
            return new JArray();
        }

        public JObject CreateJsonObject()
        {
            return new JObject();
        }

        public JSchema CreateJSchema(string json)
        {
            return JSchema.Parse(json);
        }

        public JSchemaGenerator CreateJSchemaGenerator()
        {
            return new JSchemaGenerator();
        }

        public JSchema GenerateJSchema(Type specifiedclasstype)
        {
            JSchemaGenerator generator = new JSchemaGenerator();
            return generator.Generate(specifiedclasstype);
        }

        public JsonTextReader CreateJsonTextReader(string s)
        {
            return new JsonTextReader(new StringReader(s));
        }

        public JSchemaValidatingReader CreateJSchemaVReader(JsonReader reader)
        {
            return new JSchemaValidatingReader(reader);
        }

        public JsonSerializer CreateJsonSerializer()
        {
            return new JsonSerializer();
        }

        public JsonWriter CreateJsonWriter()
        {
            StringBuilder sb = new StringBuilder();
            StringWriter sw = new StringWriter(sb);
            return new JsonTextWriter(sw);
        }

        public static JsonAPI GetInstance
        {
            get { return _inst ?? (_inst = new JsonAPI()); }
        }
    }
}
