using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Densowave.Bhtsdk.Barcode;
using HHT.Resources.Model;

namespace HHT_Rozen.Fragment
{
    public class TsumikomiWorkFragment : BaseFragment
    {
        private EditText mKosuEditText;
        private EditText mCarEditText;
        private EditText mCarryEditText;
        private EditText mKargoEditText;
        private EditText mSonotaEditText;

        private string tempCategory; // 貨物種類を臨時保存
        private string souko_cd, syuka_date, tokuisaki_cd, todokesaki_cd, bin_no, course;
        private string matehan;
        private volatile bool carLabelInputMode = false;

        string tempKamotsuNo;

        private bool IsUpdated;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_tsumikomi_work, container, false);

            SetActionBarTitle("積込検品");

            #region #View Setting
            TextView tokuiNm = view.FindViewById<TextView>(Resource.Id.txt_tsumikomiWork_tokuisakiNm);
            mKosuEditText = view.FindViewById<EditText>(Resource.Id.et_tsumikomiWork_kosu);
            mCarEditText = view.FindViewById<EditText>(Resource.Id.et_tsumikomiWork_carLabel);
            mCarryEditText = view.FindViewById<EditText>(Resource.Id.et_tsumikomiWork_carry);
            mKargoEditText = view.FindViewById<EditText>(Resource.Id.et_tsumikomiWork_kargoCar);
            mSonotaEditText = view.FindViewById<EditText>(Resource.Id.et_tsumikomiWork_sonota);
            #endregion

            souko_cd = prefs.GetString("souko_cd", "");
            syuka_date = prefs.GetString("syuka_date", "");
            tokuisaki_cd = prefs.GetString("tokuisaki_cd", "");
            todokesaki_cd = prefs.GetString("todokesaki_cd", "");
            bin_no = prefs.GetString("bin_no", "");
            course = prefs.GetString("course", "");

            tokuiNm.Text = prefs.GetString("tokuisaki_nm", "");
            mKosuEditText.SetBackgroundColor(Android.Graphics.Color.Yellow);

            mKargoEditText.Text = prefs.GetString("kagosyaCount", "0");
            mCarryEditText.Text = prefs.GetString("carryCount", "0");
            tempCategory = "";

            IsUpdated = false;

            return view;
        }

        // TRG ボタン押した時
        [Obsolete]
        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;
            
            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                // スキャン無効化
                ((MainActivity)this.Activity).DisableScanning();

                Dictionary<string, string> param = GetProcParam(barcodeData.Data);

                if (carLabelInputMode == false)
                {
                    Activity.RunOnUiThread(() =>ShowProgress("読み込み中"));
                    
                    Task.Run(async () =>
                    {
                        try
                        {
                            param = new Dictionary<string, string>()
                            {
                                { "pTerminalID",  prefs.GetString("terminal_id","")},
                                { "pSagyosyaCD", prefs.GetString("sagyousya_cd","") },
                                { "pSyukaDate", syuka_date},
                                { "pKamotsuNo", barcodeData.Data },
                                { "tokuisakiCd", tokuisaki_cd },
                                { "todokesakiCd", todokesaki_cd },
                                { "binNo", bin_no },
                            };

                            tempKamotsuNo = barcodeData.Data;

                            string url = "tumi/tumi060";
                            MTumikomiProc result = await WebService.PostRestAPI<MTumikomiProc>(url, param);

                            if (result.poMsg != null && result.poMsg != "")
                            {
                                ShowDialog("エラー", result.poMsg, () => {
                                    ((MainActivity)this.Activity).EnableScanning();
                                });
                                return;
                            }

                            tempCategory = result.poCategory;
                            matehan = result.poMatehan;
                            carLabelInputMode = true;

                            Activity.RunOnUiThread(() => {
                                //	正常登録
                                mKosuEditText.Text = result.poKosuCnt;
                                mCarEditText.SetBackgroundColor(Android.Graphics.Color.Yellow);
                                mKosuEditText.SetBackgroundColor(Android.Graphics.Color.White);
                            });
                        }
                        catch (Exception e)
                        {
                            ShowDialog("エラー", e.Message, () => {
                                ((MainActivity)this.Activity).EnableScanning();
                            });
                        }
                        
                    }).ContinueWith(t =>
                    {
                        // UIスレッドでの処理
                        ((MainActivity)this.Activity).EnableScanning();
                        Activity.RunOnUiThread(()=> ((MainActivity)this.Activity).DismissDialog());
                    });
                }
                else
                {
                    // 作業ステータス更新・積込処理
                    Activity.RunOnUiThread(() => mCarEditText.Text = barcodeData.Data);

                    UpdateSagyoStatus(barcodeData.Data);
                }
            }
        }

        // 作業ステータス更新・積込処理 TUMIKOMI080,TUMIKOMI311
        [Obsolete]
        private async void UpdateSagyoStatus(string saryouData)
        {
            Activity.RunOnUiThread(() => ShowProgress("読み込み中"));

            var param = GetProcParam(saryouData);
            param.Add("pKamotsuNo", tempKamotsuNo);

            int resultCode = 1;
            
            try
            {                
                MTumikomiProc result;
                // 増便未対応
                await Task.Run(async () =>
                {
                    string url = "tumi/tumi080";
                    result = await WebService.PostRestAPI<MTumikomiProc>(url, param);

                    if (string.IsNullOrEmpty(result.poRet))
                    {
                        resultCode = 8;
                    }
                    else
                    {
                        resultCode = int.Parse(result.poRet);

                        if((int.Parse(mCarryEditText.Text) + int.Parse(mKargoEditText.Text)) == 1)
                        {
                            resultCode = 0;
                        }
                    }

                    return result;

                }).ContinueWith(x =>
                {
                    Activity.RunOnUiThread(() => {
                        ((MainActivity)this.Activity).EnableScanning();
                        DismissProgress();
                    });
                   
                    if (x.Result.poMsg != null && x.Result.poMsg != "")
                    {
                        ShowDialog("エラー", x.Result.poMsg, () =>
                        {
                            Activity.RunOnUiThread(() => mCarEditText.Text = "");
                        });
                        return;
                    }

                    //全部完了
                    if (resultCode == 0)
                    {
                        Task.Run(async () =>
                        {
                            return await WebService.PostRestAPI("tumi/tumi090", param);
                        }).ContinueWith(t => {

                            if (t.Result == null || t.Result["status"] != "1")
                            {
                                Activity.RunOnUiThread(() => {
                                    ShowDialog("エラー", "積込完了時間更新に失敗しました。", () => {

                                    });
                                });

                                return;
                            }

                            //	正常登録
                            PlayBeepOk();
                            ShowDialog("報告", "積込検品が\n完了しました。", () =>
                            {
                                int haisoCount = Arguments.GetInt("haisoCount", 1);
                                int page = haisoCount == 1 ? 0 : 2;

                                FragmentManager.PopBackStack(FragmentManager.GetBackStackEntryAt(page).Id, 0);
                            });
                        });
                    }
                    else if (resultCode == 1)
                    {
                        Activity.RunOnUiThread(() =>
                        {
                            mKosuEditText.SetBackgroundColor(Android.Graphics.Color.Yellow);
                            mCarEditText.SetBackgroundColor(Android.Graphics.Color.White);

                            if (tempCategory == "1")
                            {
                                mCarryEditText.Text = (int.Parse(mCarryEditText.Text) - 1).ToString();
                            }
                            else if (tempCategory == "2")
                            {
                                mKargoEditText.Text = (int.Parse(mKargoEditText.Text) - 1).ToString();
                            }

                            tempCategory = "";
                            mKosuEditText.Text = "";
                            mCarEditText.Text = "";

                            carLabelInputMode = false;
                            
                            IsUpdated = true;

                        });
                    }
                });
            }
            catch
            {
                ((MainActivity)this.Activity).DismissDialog();
                
                ShowDialog("エラー", "更新できませんでした。\n管理者に連絡してください。", () => {
                    ((MainActivity)this.Activity).EnableScanning();
                });
                return;
            }
        }

        [Obsolete]
        public override bool OnBackPressed()
        {
            if (carLabelInputMode)
            {
                carLabelInputMode = false;
                mKosuEditText.Text = "0";
                mKosuEditText.SetBackgroundColor(Android.Graphics.Color.Yellow);
                mCarEditText.SetBackgroundColor(Android.Graphics.Color.White);
            }
            else
            {
                if (IsUpdated)
                {
                    string dialogMsg = "積込を終了してもいいですか？";
                    ConfirmDialog(dialogMsg, (flag) => {
                        if (flag)
                        {
                            var param = GetProcParam("");

                            Task.Run(async () =>
                            {
                                return await WebService.PostRestAPI("tumi/tumi090", param);
                            }).ContinueWith(t => {

                                if (t.Result == null || t.Result["status"] != "1")
                                {
                                    Activity.RunOnUiThread(() => {
                                        ShowDialog("エラー", "積込完了時間更新に失敗しました。", () => {
                                            FragmentManager.PopBackStack();
                                        });
                                    });

                                    return;
                                }
                                else
                                {
                                    FragmentManager.PopBackStack();
                                }
                            });
                        }
                    });
                }
                else
                {
                    return true;
                }

            }

            return false;
        }

        // PROC専用のパラメータ設定
        private Dictionary<string, string> GetProcParam(string barcodeData)
        {
            return new Dictionary<string, string>
            {
                { "pTerminalID",  prefs.GetString("terminal_id","")},
                { "pProgramID", "TUM" },
                { "pSagyosyaCD", prefs.GetString("sagyousya_cd","") },
                { "pSoukoCD",  souko_cd},
                { "pSyukaDate", syuka_date},
                { "pBinNo", bin_no},
                { "pCourse", course },
                { "pMatehan", matehan },
                { "pTokuisakiCD", tokuisaki_cd },
                { "pTodokesakiCD", todokesaki_cd },
                { carLabelInputMode == false ? "pKamotsuNo" : "pSyaryoNo", barcodeData },
                { "pHHT_No", prefs.GetString("hht_no","") }
            };
        }
    }
}
 