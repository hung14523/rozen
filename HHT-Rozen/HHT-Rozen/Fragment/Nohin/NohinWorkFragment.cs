using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Beardedhen.Androidbootstrap;
using Com.Densowave.Bhtsdk.Barcode;
using HHT.Resources.Model;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class NohinWorkFragment : BaseFragment
    {
        private readonly string TAG = "NohinWorkFragment";
        private readonly string COMPLETE_MSG = "納品検品が\n完了しました。\n\nお疲れ様でした！";
        
        TextView mCaseTextView;                     //　ケース
        TextView mOriconTextView;                   //　オリコン
        TextView mSonotaTextView;                   //　その他
        TextView mIdoTextView;                      //　移動
        TextView mMailTextView;                     //　メール
        TextView mFuteikeiTextView;                 //　不定形
        TextView mHansokuTextView;                  //　販促
        TextView mTcTextView;                       //　TC
        TextView mTsumidaiTextView;                 //　積込台数
        TextView mKosuAllTextView;                  //　総個数
        TextView mMatehanNameTextView;              //　マテハン名（削除？）
        TextView mTdkTextView;                      //  届先
        BootstrapButton confirmButton;

        // 臨時変数
        private int ko_su;                          //　納品済個数
        private int maxko_su;                       //　納品総個数
        private int matehanCnt;                     //　マテハン数

        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region #View
            View view = inflater.Inflate(Resource.Layout.fragment_nohin_work, container, false);
            mTdkTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinwork_todokesakiNm);
            mCaseTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_case);
            mOriconTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_oricon);
            mSonotaTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_sonota);
            mIdoTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_ido);
            mMailTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_mail);
            mFuteikeiTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_futeikei);
            mHansokuTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_hansoku);
            mTcTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_tc);
            mTsumidaiTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_tsumidaisu);
            mKosuAllTextView = view.FindViewById<TextView>(Resource.Id.txt_nohinWork_all);
            mMatehanNameTextView = view.FindViewById<TextView>(Resource.Id.matehanNm);
            confirmButton = view.FindViewById<BootstrapButton>(Resource.Id.nohinButton);
            #endregion

            SetActionBarTitle("納品検品");
            mTdkTextView.Text = prefs.GetString("tokuisaki_nm", "");
            confirmButton.Click += delegate { CompleteNohinButtonClicked(); };

            GetTumisuCount(); // 積込台数取得
            
            return view;
        }

        [System.Obsolete]
        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                // スキャン無効化
                ((MainActivity)this.Activity).DisableScanning();

                ProceedNohin(barcodeData.Data);
            }
        }

        [System.Obsolete]
        private void GetTumisuCount()
        {
            string tokuisaki_cd = prefs.GetString("tokuisaki_cd", "");
            string todokesaki_cd = prefs.GetString("todokesaki_cd", "");

            // 積込台数
            ((MainActivity)this.Activity).ShowProgress("");
            Task.Run(async () =>
            {
                var param = new Dictionary<string, string>()
                {
                    { "syuka_date", prefs.GetString("nohin_date","") },
                    { "tokuisaki_cd", tokuisaki_cd },
                    { "todokesaki_cd", todokesaki_cd },
                    { "course", prefs.GetString("course","") },
                    { "bin", prefs.GetString("binNo","") },
                    { "jun", prefs.GetString("jun","") },
                    { "code", prefs.GetString("code","") },
                    { "seq", prefs.GetString("seq","") },
                };

                var result = await WebService.PostRestAPI<Nohin030>("nohin/nohin020", param);
                
                maxko_su = result.kamotsuCount;
                ko_su = result.sumiCount;
                matehanCnt = result.mateCount;

                if (maxko_su == 0)
                {
                    ShowDialog("エラー", "納品可能なマテハンが存在しません。",
                        () => FragmentManager.PopBackStack());
                    return;
                }
                else if(maxko_su == ko_su)
                {
                    ShowDialog("報告", "納品処理は\n終了しています。", 
                        () => FragmentManager.PopBackStack());
                    return;
                }

                Activity.RunOnUiThread(() => {
                    mTsumidaiTextView.Text = result.sumiMateCount + "/" + result.mateCount; // 積込台数
                    mKosuAllTextView.Text = result.sumiCount + "/" + result.kamotsuCount; // 総個数

                    UpdateListBunrui(result);
                });
            }).ContinueWith(t => Activity.RunOnUiThread(() => ((MainActivity)this.Activity).DismissDialog()));
        }

        [System.Obsolete]
        private void CompleteNohinButtonClicked()
        {
            Log.Debug(TAG, "MAIN NOHIN COMPLETE");
            editor.PutString("menu_flg", "2");
            editor.PutBoolean("nohinWorkEndFlag", true);
            editor.Apply();


            Task.Run(async () =>
            {
                string syukaDate = prefs.GetString("nohin_date", "");
                string tokcd = prefs.GetString("tokuisaki_cd", "");
                string tdkcd = prefs.GetString("todokesaki_cd", "");
                string sagyosyaCd = prefs.GetString("sagyousya_cd", "");

                // 店着時間を更新
                var param = new Dictionary<string, string>
                {
                    {"nohinDate", syukaDate },
                    {"tokcd", tokcd },
                    {"tdkcd", tdkcd },
                    { "course", prefs.GetString("course","") },
                    { "bin", prefs.GetString("binNo","") },
                    { "jun", prefs.GetString("jun","") },
                    { "code", prefs.GetString("code","") },
                    { "seq", prefs.GetString("seq","") },
                    { "pSagyosyaCD", sagyosyaCd },
                    { "nohinFlag", "1" },
                };

                return await WebService.PostRestAPI("nohin/nohin080", param);

            }).ContinueWith(t => {

                if (t.Result == null || t.Result["status"] != "1")
                {
                    Activity.RunOnUiThread(() => {
                        ShowDialog("エラー", "発日時更新に失敗しました。", () => {

                        });
                    });

                    return;
                }

                Activity.RunOnUiThread(() => {
                    PlayBeepOk();
                    ShowDialog("報告", COMPLETE_MSG, () => FragmentManager.PopBackStack());
                });
            });

        }

        private void UpdateListBunrui(Nohin030 nohin030)
        {
            mOriconTextView.Text = (int.Parse(mOriconTextView.Text) + nohin030.category0).ToString();
            mCaseTextView.Text = (int.Parse(mCaseTextView.Text) + nohin030.category1).ToString();
            mFuteikeiTextView.Text = (int.Parse(mFuteikeiTextView.Text) + nohin030.category2).ToString();

            mTcTextView.Text = (int.Parse(mTcTextView.Text) + nohin030.category5).ToString();
            mHansokuTextView.Text = (int.Parse(mHansokuTextView.Text) + nohin030.category6).ToString();
            mMailTextView.Text = (int.Parse(mMailTextView.Text) + nohin030.category7).ToString();
            mIdoTextView.Text = (int.Parse(mIdoTextView.Text) + nohin030.category8).ToString();
            //default: mSonotaTextView.Text = (int.Parse(mSonotaTextView.Text) + count).ToString(); break;
        }

        [System.Obsolete]
        private void ProceedNohin(string kamotsu)
        {
            Activity.RunOnUiThread(() => ShowProgress(""));
            
            Task.Run(async () =>
            {
                string syukaDate = prefs.GetString("nohin_date", "");
                string tokcd = prefs.GetString("tokuisaki_cd", "");
                string tdkcd = prefs.GetString("todokesaki_cd", "");
                string sagyosyaCd = prefs.GetString("sagyousya_cd", "");

                var param = new Dictionary<string, string>()
                {
                    { "syuka_date", syukaDate },
                    { "tokuisaki_cd", tokcd },
                    { "todokesaki_cd", tdkcd },
                    { "kamotsu", kamotsu },
                    { "sagyosyaCD", sagyosyaCd },
                    { "course", prefs.GetString("course","") },
                    { "bin", prefs.GetString("binNo","") },
                    { "jun", prefs.GetString("jun","") },
                    { "code", prefs.GetString("code","") },
                    { "seq", prefs.GetString("seq","") },
                };

                try
                {
                    var result = await WebService.PostRestAPI<Nohin030>("nohin/nohin030", param);
                    if (result.matehanCd == "")
                    {
                        ShowDialog("エラー", "該当データがありません。", () => {
                            ((MainActivity)this.Activity).EnableScanning();
                        });
                        return;
                    }

                    if (!string.IsNullOrEmpty(result.message))
                    {
                        ShowDialog("エラー", result.message, () => {
                            ((MainActivity)this.Activity).EnableScanning();
                        });
                        return;
                    }

                    int idx = mTsumidaiTextView.Text.IndexOf('/');
                    int tsumidai = (int.Parse(mTsumidaiTextView.Text.Substring(0, idx)) + 1);

                    // 総個数
                    idx = mKosuAllTextView.Text.IndexOf('/');
                    ko_su = int.Parse(mKosuAllTextView.Text.Substring(0, idx)) + result.matehanKosu;

                    Activity.RunOnUiThread(() => {

                        ((MainActivity)this.Activity).EnableScanning();

                        mMatehanNameTextView.Text = result.matehanCd[result.matehanCd.Length - 1].ToString() == "1" ? "キャリー" : "かご車";

                        // 画面の数字をカウントアップする
                        mTsumidaiTextView.Text = tsumidai + "/" + matehanCnt;
                        mKosuAllTextView.Text = ko_su + "/" + maxko_su;

                        UpdateListBunrui(result);

                        if (ko_su == maxko_su)
                        {
                            confirmButton.Enabled = true;
                        }
                    });
                }
                catch
                {
                    ShowDialog("エラー", "該当データがありません。", () => {
                        ((MainActivity)this.Activity).EnableScanning();
                    });
                    return;
                }
                

            }).ContinueWith(t => {
                Activity.RunOnUiThread(() => DismissProgress());
            });
        }

        

    }
}