using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Beardedhen.Androidbootstrap;
using Com.Densowave.Bhtsdk.Barcode;
using HHT;

namespace HHT_Rozen.Fragment
{
    public class KosuCarryFragment : BaseFragment
    {
        private View view;
        TextView mOriconListTextView;
        BootstrapButton mConfirmButton;
        private BootstrapEditText mHaisoDateEditText;

        string mCarryBarcodes;
        string mCarryCaseKbns;

        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            view = inflater.Inflate(Resource.Layout.fragment_kosu_arata_carry, container, false);

            SetActionBarTitle("キャリー紐づけ");

            mCarryBarcodes = prefs.GetString("mCarryBarcodes", "");
            mCarryCaseKbns = prefs.GetString("mCarryCaseKbns", "");

            // バーコード桁数を20桁で設定
            ((MainActivity)Activity).ResizeBarcodeLength(20);

            mHaisoDateEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_haisoDate);
            mHaisoDateEditText.Text = CommonUtils.GetDateYYYYMMDDwithSlash(prefs.GetString("syuka_date", ""));

            mOriconListTextView = view.FindViewById<TextView>(Resource.Id.txt_oriconList);
            mConfirmButton = view.FindViewById<BootstrapButton>(Resource.Id.btn_confirm);
            mConfirmButton.Click += delegate
            {
                string barcode = mOriconListTextView.Text;
                if (barcode.Length == 0)
                {
                    ShowDialog("エラー", "貨物番号をスキャンしてください。", () => { });
                    return;
                }

                Task.Run(async () => {

                    var param = new Dictionary<string, string>
                    {
                        { "pSyukaDate", prefs.GetString("syuka_date","")},
                        { "pKamotsuNoList", mCarryBarcodes},
                        { "pCaseKbnList", mCarryCaseKbns},
                        { "tantoCd", prefs.GetString("sagyousya_cd","")},
                        { "serial",  Build.Serial }
                    };

                    return await PostRestAPI("", "kosu/kosu430", param);
                }).ContinueWith((t) => {

                    ((MainActivity)this.Activity).EnableScanning();

                    Activity.RunOnUiThread(() => {

                        dynamic result = t.Result;

                        if (t.Result.ContainsKey("errorMsg"))
                        {
                            string errorMsg = t.Result["errorMsg"];
                            ShowDialog("エラー", errorMsg, () => { });
                            return;
                        }

                        PlayBeepOk();
                        Vibrate(300);

                        mCarryBarcodes = "";
                        mCarryCaseKbns = "";

                        editor.Remove("mCarryBarcodes");
                        editor.Remove("mCarryCaseKbns");
                        editor.Apply();

                        ShowDialog("報告", "完了しました。", () => {
                            mOriconListTextView.Text = "";
                        });

                    });
                });


            };

            mOriconListTextView.Text = mCarryBarcodes.Replace(",", "\n");

            return view;
        }

        [System.Obsolete]
#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                // 2重スキャン防ぐ
                ((MainActivity)this.Activity).DisableScanning();

                string barcode = mOriconListTextView.Text;
                string barcodeArray = barcode.Replace("\n", ",");

                if(barcodeArray != "")
                {
                    string haisoCd = barcodeArray.Substring(7, 4);

                    if (barcodeArray.IndexOf(barcodeData.Data) >= 0)
                    {
                        ShowDialog("エラー", "既にスキャン済みです。", () => {
                            ((MainActivity)this.Activity).EnableScanning();
                        });
                        return;
                    }

                    if (haisoCd != barcodeData.Data.Substring(7, 4))
                    {
                        ShowDialog("エラー", "配送先が違います。", () => {
                            ((MainActivity)this.Activity).EnableScanning();
                        });
                        return;
                    }

                    if (barcodeArray.Split(",").Length >= 6)
                    {
                        ShowDialog("エラー", "6個目以上スキャンできません。", () => {
                            ((MainActivity)this.Activity).EnableScanning();
                        });
                        return;
                    }

                }
                

                // マテハンで紐づく
                var param = new Dictionary<string, string>
                {
                    { "pSyukaDate", prefs.GetString("syuka_date","")},
                    { "pKamotsuNo", barcodeData.Data},
                    { "tantoCd", prefs.GetString("sagyousya_cd","")},
                    { "handyId", Build.Serial}
                };

                Task.Run(async () => {
                    // CHK グローバル確認してから
                    var chkResult = await PostRestAPI("", "kosu/kosu440", param);

                    ((MainActivity)this.Activity).EnableScanning();

                    if (chkResult.ContainsKey("errorMsg"))
                    {
                        
                        ShowDialog("エラー", chkResult["errorMsg"].ToString());
                        return;
                    }

                    string syukaKbn = barcodeData.Data.Substring(0, 1);

                    if (chkResult["existFlag"].ToString() == "0" && (syukaKbn == "0" || syukaKbn == "3"))
                    {
                        Vibrate();

                        // 個口データが存在しない場合
                        Bundle bundle = new Bundle();
                        bundle.PutString("barcode", barcodeData.Data);
                        bundle.PutString("matehancd", "");
                        bundle.PutString("matekbn", "1");

                        StartFragment(FragmentManager, typeof(KosuNisugataFragment), bundle);
                    }
                    else
                    {
                        string caseKbn = "";

                        if(syukaKbn == "0" || syukaKbn == "3")
                        {
                            caseKbn = chkResult["caseKbn"].ToString();
                        }
                        
                        if (mCarryBarcodes.Length == 0)
                        {
                            mCarryBarcodes = barcodeData.Data;
                            mCarryCaseKbns = caseKbn;

                            // 作業開始
                            var param = new Dictionary<string, string>() { { "scd", prefs.GetString("sagyousya_cd", "") }, { "kbn", "2" } };
                            var res = await PostRestAPI("", "UpdateSagyoTime", param);
                        }
                        else
                        {
                            mCarryBarcodes += "," + barcodeData.Data;
                            mCarryCaseKbns += "," + caseKbn;
                        }

                        editor.PutString("mCarryBarcodes", mCarryBarcodes);
                        editor.PutString("mCarryCaseKbns", mCarryCaseKbns);
                        editor.Apply();

                        Activity.RunOnUiThread(() => {
                            
                            if(mOriconListTextView.Text == "")
                            {
                                mOriconListTextView.Text = barcodeData.Data;
                            }
                            else
                            {
                                mOriconListTextView.Text += "\n" + barcodeData.Data;
                            }
                        });
                    }
                });
            }
        }

        public override bool OnBackPressed()
        {
            if(mOriconListTextView.Text != "")
            {
                ShowDialog("警告", "スキャンした履歴がありますが、キャンセルしますが？",(flag)=>{
                    if (flag)
                    {
                        UpdateSagyoTime();

                        FragmentManager.PopBackStack();
                    }
                });
                return false;
            }

            return base.OnBackPressed();
        }

        private async void UpdateSagyoTime()
        {
            var param = new Dictionary<string, string>() { { "scd", prefs.GetString("sagyousya_cd", "") }, { "kbn", "3" } };
            var res = await PostRestAPI("", "UpdateSagyoTime", param);
        }


    }
}