using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using HHT_Rozen.Resources.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HHT_Rozen
{
    public static class WebApi
    {
        //public static string WEB_SERVICE_URL = "https://bms.jobs-logistics.jp/api/whcomm/";
        public static string WEB_SERVICE_URL = "https://test.jobs-logistics.jp/api/whcomm/";
        public static string REST_URL_APPEND = "/WebLink";
        public static string NAMESPACE = "";
        public static string MAC_FILE_NAME = "SOTETSU";

        private static readonly HttpClient _client = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            Credentials = new NetworkCredential("iris", "iris"),
        });

        public async static Task<Response> Post(string methodName, Dictionary<string, string> param)
        {
            if (param.ContainsKey("rtn"))
                param.Remove("rtn");

            if (NAMESPACE == "" || methodName == "GetSoukoName" || methodName == "LoginUser")
            {
                NAMESPACE = "butsu";
            }

            param.Add("rtn", methodName + "^" + MAC_FILE_NAME + "()");

            var response = await _client.PostAsync(WEB_SERVICE_URL + NAMESPACE + REST_URL_APPEND, new FormUrlEncodedContent(param));

            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine(@"\t" + methodName + " successfully processed.");

                var contents = await response.Content.ReadAsStringAsync();
                var result = JsonConvert.DeserializeObject<Response>(contents);

                return result;
            }
            else
            {
                Log.Error("WebService", response.Headers + " : " + response.ReasonPhrase);
                throw new Exception();
            }
        }
        public static void SetNameSpace(string nameSpace)
        {
            NAMESPACE = nameSpace;
        }

        public static string GetNameSpace()
        {
            return NAMESPACE;
        }
    }
}