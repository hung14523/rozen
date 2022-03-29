using Android.App;
using Android.OS;
using Android.Util;
using Android.Views;
using Android.Widget;
using Com.Beardedhen.Androidbootstrap;
using Com.Densowave.Bhtsdk.Barcode;
using HHT.Resources.Model;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class TsumikaeIdouSakiFragment : BaseFragment
    {
        private readonly string TAG = "TsumikaeIdouSakiFragment";

        private TextView mCaseTextView;
        private TextView mOriconTextView;
        private TextView mIdoTextView;           //店移動
        private TextView mMailTextView;          //メール
        private TextView mFuteikeiTextView;
        private TextView mHansokuTextView;
        private TextView mHaisoDaikoTextView;    // 配送代行
        private TextView mTotalKosuTextView;
        private BootstrapButton mConfirmButton;
        
        private int menuFlag, btvScnFlg;
        
        private List<string> motokamotuList;

        private string sakiKamotsuNo;
        List<Ido> motoInfoList;

        private readonly int TANPIN_IDO = 1;    //単品移動
        private readonly int ZENPIN_IDO = 2;
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region #View setting
            View view = inflater.Inflate(Resource.Layout.fragment_tsumikae_Idou_saki, container, false);

            SetActionBarTitle("移動先");
            mCaseTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_case);
            mOriconTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_oricon);
            mIdoTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_idosu);
            mMailTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_mail);
            mFuteikeiTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_futeikei);
            mHansokuTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_hansoku);
            mHaisoDaikoTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_tc);
            mTotalKosuTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_kosu);
            mConfirmButton = view.FindViewById<BootstrapButton>(Resource.Id.confirmButton);
            #endregion

            menuFlag = prefs.GetInt("menuFlag", TANPIN_IDO);
            btvScnFlg = 0;
            
            mConfirmButton.Click += delegate{ if(btvScnFlg > 0) { CompleteIdou(); }};
            motoInfoList = JsonConvert.DeserializeObject<List<Ido>>(Arguments.GetString("motoInfo"));

            return view;
        }

        [System.Obsolete]
        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                string kamotsu_no = barcodeData.Data;

                if (btvScnFlg > 0)
                {
                    ShowDialog("エラー", "既にスキャン済みです", () => { });
                    Log.Debug(TAG, "既にスキャン済みです barcode :" + kamotsu_no);
                    return;
                }

                if (kamotsu_no.Length != 20)
                {
                    ShowDialog("エラー", "正しいバーコードをスキャンしてください。", () => { });
                    Log.Debug(TAG, "正しいバーコードをスキャンしてください barcode :" + kamotsu_no);
                    return;
                }

                // 移動先バーコードチェック
                Task.Run(async () =>
                {
                    if (!await CheckScanNo(kamotsu_no)) return;

                    var param = new Dictionary<string, string>()
                    {
                        {"kamotsuNo" ,kamotsu_no }
                    };

                    var result = await WebService.PostRestAPI<IDOU020>("ido/ido020", param);

                    #region ido020 return check
                    if (result == null)
                    {
                        ShowDialog("報告", "表示データがありません", () => { });
                        return;
                    }

                    if (result.errorMsg != "")
                    {
                        ShowDialog("エラー", result.errorMsg, () => { });
                        return;
                    }

                    string mMateCdString = prefs.GetString("motomate_cd", "");
                    if (mMateCdString.Split(',').Contains(result.matehan))
                    {
                        ShowDialog("エラー", "同一のマテハンです", () => { });
                        return;
                    }
                    #endregion

                    sakiKamotsuNo = kamotsu_no;
                    btvScnFlg = 1;

                    Activity.RunOnUiThread(() => {
                        
                        // スキャンした個口数表示
                        mCaseTextView.Text = result.caseSu.ToString();
                        mOriconTextView.Text = result.oriconSu.ToString();
                        mFuteikeiTextView.Text = result.hutekeiSu.ToString();
                        mHaisoDaikoTextView.Text = result.haisodaikoSu.ToString();
                        mHansokuTextView.Text = result.hansokuSu.ToString();
                        mMailTextView.Text = result.mailSu.ToString();
                        mIdoTextView.Text = result.tenidoSu.ToString();
                        mTotalKosuTextView.Text = (result.GetTotal()).ToString();

                        mConfirmButton.Enabled = true;
                    });
                });
            }
        }

        [System.Obsolete]
        private async Task<bool> CheckScanNo(string kamotsu_no)
        {
            // 元と先マテハンが一致するか確認
            motokamotuList = prefs.GetStringSet("kamotuList", new List<string>()).ToList();
            if (motokamotuList.FindIndex(x => x == kamotsu_no) != -1)
            {
                ShowDialog("エラー", "同一のマテハンです。", () => { });
                return false;
            }

            var param = new Dictionary<string, string>
            {
                { "kamotsuNo", kamotsu_no }
            };

            IDOU033 kamotuInfo = await WebService.PostRestAPI<IDOU033>("ido/ido033", param);

            if(kamotuInfo.errorMsg != "")
            {
                ShowDialog("エラー", kamotuInfo.errorMsg, () => { });
                return false;
            }

            // 得意先、届先が一致するかを確認する
            if ((kamotuInfo.tokuisaki_cd == prefs.GetString("tmptokui_cd", ""))
                || prefs.GetString("tmptokui_cd", "") == "")
            {
                // Do nothing
            }
            else
            {
                ShowDialog("エラー", "届先が異なります。", () => { });
                return false;
            }

            return true;
        }

        [System.Obsolete]
        private void CompleteIdou()
        {
            // 積替え確定処理 (単品移動：ido050、全品移動：ido060)
            if (menuFlag == TANPIN_IDO)
            {
                ((MainActivity)Activity).ShowProgress("");
                Task.Run(async () =>
                {
                    bool isok = true;
                    foreach (Ido motoInfo in motoInfoList)
                    {
                        var param = new Dictionary<string, string>
                        {
                            {"pSagyosyaCD", prefs.GetString("sagyousya_cd", "") },
                            {"mSyukaDate", prefs.GetString("syuka_date", "")},
                            {"pSakiKamotsuNo", sakiKamotsuNo },
                            {"pMotoKamotsuNo", motoInfo.kamotsuNo },
                        };

                        IDOU050 idou050 = await WebService.PostRestAPI<IDOU050>("ido/ido050", param);

                        if (idou050 == null || idou050.poRet != "0")
                        {
                            isok = false;
                        }
                    }

                    return isok;
                }).ContinueWith(t => {
                    Activity.RunOnUiThread(() => { 
                        ((MainActivity)this.Activity).DismissDialog();
                        
                        if (t.Result == true)
                        {
                            PlayBeepOk();
                            ShowDialog("報告", "移動処理が\n完了しました。", () => { BackToMainMenu(); });
                        }
                        else
                        {
                            ShowDialog("エラー", "データがありません。", () => { });
                        }
                    });
                });
            }
            else if (menuFlag == ZENPIN_IDO)
            {
                var param = new Dictionary<string, string>
                {
                    {"pSagyosyaCD", prefs.GetString("sagyousya_cd", "") },
                    {"mSyukaDate", prefs.GetString("syuka_date", "")},
                    {"pSakiKamotsuNo", sakiKamotsuNo },
                    {"pMotoMatehan", motoInfoList[0].motoMateCode }
                };

                Task.Run(async () => await WebService.PostRestAPI<IDOU060>("ido/ido060", param))
                    .ContinueWith(t => {
                        IDOU060 idou060 = t.Result;
                        if (idou060.poMsg != null && idou060.poMsg != "")
                        {
                            ShowDialog("エラー", idou060.poMsg, () => { });
                            return;
                        }

                        PlayBeepOk();
                        ShowDialog("報告", "移動処理が\n完了しました。", () => { BackToMainMenu(); });
                    });
            }
        }

        [System.Obsolete]
        private void BackToMainMenu()
        {
            Activity.RunOnUiThread(() => {
                FragmentManager.PopBackStack(FragmentManager.GetBackStackEntryAt(0).Id, 0);
            });
        }
    }
}