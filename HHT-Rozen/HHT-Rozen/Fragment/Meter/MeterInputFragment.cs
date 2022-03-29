using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Beardedhen.Androidbootstrap;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class MeterInputFragment : BaseFragment
    {
        View view;

        BootstrapEditText etStartMeter;
        BootstrapEditText etEndMeter;
        BootstrapEditText etKyuyu;

        private bool registFlg;
        private BootstrapButton mConfirmButton;

        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.fragment_meter_input, container, false);
            registFlg = Arguments.GetBoolean("registFlg", true);
            
            SetActionBarTitle(registFlg ? "出庫ﾒｰﾀｰ登録" : "帰着ﾒｰﾀｰ登録");

            TextView lblTitle = view.FindViewById<TextView>(Resource.Id.textView1);
            TextView lblTitle2 = view.FindViewById<TextView>(Resource.Id.textView2);

            TextView lblTitle3 = view.FindViewById<TextView>(Resource.Id.textView3);
            etStartMeter = view.FindViewById<BootstrapEditText>(Resource.Id.et_meterInput_startMeter);
            etEndMeter = view.FindViewById<BootstrapEditText>(Resource.Id.et_meterInput_endMeter);
            etKyuyu = view.FindViewById<BootstrapEditText>(Resource.Id.et_meterInput_kyuyu);

            mConfirmButton= view.FindViewById<BootstrapButton>(Resource.Id.confirmButton);
            mConfirmButton.Click += delegate
            {
                string message = "";
                string meter = "";

                try
                {
                    if (registFlg)
                    {
                        message = "出庫メーター" + "\n" + string.Format("{0:#,0}", int.Parse(etStartMeter.Text)) + "\nでよろしいですか？";
                        meter = etStartMeter.Text; 
                    }
                    else
                    {
                        if (etEndMeter.Text == "")
                        {
                            ErrorDialog("帰着メーターを入力してください", () => { }); return;
                        }

                        message = "帰着メーター" + "\n" + string.Format("{0:#,0}", int.Parse(etEndMeter.Text)) + "\nでよろしいですか？";
                        meter = etEndMeter.Text;

                        int.TryParse(etStartMeter.Text.Replace(",", ""), out int smeter);
                        bool parseResult = int.TryParse(etEndMeter.Text.Replace(",", ""), out int emeter);

                        if (!parseResult) { ErrorDialog("入力値に不具合があります\n再度入力してください", () => { }); return; }
                        if(smeter >= emeter) { ErrorDialog("帰着より出庫メーターが大きいです。", () => { }); return; }
                    }
                }
                catch
                {
                    ErrorDialog("チェック中にエラーが発生しました", () => { }); return;
                }

                    ConfirmDialog(message, (flg) => {
                    if (flg)
                    {
                        Task.Run(async() => {

                            var param = new Dictionary<string, string>()
                            {
                                {"tdate", Arguments.GetString("tdate")},
                                {"course",Arguments.GetString("course")},
                                {"bin",Arguments.GetString("bin")},
                                {"tantoCd", prefs.GetString("sagyousya_cd","")},
                                {"kbn",Arguments.GetString("kbn")},
                                {"meter",meter},
                                {"kyuyu",etKyuyu.Text},
                            };

                            var response = await WebService.PostRestAPI("meter/RegistMeter", param);
                            if (response["errMesg"] != "")
                            {
                                ErrorDialog(response["errMesg"], () => { });
                                return;
                            }

                            Activity.RunOnUiThread(() => {
                                Vibrate();

                                NoticeDialog("登録しました。", () => {
                                    FragmentManager.PopBackStack();
                                    FragmentManager.PopBackStack();
                                });
                            });
                        });
                    }
                });
                return;
            };


            if (registFlg)
            {
                etStartMeter.Text = Arguments.GetString("meter");
                etStartMeter.RequestFocus();
            }
            else
            {  
                lblTitle2.Visibility = ViewStates.Visible;
                lblTitle3.Visibility = ViewStates.Visible;
                etEndMeter.Visibility = ViewStates.Visible;
                etKyuyu.Visibility = ViewStates.Visible;

                if(Arguments.GetString("meter") != "")
                {
                    etStartMeter.Text = string.Format("{0:#,0}", int.Parse(Arguments.GetString("meter")));
                }

                etStartMeter.Enabled = false;
                etEndMeter.RequestFocus();

            }

            return view;
        }
    }
}