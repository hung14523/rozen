
using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace HHT.Resources.Model
{
    class ResponseData
    {
        public string Status { get; set; }
        public string Message { get; set; }
        public object Data { get; set; }

        public Dictionary<string, string> GetDataObject()
        {
            if (Data.GetType() == typeof(JObject))
            {
                return ((JObject)Data).ToObject<Dictionary<string, string>>();
            }
            else if (Data.GetType() == typeof(JArray))
            {
                return ((JArray)Data).ToObject<Dictionary<string, string>>();
            }

            return default;
        }

        public T GetDataObject<T>()
        {
            if (Data.GetType() == typeof(JObject))
            {
                return ((JObject)Data).ToObject<T>();
            }
            else if (Data.GetType() == typeof(JArray))
            {
                return ((JArray)Data).ToObject<T>();
            }

            return default;
        }
    }
}