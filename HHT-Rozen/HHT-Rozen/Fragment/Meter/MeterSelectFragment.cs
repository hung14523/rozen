using Android.OS;
using Android.Views;
using Com.Beardedhen.Androidbootstrap;
using Com.Densowave.Bhtsdk.Barcode;
using HHT;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class MeterSelectFragment : BaseFragment
    {
        private View view;
        private BootstrapEditText mSyukaDateEditText;
        private BootstrapEditText mCourseEditText;
        private BootstrapEditText mBinEditText;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region Component Init

            view = inflater.Inflate(Resource.Layout.fragment_meter_select, container, false);
            SetActionBarTitle(Arguments.GetBoolean("registFlg") ? "出庫ﾒｰﾀｰ登録" : "帰着ﾒｰﾀｰ登録");

            BootstrapButton btnConfirm = view.FindViewById<BootstrapButton>(Resource.Id.btn_mailRegistSelect_confirm);
            btnConfirm.Click += delegate { Confirm(); };

            mSyukaDateEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_meterSelect_tdate);
            mSyukaDateEditText.FocusChange += (sender, e) =>
            {
                if (e.HasFocus)
                {
                    mSyukaDateEditText.Text = mSyukaDateEditText.Text.Replace("/", "");
                    mSyukaDateEditText.SetSelection(mSyukaDateEditText.Text.Length);
                }
                else
                {
                    try
                    {
                        mSyukaDateEditText.Text = CommonUtils.GetDateYYYYMMDDwithSlash(mSyukaDateEditText.Text);
                    }
                    catch
                    {
                        ShowDialog("エラー", "正しい日付を入力してください。", () =>
                        {
                            mSyukaDateEditText.Text = "";
                            mSyukaDateEditText.RequestFocus();
                        });
                    }
                }
            };

            mCourseEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_meterSelect_course);
            mBinEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_meterSelect_bin);
            mBinEditText.KeyPress += (sender, e) =>
            {
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    e.Handled = true;
                    CommonUtils.HideKeyboard(Activity);
                    Confirm();
                }
                else
                {
                    e.Handled = false;
                }
            };

            #endregion

            var dt = DateTime.Now;
            var today = (int.Parse(dt.ToString("HH")) >= 6 ? dt.AddDays(1) : dt).ToString("yyyy/MM/dd"); 

            mSyukaDateEditText.Text = today;
            mCourseEditText.RequestFocus();

            return view;
        }

        [Obsolete]
        private void Confirm()
        {
            if (mSyukaDateEditText.Text == "")
            {
                ShowDialog("エラー", "日付を入力してください。", () => mSyukaDateEditText.RequestFocus());
                return;
            }

            if (mCourseEditText.Text == "")
            {
                ShowDialog("エラー", "コースを入力してください。", () => mCourseEditText.RequestFocus());
                return;
            }

            if (mBinEditText.Text == "")
            {
                ShowDialog("エラー", "便を入力してください。", () => mBinEditText.RequestFocus());
                return;
            }

            try
            {
                CommonUtils.GetDateYYYYMMDDwithSlash(mSyukaDateEditText.Text);
            }
            catch
            {
                ShowDialog("エラー", "日付を正しく入力してください。", () =>
                {
                    mSyukaDateEditText.Text = "";
                    mSyukaDateEditText.RequestFocus();
                });

                return;
            }

            Task.Run(async () =>
            {
                var param = new Dictionary<string, string>() 
                {
                    {"tdate", mSyukaDateEditText.Text.Replace("/", "")},
                    {"course",mCourseEditText.Text},
                    {"bin",mBinEditText.Text},
                    {"tantoCd", prefs.GetString("sagyousya_cd","")},
                    {"kbn",Arguments.GetBoolean("registFlg") ? "0" : "1"},
                };
                
                var result = await WebService.PostRestAPI("meter/GetMeter", param);
                if (result["errMesg"] != "")
                {
                    ErrorDialog(result["errMesg"], () => { });
                    return;
                }

                // 登録済みメールバッグ数を取得
                Bundle bundle = new Bundle();
                bundle.PutString("meter", result["meter"]);

                bundle.PutString("tdate", mSyukaDateEditText.Text.Replace("/", ""));
                bundle.PutString("course", mCourseEditText.Text);
                bundle.PutString("bin", mBinEditText.Text);
                bundle.PutString("kbn", Arguments.GetBoolean("registFlg") ? "0" : "1");
                
                bundle.PutBoolean("registFlg", Arguments.GetBoolean("registFlg"));
                StartFragment(FragmentManager, typeof(MeterInputFragment), bundle);

            });
        }

        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                // souko(3) , yyyymmdd(8) , course(5), bin(1) = 17

                // スキャン無効化
                ((MainActivity)this.Activity).DisableScanning();

                string barcode = barcodeData.Data;
                if (barcode.Length != 17)
                {
                    ShowDialog("エラー", "正しいバーコードをスキャンしてください。", () =>
                    {
                        ((MainActivity)this.Activity).EnableScanning();
                    });
                    return;
                }

                string soukoCd = barcode.Substring(0, 3);
                string hiduke = barcode.Substring(3, 8);
                string course = int.Parse(barcode.Substring(11, 5)).ToString();
                string bin = int.Parse(barcode[16..]).ToString();

                Activity.RunOnUiThread(() =>
                {
                    mSyukaDateEditText.Text = CommonUtils.GetDateYYYYMMDDwithSlash(hiduke);
                    mCourseEditText.Text = course;
                    mBinEditText.Text = bin;

                    ((MainActivity)this.Activity).EnableScanning();

                });
            }
        }

    }
}