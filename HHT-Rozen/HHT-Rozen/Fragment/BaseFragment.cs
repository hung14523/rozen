using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Preference;
using Com.Densowave.Bhtsdk.Barcode;
using HHT;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xamarin.Essentials;

namespace HHT_Rozen.Fragment
{
    public class BaseFragment : AndroidX.Fragment.App.Fragment
    {
        private CustomDialogFragment dialog;
        public static ISharedPreferences prefs;
        public static ISharedPreferencesEditor editor;
        public static bool IsTest;

        public enum KOSUMENU
        {
            ARATA_CASE,
            ARATA_ORICON
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            prefs = PreferenceManager.GetDefaultSharedPreferences(Context);
            editor = prefs.Edit();

            string nameSapce = prefs.GetString("CacheNamespace", "");
            if (nameSapce == "ara")
            {
                IsTest = true;
            }
        }

        [Obsolete]
        protected void StartFragment(AndroidX.Fragment.App.FragmentManager fm, Type fragmentClass)
        {
            BaseFragment fragment = null;
            try
            {
                fragment = (BaseFragment)Activator.CreateInstance(fragmentClass);
            }
            catch
            {

            }
            Activity.SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.fragmentContainer, fragment)
                .AddToBackStack(null)
                .Commit();
        }
        protected void StartFragment(AndroidX.Fragment.App.FragmentManager fm, Type fragmentClass, Bundle bundle)
        {
            BaseFragment fragment = null;
            try
            {
                fragment = (BaseFragment)Activator.CreateInstance(fragmentClass);
                fragment.Arguments = bundle;
            }
            catch
            {

            }

            CommonUtils.HideKeyboard(Activity);

            Activity.SupportFragmentManager
                .BeginTransaction()
                .Replace(Resource.Id.fragmentContainer, fragment)
                .AddToBackStack(null)
                .Commit();
        }

        public virtual bool OnKeyDown(Keycode keycode, KeyEvent paramKeyEvent)
        {
            return true;
        }

        public virtual void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            return;
        }

        public virtual bool OnBackPressed()
        {
            ((MainActivity)this.Activity).DismissDialog();
            CommonUtils.HideKeyboard(Activity);
            return true;
        }

        public void SetActionBarTitle(string title)
        {
            TextView titleTextView = ((MainActivity)this.Activity).SupportActionBar.CustomView.FindViewById<TextView>(Resource.Id.toolbar_title1);

            titleTextView.Text = title;
        }

        public void Vibrate()
        {
            Vibration.Vibrate(1000);
        }

        public void Vibrate(int time)
        {
            Vibration.Vibrate(time);
        }

        [Obsolete]
        public void ConfirmDialog(string body, Action<bool> callback)
        {
            ShowDialog("警告", body, callback);
        }

        [Obsolete]
        public void ErrorDialog(string body, Action callback)
        {
            ShowDialog("エラー", body, callback);
        }

        [Obsolete]
        public void NoticeDialog(string body, Action callback)
        {
            ShowDialog("報告", body, callback);
        }

        [Obsolete]
        public void ShowDialog(string title, string body, Action callback)
        {
            if (dialog != null) dialog.Dismiss();

            Bundle bundle = new Bundle();
            bundle.PutString("title", title);
            bundle.PutString("body", body);

            if (title == "エラー")
            {
                PlayBeepNg();
                Vibrate();
            }
            else if (title == "報告" || title == "満タン完了")
            {
                PlayBeepOk();
            }

            dialog = new CustomDialogFragment { Arguments = bundle };
            dialog.Cancelable = false;
            dialog.Show(FragmentManager, "");
            dialog.Dismissed += (s, e) =>
            {
                callback?.Invoke();
            };
        }

        [Obsolete]
        public void ShowDialog(string title, string body)
        {
            Bundle bundle = new Bundle();
            bundle.PutString("title", title);
            bundle.PutString("body", body);

            if (title == "エラー") { PlayBeepNg(); Vibrate(); }
            else if (title == "報告") { PlayBeepOk(); }

            CustomDialogFragment dialog = new CustomDialogFragment { Arguments = bundle };
            dialog.Cancelable = false;
            dialog.Show(FragmentManager, "");
            dialog.Dismissed += (s, e) =>
            {

            };
        }

        [Obsolete]
        public void ShowDialog(string title, string body, Action<bool> callback)
        {
            Bundle bundle = new Bundle();
            bundle.PutString("title", title);
            bundle.PutString("body", body);

            if (title == "エラー") PlayBeepNg();
            else if (title == "報告") PlayBeepOk();

            CustomDialogFragment dialog = new CustomDialogFragment { Arguments = bundle };
            dialog.Cancelable = false;
            dialog.Show(FragmentManager, "");
            dialog.Dismissed += (s, e) =>
            {
                if (e.Text == "true")
                {
                    callback(true);
                }
                else
                {
                    callback(false);
                }
            };
        }

        public void PlayBeepOk()
        {
            ((MainActivity)this.Activity).PlayBeep(Resource.Raw.beep_ok);
        }

        public void PlayBeepNg()
        {
            ((MainActivity)this.Activity).PlayBeep(Resource.Raw.beep_ng);
        }

        public void ShowProgress(string message)
        {
            ((MainActivity)this.Activity).ShowProgress(message);
        }

        public void DismissProgress()
        {
            ((MainActivity)this.Activity).DismissDialog();
        }

        public async Task<Dictionary<string, string>> PostRestAPI(string message, string url, Dictionary<string, string> param)
        {
            Activity.RunOnUiThread(() => ShowProgress(message));

            var result = await WebService.PostRestAPI(url, param);

            //await Task.Delay(1500);
            Activity.RunOnUiThread(() => DismissProgress());

            return await Task.FromResult(result);
        }

        public async Task<T> PostRestAPI<T>(string message, string url, Dictionary<string, string> param)
        {
            Activity.RunOnUiThread(() => ShowProgress(message));

            var result = await WebService.PostRestAPI<T>(url, param);

            await Task.Delay(500);
            Activity.RunOnUiThread(() => DismissProgress());

            return await Task.FromResult<T>(result);
        }

        public async Task<Dictionary<string, string>> GetRestAPI(string message, string url)
        {
            Activity.RunOnUiThread(() => ShowProgress(message));

            var result = await WebService.GetRestAPI(url);

            await Task.Delay(500);
            Activity.RunOnUiThread(() => DismissProgress());

            return await Task.FromResult(result);
        }
    }
}