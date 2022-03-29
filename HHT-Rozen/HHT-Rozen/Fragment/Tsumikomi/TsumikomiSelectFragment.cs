using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Com.Beardedhen.Androidbootstrap;
using Com.Densowave.Bhtsdk.Barcode;
using HHT;
using Java.Lang;

namespace HHT_Rozen.Fragment
{
    public class TsumikomiSelectFragment : BaseFragment
    {
        BootstrapEditText mSyukaDateEditText;
        BootstrapEditText mCourseEditText;
        BootstrapEditText mBinEditText;
        BootstrapButton mConfirmButton;

        private bool IsTest;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region #View
            View view = inflater.Inflate(Resource.Layout.fragment_tsumikomi_select, container, false);

            SetActionBarTitle("積込検品");

            mSyukaDateEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_tsumikomiSelect_syukaDate);
            mCourseEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_tsumikomiSelect_course);
            mBinEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_tsumikomiSelect_bin);
            mConfirmButton = view.FindViewById<BootstrapButton>(Resource.Id.btn_tsumikomiSelect_confirm);
            #endregion

            #region #Component Event
            mSyukaDateEditText.FocusChange += (sender, e) => {
                if (e.HasFocus)
                {
                    mSyukaDateEditText.Text = mSyukaDateEditText.Text.Replace("/", "");
                    mSyukaDateEditText.SetSelection(mSyukaDateEditText.Text.Length);
                }
                else
                {
                    if (mSyukaDateEditText.Text == "")
                        return;
                    
                    try
                    {
                        mSyukaDateEditText.Text = CommonUtils.GetDateYYYYMMDDwithSlash(mSyukaDateEditText.Text);
                    }
                    catch
                    {
                        ShowDialog("エラー", "正しい日付を入力してください。", () =>
                        {
                            mSyukaDateEditText.Text = DateTime.Now.ToString("yyyyMMdd");
                            mSyukaDateEditText.RequestFocus();
                        });
                    }
                }
            };
            mBinEditText.KeyPress += (sender, e) => {
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
            mConfirmButton.Click += delegate { Confirm(); };
            #endregion

            // First init
            var today = DateTime.Now.ToString("yyyy/MM/dd");
            var hour = int.Parse(DateTime.Now.ToString("HH"));
            if (hour >= 7)
            {
                today = DateTime.Now.AddDays(1).ToString("yyyy/MM/dd");
            }

            mSyukaDateEditText.Text = today;
            mCourseEditText.RequestFocus();
            
            return view;
        }

        // CHECK INPUT AND MOVE TO NEXT FRAGMENT
        [Obsolete]
        private void Confirm()
        {
            if (!CheckInputEmpty())
                return;
           
            ((MainActivity)this.Activity).ShowProgress("便情報を確認しています。");

            Task.Run(async () =>
            {
                string syuka_date = mSyukaDateEditText.Text.Replace("/", "");

                var param = new Dictionary<string, string>()
                {
                    {"syuka_date",syuka_date},
                    {"course",mCourseEditText.Text},
                    {"bin",mBinEditText.Text},
                };

                var result = await WebService.PostRestAPI("tumi/tumi010", param);
                
                if (result == null || result.Count == 0 || result["state"].ToString() == "0")
                {
                    ShowDialog("エラー", "コースNoがみつかりません。", () => { });
                    return;
                }
                else if (int.Parse(result["state"].ToString()) >= 3)
                {
                    ShowDialog("エラー", "該当コースの積込みは完了しています。", () => { });
                    return;
                }

                editor.PutString("syuka_date", syuka_date);
                editor.PutString("course", mCourseEditText.Text);
                editor.PutString("bin_no", mBinEditText.Text);
                editor.PutString("kansen_kbn", "0");
                editor.Apply();

                StartFragment(FragmentManager, typeof(TsumikomiSearchFragment));

            }).ContinueWith(t => {
                Activity.RunOnUiThread(() => ((MainActivity)this.Activity).DismissDialog());
            });
        }

        [Obsolete]
        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                // スキャン無効化
                ((MainActivity)this.Activity).DisableScanning();

                string barcode = barcodeData.Data;
                if(barcode.Length != 17)
                {
                    ShowDialog("エラー", "正しいバーコードをスキャンしてください。", () => {
                        ((MainActivity)this.Activity).EnableScanning();
                    });
                    return;
                }

                string soukoCd = Integer.ParseInt(barcode.Substring(0, 3)).ToString();
                string hiduke = barcode.Substring(3, 8);
                string course = Integer.ParseInt(barcode.Substring(11, 5)).ToString();
                string bin = barcode.Substring(16, 1);

                if (soukoCd != prefs.GetString("souko_cd", ""))
                {
                    ShowDialog("エラー", "倉庫コードが違います。", () => {
                        ((MainActivity)this.Activity).EnableScanning();
                    });
                    return;
                }

                Activity.RunOnUiThread(() => {
                    mSyukaDateEditText.Text = CommonUtils.GetDateYYYYMMDDwithSlash(hiduke);
                    mCourseEditText.Text = course;
                    mBinEditText.Text = bin;

                    ((MainActivity)this.Activity).EnableScanning();
                });
            }
        }

        [Obsolete]
        private bool CheckInputEmpty()
        {
            if (mSyukaDateEditText.Text == "")
            {
                ShowDialog("エラー", "配送日を入力してください。", () => { mSyukaDateEditText.RequestFocus(); });
                return false;
            }

            if (mCourseEditText.Text == "")
            {
                ShowDialog("エラー", "コースNoを入力してください。", () => { mCourseEditText.RequestFocus(); });
                return false;
            }

            if (mBinEditText.Text == "")
            {
                ShowDialog("エラー", "便を入力してください。", () => { mBinEditText.RequestFocus(); });
                return false;
            }

            return true;
        }
    }
}