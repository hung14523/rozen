using System;
using System.Collections.Generic;
using System.Globalization;
using System.Net.Http;
using System.Text.RegularExpressions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views.InputMethods;

namespace HHT
{
    public class CommonUtils
    {

        private static readonly HttpClient client = new HttpClient();

        // サーバからデータ取得（非同期）
        public static string Post(string url, Dictionary<string, string> values)
        {
            var content = new FormUrlEncodedContent(values);
            
            var response = client.PostAsync(url, content).Result;
            var responseString = response.Content.ReadAsStringAsync().Result;
            
            return responseString;
        }

        // ソフトキーボードを隠す
        public static void HideKeyboard(Activity activity)
        {
            InputMethodManager inputManager = (InputMethodManager)activity.GetSystemService(Context.InputMethodService);

            var currentFocus = activity.CurrentFocus;
            if (currentFocus != null)
            {
                inputManager.HideSoftInputFromWindow(currentFocus.WindowToken, HideSoftInputFlags.None);
            }
        }
        
        public static string GetDateYYYYMMDDwithSlash(string dateString)
        {
            string ymd = Regex.Replace(Convert.ToString(dateString), @"[^\u0000-\u007F]|/", string.Empty);
            DateTime dt = DateTime.ParseExact(ymd.ToString(), "yyyyMMdd", CultureInfo.InvariantCulture);
            return dt.ToString("yyyy/MM/dd", CultureInfo.InvariantCulture);
        }

        public static bool IsEmulator()
        {
            return Build.Fingerprint.StartsWith("generic")
                    || Build.Fingerprint.StartsWith("unknown")
                    || Build.Model.Contains("google_sdk")
                    || Build.Model.Contains("Emulator")
                    || Build.Model.Contains("Android SDK built for x86")
                    || Build.Manufacturer.Contains("Genymotion")
                    || (Build.Brand.StartsWith("generic") && Build.Device.StartsWith("generic"))
                    || "google_sdk".Equals(Build.Product);
        }

    }
}