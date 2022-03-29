using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Text;
using Android.Views;
using Android.Widget;
using Com.Beardedhen.Androidbootstrap;
using Com.Densowave.Bhtsdk.Barcode;
using HHT;

namespace HHT_Rozen.Fragment
{
    public class KosuKagosyaFragment : BaseFragment
    {
        private TextView mTenpoNameTextView;
        private BootstrapEditText mHaisoDateEditText;
        private BootstrapEditText mSakiEditText;
        private BootstrapEditText mMotoEditText;
        private BootstrapButton mMantanButton;

        private bool isFirstSekizai;
        private string matehancd;
        private string totalKosu;
        private bool mantanFlag;
        private bool sagyoflg;

        [Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region #View Setting
            View view = inflater.Inflate(Resource.Layout.fragment_kosu_arata_kagosya, container, false);
            SetActionBarTitle("かご車紐づけ");

            mTenpoNameTextView = view.FindViewById<TextView>(Resource.Id.txt_tenpoName);
            mSakiEditText = view.FindViewById<BootstrapEditText>(Resource.Id.himotukesaki);
            mMotoEditText = view.FindViewById<BootstrapEditText>(Resource.Id.himotukemoto);
            mMantanButton = view.FindViewById<BootstrapButton>(Resource.Id.btn_todoke_mantan);
            mHaisoDateEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_haisoDate);

            #endregion


            mHaisoDateEditText.Text = CommonUtils.GetDateYYYYMMDDwithSlash(prefs.GetString("syuka_date", ""));

            mMotoEditText.FocusChange += (s, e) =>
            {
                if (e.HasFocus && mSakiEditText.Text == "" && isFirstSekizai == false)
                {
                    mSakiEditText.RequestFocus();
                }
            };

            mSakiEditText.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    e.Handled = true;

                    if (mSakiEditText.Text == "")
                    {
                        ShowDialog("警告", "\n一個目の組付けですか？\n", (okFlag) =>
                        {
                            if (okFlag)
                            {
                                isFirstSekizai = true;
                                matehancd = "";

                                if (sagyoflg == false)
                                {
                                    Task.Run(async () =>
                                    {
                                        // 作業開始
                                        var param = new Dictionary<string, string>() { { "scd", prefs.GetString("sagyousya_cd", "") }, { "kbn", "2" } };
                                        var res = await PostRestAPI("", "UpdateSagyoTime", param);
                                    });

                                    sagyoflg = true;
                                }

                                mSakiEditText.Enabled = false;
                                mMotoEditText.RequestFocus();
                            }
                            else
                            {
                                mSakiEditText.Text = "";
                                mSakiEditText.RequestFocus();
                            }
                        });
                    }
                    else
                    {
                        if (mSakiEditText.Text.Length != 20)
                        {
                            ShowDialog("エラー", "有効な梱包番号ではありません。", () =>
                            {
                                mSakiEditText.RequestFocus();
                            });
                            return;
                        }
                        else
                        {
                            InputSaki(mSakiEditText.Text);
                        }
                    }
                }
            };

            mMotoEditText.KeyPress += (object sender, View.KeyEventArgs e) =>
            {
                e.Handled = false;
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    if (mMotoEditText.Text.Length == 20)
                    {
                        ProceedHimotsuke(mMotoEditText.Text);
                    }

                    e.Handled = true;
                    return;
                }
                else
                {
                    CommonUtils.HideKeyboard(Activity);
                }
            };

            mMantanButton.Click += MMantanButton_Click;

            // 当画面だけの仕様でCODE128は読めないようにする。
            ((MainActivity)Activity).ResizeBarcodeLength(20);

            mantanFlag = false;
            isFirstSekizai = false;
            totalKosu = "";
            sagyoflg = false;


            return view;
        }

        [Obsolete]
        private void MMantanButton_Click(object sender, System.EventArgs e)
        {
            var param = new Dictionary<string, string>
            {
                { "pSyukaDate", prefs.GetString("syuka_date","")},
                { "pMatehan", matehancd},
                { "pKamotsuNo", mSakiEditText.Text},
                { "tantoCd", prefs.GetString("sagyousya_cd","")},
                { "totalKosu", totalKosu },
            };

            ShowDialog("満タン完了", totalKosu + "個口\nよろしいですか？", (flag) =>
            {

                if (flag == false)
                {
                    return;
                }

                // 正しい配送先なのか？サーバーで確認する
                Task.Run(async () =>
                {
                    return await PostRestAPI("", "kosu/kosu420", param);
                }).ContinueWith(t =>
                {

                    Activity.RunOnUiThread(() =>
                    {

                        mantanFlag = false;

                        if (t.Result.ContainsKey("errorMsg"))
                        {
                            string errorMsg = t.Result["errorMsg"];
                            ShowDialog("エラー", errorMsg, () => { });
                            return;
                        }

                        Vibrate();
                        ShowDialog("報告", "完了しました。", () =>
                        {
                            //FragmentManager.PopBackStack();
                            mMotoEditText.Text = "";
                            mSakiEditText.Text = "";
                            mTenpoNameTextView.Text = "";

                            mMantanButton.Enabled = false;

                            mantanFlag = false;
                            isFirstSekizai = false;
                            totalKosu = "";
                            sagyoflg = false;
                            mSakiEditText.Enabled = true;
                            mSakiEditText.RequestFocus();
                        });
                    });
                });
            });

            return;
        }

        public override void OnResume()
        {
            base.OnResume();
            mMotoEditText.Text = "";
            mSakiEditText.Text = "";

            mSakiEditText.RequestFocus();
        }

        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                Console.WriteLine("-----------------------------------------------barcodeData：" + barcodeData.Data);
                // BARCODE CHECK
                if (barcodeData.Data.Length != 20)
                {
                    ShowDialog("エラー", "正しいバーコードをスキャンしてください。", () => { });
                    return;
                }

                // スキャン無効化
                ((MainActivity)this.Activity).DisableScanning();

                ProceedHimotsuke(barcodeData.Data);
            }
        }

        private void ProceedHimotsuke(string barcode)
        {
            if (mSakiEditText.Text == "" && isFirstSekizai == false)
            {
                // 正しい配送先なのか？サーバーで確認する
                Task.Run(async () =>
                {
                    var param = new Dictionary<string, string>
                        {
                            { "pSyukaDate", prefs.GetString("syuka_date","")},
                            { "pKamotsuNo", barcode}
                        };

                    return await PostRestAPI("", "kosu/kosu010", param);
                }).ContinueWith(t =>
                {

                    Activity.RunOnUiThread(() =>
                    {
                        ((MainActivity)this.Activity).EnableScanning();

                        if (t.Result.ContainsKey("errorMsg"))
                        {
                            string errorMsg = t.Result["errorMsg"];
                            ShowDialog("エラー", errorMsg, () => { });
                            return;
                        }

                        mSakiEditText.SetFilters(new IInputFilter[] { new InputFilterLengthFilter(barcode.Length) });
                        mSakiEditText.Text = barcode;
                        mSakiEditText.Enabled = false;

                        matehancd = t.Result["matehanCd"].ToString();
                        mTenpoNameTextView.Text = t.Result["tokNm"].ToString();
                        totalKosu = t.Result["totalKosu"].ToString();

                        if (sagyoflg == false)
                        {
                            Task.Run(async () =>
                            {
                                // 作業開始
                                var param = new Dictionary<string, string>() { { "scd", prefs.GetString("sagyousya_cd", "") }, { "kbn", "2" } };
                                var res = await PostRestAPI("", "UpdateSagyoTime", param);
                            });

                            sagyoflg = true;
                        }

                        mMantanButton.Enabled = true;
                        mMotoEditText.Enabled = true;

                        mMotoEditText.RequestFocus();
                    });
                });
            }
            else
            {
                InputMoto(barcode);
            }
        }


        private void MoveToNisugataPage(string barcode)
        {
            // 個口データが存在しない場合
            Bundle bundle = new Bundle();
            bundle.PutString("barcode", barcode);
            bundle.PutString("matehancd", matehancd);
            bundle.PutString("matekbn", "0");

            Activity.RunOnUiThread(() => DismissProgress());
            StartFragment(FragmentManager, typeof(KosuNisugataFragment), bundle);
        }


        private void InputSaki(string barcode)
        {
            // 正しい配送先なのか？サーバーで確認する
            Task.Run(async () =>
            {
                var param = new Dictionary<string, string>
                {
                    { "pSyukaDate", prefs.GetString("syuka_date","")},
                    { "pKamotsuNo", barcode}
                };

                return await PostRestAPI("", "kosu/kosu010", param);
            }).ContinueWith(t =>
            {

                Activity.RunOnUiThread(() =>
                {
                    ((MainActivity)this.Activity).EnableScanning();

                    if (t.Result.ContainsKey("errorMsg"))
                    {
                        string errorMsg = t.Result["errorMsg"];
                        ShowDialog("エラー", errorMsg, () => { });
                        return;
                    }

                    mSakiEditText.SetFilters(new IInputFilter[] { new InputFilterLengthFilter(barcode.Length) });
                    mSakiEditText.Text = barcode;
                    mSakiEditText.Enabled = false;

                    matehancd = t.Result["matehanCd"].ToString();
                    mTenpoNameTextView.Text = t.Result["tokNm"].ToString();
                    totalKosu = t.Result["totalKosu"].ToString();
                    mMantanButton.Enabled = true;
                    mMotoEditText.Enabled = true;

                    mMotoEditText.RequestFocus();
                });

            });
        }

        private void InputMoto(string barcode)
        {
            // マテハンで紐づく
            var param = new Dictionary<string, string>
            {
                { "pSyukaDate", prefs.GetString("syuka_date","")},
                { "pKamotsuNo", barcode},
                { "tantoCd", prefs.GetString("sagyousya_cd","")},
                { "pMatehan", matehancd},
                { "handyId", Build.Serial}
            };

            Activity.RunOnUiThread(() => ShowProgress(""));

            Task.Run(async () =>
            {
                // スキャン済み確認
                var chkResult = await PostRestAPI("", "kosu/kosu440", param);

                Activity.RunOnUiThread(() => DismissProgress());
                ((MainActivity)this.Activity).EnableScanning();

                if (chkResult.ContainsKey("errorMsg"))
                {
                    ShowDialog("エラー", chkResult["errorMsg"].ToString());
                    return;
                }

                string syukaKbn = barcode.Substring(0, 1);
                //syukaKbn = "5";

                // chkjdataデータ確認
                if (chkResult["existFlag"].ToString() == "0" && (syukaKbn == "0" || syukaKbn == "3"))
                {
                    // 個口データが存在しない場合 TC,DC
                    Vibrate();
                    MoveToNisugataPage(barcode);
                }
                else
                {
                    if (syukaKbn == "0" || syukaKbn == "3")
                    {
                        if (!chkResult.ContainsKey("caseKbn"))
                        {
                            ShowDialog("エラー", "取引先が見つかりませんでした。");
                            return;
                        }

                        // 個口データが存在する場合または出荷区分が０，３ではない場合
                        param.Add("caseKbn", chkResult["caseKbn"].ToString());

                    }
                    else
                    {
                        param.Add("caseKbn", "");
                    }

                    var kosu410 = await PostRestAPI("", "kosu/kosu020", param);

                    Activity.RunOnUiThread(() =>
                    {
                        if (kosu410 == null || kosu410.ContainsKey("errorMsg"))
                        {
                            string errorMsg = kosu410["errorMsg"];
                            ShowDialog("エラー", errorMsg, () => { });
                            return;
                        }

                        string status = kosu410["status"].ToString();
                        if (status == "1")
                        {
                            ShowDialog("エラー", "すでに紐づけ作業済みです。", () => { });
                            return;
                        }

                        string kosu = kosu410["kosu"].ToString();
                        mMotoEditText.Text = barcode;
                        matehancd = kosu410["matehanCd"].ToString();

                        ShowDialog("報告", kosu410["tokNm"].ToString() + "\n" + kosu + "個口", () =>
                        {
                            isFirstSekizai = false;

                            mSakiEditText.Enabled = true;
                            mMotoEditText.Enabled = false;

                            mSakiEditText.Text = "";
                            mMotoEditText.Text = "";
                            mTenpoNameTextView.Text = "";
                            matehancd = "";
                            mantanFlag = true;  // BackPressed

                            mMantanButton.Enabled = false;

                            mSakiEditText.RequestFocus();
                        });
                    });
                }
            });
        }

        [Obsolete]
        public override bool OnBackPressed()
        {
            if (mantanFlag)
            {
                ShowDialog("警告", "スキャンした履歴がありますが、キャンセルしますが？", (flag) =>
                {
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