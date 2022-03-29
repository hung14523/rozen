using Android.App;
using Android.Content;
//using Android.Support.V7.App;
using Android.Util;
using System;
using System.Threading.Tasks;
using AndroidX.AppCompat.App;
using Microsoft.AppCenter.Crashes;

namespace HHT_Rozen
{
    [Activity(Theme = "@style/MyTheme.Splash", MainLauncher = true, NoHistory = true)]
    public class SplashActivity : AppCompatActivity
    {
        static readonly string TAG = typeof(SplashActivity).Name;

        protected override void OnResume()
        {
            base.OnResume();

            Task.Run(() =>
            {
                
                try
                {
                    /*
                    bool isConnected = await WebService.IsConnected();
                    
                    RunOnUiThread(() =>
                    {
                        if (isConnected)
                        {
                            Toast.MakeText(ApplicationContext, "サーバと接続成功しました。", ToastLength.Short).Show();
                        }
                        else
                        {
                            Toast.MakeText(ApplicationContext, "サーバと接続失敗。\n接続情報を確認してください。", ToastLength.Short).Show();
                        }
                    });
                    */
                }
                catch (Exception exception)
                {
                    Crashes.TrackError(exception);
                }

                Log.Debug(TAG, "Startup work is finished - starting MainActivity.");
                StartActivity(new Intent(Application.Context, typeof(MainActivity)));
                
            });
        }
        
        public override void OnBackPressed() { }
    }
}