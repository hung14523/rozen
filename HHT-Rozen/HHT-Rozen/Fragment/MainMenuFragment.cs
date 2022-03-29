using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class MainMenuFragment : BaseFragment
    {
        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            // Create your fragment here
        }

        [Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region #ViewSetting
            View view = inflater.Inflate(Resource.Layout.fragment_menu_main, container, false);

            SetActionBarTitle("業務メニュー");

            Button btnNyuka = view.FindViewById<Button>(Resource.Id.btn_main_manager_nyuka);
            Button btnTsumikae = view.FindViewById<Button>(Resource.Id.btn_main_manager_tsumikae);
            Button btnTsumikomi = view.FindViewById<Button>(Resource.Id.btn_main_manager_tsumikomi);
            Button btnNohin = view.FindViewById<Button>(Resource.Id.btn_main_manager_nohin);
            Button btnMeter = view.FindViewById<Button>(Resource.Id.btn_main_manager_meter);
            Button btnPms = view.FindViewById<Button>(Resource.Id.btn_main_manager_pms);

            #endregion
            btnNyuka.Click += delegate
            {

                // PMS作業中に他の作業はできない
                /*
                if (prefs.GetString("sflg", "0") != "0")
                {
                    ErrorDialog("他の作業中です。先に作業終了してください。", () => { });
                    return;
                }
                */

                UpdateSagyoTime();
                StartFragment(FragmentManager, typeof(KosuMenuFragment));
            };
            btnTsumikae.Click += delegate { StartFragment(FragmentManager, typeof(TsumikaeMenuFragment)); };
            btnTsumikomi.Click += delegate { StartFragment(FragmentManager, typeof(TsumikomiSelectFragment)); };
            btnNohin.Click += delegate { StartFragment(FragmentManager, typeof(NohinSelectFragment)); };
            btnMeter.Click += delegate { StartFragment(FragmentManager, typeof(MeterMenuFragment)); };
            /*
            btnPms.Click += delegate { StartFragment(FragmentManager, typeof(PmsGyomuFragment)); };
            */

            btnPms.Visibility = ViewStates.Gone;
            return view;
        }
        public override void OnResume()
        {
            base.OnResume();

            // PMS作業ステータスを取得(あらたは使わない）
            /*
            Task.Run(async () => {
                var res = await PostRestAPI("", "GetSagyoStatus", new Dictionary<string, string>() { { "scd", prefs.GetString("sagyousya_cd", "") } });
                if (res.ContainsKey("errMesg"))
                {
                    ErrorDialog(res["errMesg"], () => { });
                }

                //var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(res.data.ToString());
                editor.PutString("sflg", res["sflg"]);
                editor.Apply();
            });
            */
        }

        [Obsolete]
        public override bool OnKeyDown(Keycode keycode, KeyEvent paramKeyEvent)
        {
            if (keycode == Keycode.Num1)
            {
                // PMS作業中に他の作業はできない
                /*
                if (prefs.GetString("sflg", "0") != "0")
                {
                    ErrorDialog("他の作業中です。先に作業終了してください。", () => { });
                    return false;
                }
                */

                StartFragment(FragmentManager, typeof(KosuMenuFragment));
                UpdateSagyoTime();
            }
            else if (keycode == Keycode.Num2)
            {
                StartFragment(FragmentManager, typeof(TsumikaeMenuFragment));
            }
            else if (keycode == Keycode.Num3)
            {
                StartFragment(FragmentManager, typeof(TsumikomiSelectFragment));
            }
            else if (keycode == Keycode.Num4)
            {
                StartFragment(FragmentManager, typeof(NohinSelectFragment));
            }
            else if (keycode == Keycode.Num4)
            {
                StartFragment(FragmentManager, typeof(MeterMenuFragment));
            }

            return true;
        }

        private async void UpdateSagyoTime()
        {
            // メニューから遷移のみ処理する
            var param = new Dictionary<string, string>() { { "scd", prefs.GetString("sagyousya_cd", "") }, { "kbn", "1" } };
            var res = await PostRestAPI("", "UpdateSagyoTime", param); //使用しない
            /*
            ShowProgress("");
            Task.Run(async () =>
            {
                try
                {
                    var response = await WebApi.Post("UpdateSagyoTime",param);
                    //var errMesg = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.errMesg.ToString());
                    if (!string.IsNullOrEmpty(response.errMesg)) {
                        ShowDialog("エラー", response.errMesg);
                        return;
                    }
                }
                catch(Exception) {

                }
                finally
                {
                    Activity.RunOnUiThread(() => { DismissProgress(); });
                }
            });
            */
        }
    }
}