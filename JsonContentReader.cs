using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using DotLiquid;
using Scriban;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Dynamic;
using Scriban.Runtime;
using System.IO;

namespace CloudLiquid.ContentFactory
{
    public class JsonContentReader : IContentReader
    {
        public JsonContentReader()
        {

        }

        public static async Task<ScriptObject> ScribanParseRequestAsync(HttpContent content)
        {
            string requestBody = await content.ReadAsStringAsync();
            var transformInput = new Dictionary<string, object>();

            var requestJson = JsonConvert.DeserializeObject<IDictionary<string, object>>(requestBody, new DictionaryConverter());

            // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
            transformInput.Add("content", requestJson);
            var sObject = BuildScriptObject(transformInput);
            return sObject;
        }

       // public static string RenderJson(string json, string content)
       // {

           // var expando = JsonConvert.DeserializeObject<ExpandoObject>(json);
           // var sObject = BuildScriptObject(expando);
           // var templateCtx = new Scriban.TemplateContext();
           // templateCtx.PushGlobal(sObject);
           // var template = Scriban.Template.Parse(content);
           // var result = template.Render(templateCtx);

           // return result;
       // }

        public static ScriptObject BuildScriptObject(IDictionary<string, object> expando)
        {
            var dict = (IDictionary<string, object>)expando;
            var scriptObject = new ScriptObject();

            foreach (var kv in dict)
            {
                var renamedKey = StandardMemberRenamer.Rename(kv.Key);

                if (kv.Value is ExpandoObject expandoValue)
                {
                    scriptObject.Add(renamedKey, BuildScriptObject(expandoValue));
                }
                else
                {
                    scriptObject.Add(renamedKey, kv.Value);
                }
            }

            return scriptObject;
        }

        public async Task<Hash> ParseRequestAsync(HttpContent content)
        {
            string requestBody = await content.ReadAsStringAsync();

            var transformInput = new Dictionary<string, object>();

            var requestJson = JsonConvert.DeserializeObject<IDictionary<string, object>>(requestBody, new DictionaryConverter());

            // Wrap the JSON input in another content node to provide compatibility with Logic Apps Liquid transformations
            transformInput.Add("content", requestJson);

            return Hash.FromDictionary(transformInput);
        }

		public Hash ParseString(string content)
		{
			var transformInput = new Dictionary<string, object>();
            var requestJson = JsonConvert.DeserializeObject<IDictionary<string, object>>(content, new DictionaryConverter());
            transformInput.Add("content", requestJson);
            return Hash.FromDictionary(transformInput);
		}

        public static JToken RemoveEmptyChildren(JToken token)
        {
            if (token.Type == JTokenType.Object)
            {
                JObject copy = new JObject();
                foreach (JProperty prop in token.Children<JProperty>())
                {
                    JToken child = prop.Value;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(prop.Name, child);
                    }
                }
                return copy;
            }
            else if (token.Type == JTokenType.Array)
            {
                JArray copy = new JArray();
                foreach (JToken item in token.Children())
                {
                    JToken child = item;
                    if (child.HasValues)
                    {
                        child = RemoveEmptyChildren(child);
                    }
                    if (!IsEmpty(child))
                    {
                        copy.Add(child);
                    }
                }
                return copy;
            }
            return token;
        }

        public static bool IsEmpty(JToken token)
        {
            if (token.Type == JTokenType.Array && ((JArray)token).Count <= 0)
                return true;
            else if (token.Type == JTokenType.String)
                return (token.Type == JTokenType.Null || string.IsNullOrEmpty(Convert.ToString(((JValue)token).Value)));
            else
                return (token.Type == JTokenType.Null);
        }
    }
}
