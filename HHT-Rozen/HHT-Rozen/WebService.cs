using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace HHT_Rozen
{
    class WebService
    {
        private static readonly HttpClient _client = new HttpClient(new HttpClientHandler()
        {
            AllowAutoRedirect = false,
            UseProxy = false,
            Credentials = new NetworkCredential("iris", "iris"),
        });

        public readonly static string TAG = "WebService";

        private static string WEB_SERVICE_URL = "";

        public async static Task<bool> IsConnected()
        {
            if (WEB_SERVICE_URL == "")
            {
                return false;
            }

            var url = WEB_SERVICE_URL + "connect";

            _client.Timeout = TimeSpan.FromSeconds(2000);
            string resultData = await _client.GetStringAsync(url);

            if (resultData.Equals("true"))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #region ログイン=====================================================================

        public async static Task<Dictionary<string, string>> GetRestAPI(string requestCode)
        {
            if (WEB_SERVICE_URL == "")
            {
                return null;
            }

            var url = WEB_SERVICE_URL + requestCode;

            var response = await _client.GetStringAsync(url);

            Console.WriteLine("requestCode : " + response);
            Log.Error(TAG, "param : " + requestCode);

            var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(response);

            Console.WriteLine("result : \n" + JToken.Parse(response).ToString());

            return await Task.FromResult(result);
        }

        // Cache RestAPI専用
        public async static Task<Dictionary<string, string>> PostRestAPI(
            string requestCode, Dictionary<string, string> param)
        {
            var url = WEB_SERVICE_URL + requestCode;
            var convertedParam = new FormUrlEncodedContent(param);

            var response = await _client.PostAsync(url, convertedParam);

            if (response.IsSuccessStatusCode)
            {
                var contents = await response.Content.ReadAsStringAsync();
                bool errorFlg = false;
                string s = string.Join(";", param.Select(x => x.Key + "=" + x.Value).ToArray());
                Console.WriteLine("requestCode : " + requestCode);
                Log.Error(TAG, "param : " + s);

                var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(contents, new JsonSerializerSettings
                {
                    Error = (sender, errorArgs) =>
                    {
                        errorFlg = true;
                        var currentError = errorArgs.ErrorContext.Error.Message;
                        errorArgs.ErrorContext.Handled = true;

                        Log.Error(TAG, "JsonConvert.DeserializeObject Error Message : " + currentError);
                        Log.Error(TAG, "Return Message : " + contents);
                    }
                });

                if (!errorFlg)
                    Console.WriteLine("result : \n" + JToken.Parse(contents).ToString());


                return await Task.FromResult(result);
            }
            else
            {
                Log.Error(TAG, response.Headers + " : " + response.ReasonPhrase);
                throw new Exception(response.ReasonPhrase);
            }
        }

        public async static Task<T> PostRestAPI<T>(
            string requestCode, Dictionary<string, string> param)
        {
            var url = WEB_SERVICE_URL + requestCode;
            var convertedParam = new FormUrlEncodedContent(param);

            var response = await _client.PostAsync(url, convertedParam);

            if (response.IsSuccessStatusCode)
            {
                var contents = await response.Content.ReadAsStringAsync();
                bool errorFlg = false;
                string s = string.Join(";", param.Select(x => x.Key + "=" + x.Value).ToArray());
                Console.WriteLine("requestCode : " + requestCode);
                Log.Error(TAG, "param : " + s);

                var result = JsonConvert.DeserializeObject<T>(contents, new JsonSerializerSettings
                {
                    Error = (sender, errorArgs) =>
                    {
                        errorFlg = true;
                        var currentError = errorArgs.ErrorContext.Error.Message;
                        errorArgs.ErrorContext.Handled = true;

                        Log.Error(TAG, "JsonConvert.DeserializeObject Error Message : " + currentError);
                        Log.Error(TAG, "Return Message : " + contents);
                    }
                });

                if (!errorFlg)
                    Console.WriteLine("result : \n" + JToken.Parse(contents).ToString());
                return await Task.FromResult<T>(result);
            }
            else
            {
                Log.Error(TAG, response.Headers + " : " + response.ReasonPhrase);
                throw new Exception(response.ReasonPhrase);
            }
        }

        #endregion

        public static void SetRESTURLForArata(string restUrl, string nameSpace)
        {
            WEB_SERVICE_URL = "https://" + restUrl + "/httapi/" + nameSpace + "/";
        }
    }
}