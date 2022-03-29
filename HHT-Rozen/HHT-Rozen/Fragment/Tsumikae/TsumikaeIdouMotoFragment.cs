using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Beardedhen.Androidbootstrap;
using Com.Densowave.Bhtsdk.Barcode;
using HHT;
using HHT.Resources.Model;
using Newtonsoft.Json;

namespace HHT_Rozen.Fragment
{
    public class TsumikaeIdouMotoFragment : BaseFragment
    {
        private TextView mCaseTextView;
        private TextView mOriconTextView;
        private TextView mIdoTextView;
        private TextView mMailTextView;
        private TextView mSonotaTextView;
        private TextView mFuteikeiTextView;
        private TextView mHansokuTextView;
        private TextView mTcTextView;
        private TextView mKosuTextView;
        private BootstrapButton mConfirmButton;
        private BootstrapButton mMatehanButton;

        private List<string> kamotuList;        // スキャンした貨物番号リスト
        private List<string> motomateCdList;    // スキャンした貨物のマテハン番号リスト
        private List<Ido> motoMateInfo;         // スキャンした貨物のマテハン番号リスト（重複？）

        private static readonly int TANPIN_IDO = 1;
        private static readonly int ZENPIN_IDO = 2;
        private static readonly int MATE_IDO = 3;

        private int menuFlag;

        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region #View
            View view = inflater.Inflate(Resource.Layout.fragment_tsumikae_Idou_moto, container, false);

            // タイトル設定
            SetActionBarTitle("移動元");

            mCaseTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_case);
            mOriconTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_oricon);
            mIdoTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_idosu);
            mMailTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_mail);
            mSonotaTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_sonota);
            mFuteikeiTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_futeikei);
            mHansokuTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_hansoku);
            mTcTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_tc);
            mKosuTextView = view.FindViewById<TextView>(Resource.Id.txt_tsumikae_kosu);
            mConfirmButton = view.FindViewById<BootstrapButton>(Resource.Id.confirmButton);
            mMatehanButton = view.FindViewById<BootstrapButton>(Resource.Id.mateButton);
            #endregion

            // メニュー区分
            menuFlag = prefs.GetInt("menuFlag", TANPIN_IDO);
            mMatehanButton.Visibility = menuFlag == TANPIN_IDO ? ViewStates.Visible : ViewStates.Gone;
            
            // 確定ボタン: スキャン後活性して他の画面へ遷移
            mConfirmButton.Click += delegate {
                Bundle bundle = new Bundle();
                bundle.PutString("motoInfo", JsonConvert.SerializeObject(motoMateInfo));

                StartFragment(FragmentManager
                    , menuFlag == TANPIN_IDO || menuFlag == ZENPIN_IDO
                        ? typeof(TsumikaeIdouSakiFragment) : typeof(TsumikaeIdouMatehanFragment), bundle);
            };

            // マテハンボタン
            mMatehanButton.Click += delegate
            {
                Bundle bundle = new Bundle();
                bundle.PutString("motoInfo", JsonConvert.SerializeObject(motoMateInfo));

                StartFragment(FragmentManager, typeof(TsumikaeIdouMatehanFragment), bundle);
            };

            // 当画面だけの仕様でCODE128は読めないようにする。
            ((MainActivity)Activity).ResizeBarcodeLength(20);

            kamotuList = new List<string>();
            motomateCdList = new List<string>();
            motoMateInfo = new List<Ido>();

            return view;
        }

        public override bool OnKeyDown(Keycode keycode, KeyEvent paramKeyEvent)
        {
            if (keycode == Keycode.Back)
            {
                editor.PutString("case_su", "0");
                editor.PutString("sk_case_su", "0");
                editor.PutString("oricon_su", "0");
                editor.PutString("sk_oricon_su", "0");
                editor.PutString("ido_su", "0");
                editor.PutString("sk_ido_su", "0");
                editor.PutString("mail_su", "0");
                editor.PutString("sk_mail_su", "0");
                editor.PutString("sonota_su", "0");
                editor.PutString("sk_sonota_su", "0");
                editor.PutString("futeikei_su", "0");
                editor.PutString("sk_futeikei_su", "0");
                editor.PutString("hansoku_su", "0");
                editor.PutString("sk_hansoku_su", "0");
                editor.PutString("sonota_su", "0");
                editor.PutString("sk_sonota_su", "0");

                editor.PutString("ko_su", "0");

                editor.PutString("motok_su", "0");

                editor.PutStringSet("kamotuList", new List<string>());
                editor.Apply();
            }
            
            return true;
        }

        [System.Obsolete]
        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                string kamotsuNo = barcodeData.Data;

                if (menuFlag == ZENPIN_IDO || menuFlag == MATE_IDO)
                {
                    if(mKosuTextView.Text != "0")
                    {
                        ShowDialog("エラー", "既にスキャン済みです", () => { });
                        return;
                    }
                }

                // スキャン重複チェック
                if (kamotuList.FindIndex(x => x == kamotsuNo) != -1)
                {
                    Activity.RunOnUiThread(() => ShowDialog("エラー", "同一の商品です。", () => { }));
                    return;
                }

                // 配送先チェック
                if (kamotuList.Count > 0)
                {
                    if (kamotuList[0].Substring(7, 4) != kamotsuNo.Substring(7, 4))
                    {
                        Activity.RunOnUiThread(() => ShowDialog("エラー", "配送先が違います。", () => { }));
                        return;
                    }
                }

                // スキャン無効化
                ((MainActivity)Activity).DisableScanning();
                Activity.RunOnUiThread(() => ((MainActivity)Activity).ShowProgress(""));
                
                Task.Run(async() => {

                    // スキャン処理
                    if (menuFlag == TANPIN_IDO) // 単品移動
                    {
                        await SettingTanpinMotoInfo(kamotsuNo);
                    }
                    else // 全品移動, マテハン移動
                    {
                        await SettingZenpinMotoInfo(kamotsuNo);
                    }

                }).ContinueWith((x) => {
                    Activity.RunOnUiThread(() => ((MainActivity)Activity).DismissDialog());
                    ((MainActivity)this.Activity).EnableScanning();
                });
                
            }
        }

        // 単品移動
        [System.Obsolete]
        private async Task SettingTanpinMotoInfo(string kamotsu_no)
        {
            try
            {
                // 貨物番号に紐づく情報を取得する
                IDOU033 idou033 = await GetKamotsuInfo(kamotsu_no);
                if(idou033 == null)
                {
                    return;
                }

                SetMatehanValue(idou033.bunrui, 1);
                motomateCdList.Add(idou033.matehan);
                kamotuList.Add(kamotsu_no);
                motoMateInfo.Add(new Ido { kamotsuNo = kamotsu_no, motoMateCode = idou033.matehan });
                
                string motomate = prefs.GetString("motomate_cd", "");
                motomate += motomate == "" ? idou033.matehan : "," + idou033.matehan;

                editor.PutString("motomate_cd", motomate);
                editor.PutStringSet("motomateCdList", motomateCdList);
                editor.PutStringSet("kamotuList", kamotuList);
                editor.Apply();

                Activity.RunOnUiThread(() => {
                    mConfirmButton.Enabled = true;
                    mMatehanButton.Enabled = true;
                });
            }
            catch
            {
                ShowDialog("エラー", "移動元の梱包Noが見つかりません。", () => { });
            }
        }

        // 全品移動、マテハン移動
        [System.Obsolete]
        private async Task SettingZenpinMotoInfo(string kamotsu_no)
        {
            try
            {
                var param = new Dictionary<string, string>()
                {
                    {"kamotsuNo", kamotsu_no }
                };

                var result = await WebService.PostRestAPI<IDOU020>("ido/ido020", param);
                if (result.errorMsg != "")
                {
                    ShowDialog("エラー", result.errorMsg, () => { });
                    return;
                }

                string matehan = result.matehan;
                List<string> kamotsuList = result.kamotsuList;

                Activity.RunOnUiThread(() => {
                    mCaseTextView.Text = result.caseSu.ToString();
                    mOriconTextView.Text = result.oriconSu.ToString();
                    mFuteikeiTextView.Text = result.hutekeiSu.ToString();
                    mHansokuTextView.Text = result.hansokuSu.ToString();
                    mMailTextView.Text = result.mailSu.ToString();
                    mTcTextView.Text = result.haisodaikoSu.ToString();
                    mIdoTextView.Text = result.tenidoSu.ToString();
                    mKosuTextView.Text = (result.GetTotal()).ToString();
                });
                
                foreach (string kamotsu in kamotsuList)
                {
                    motoMateInfo.Add(new Ido { kamotsuNo = kamotsu, motoMateCode = matehan });   
                }
                
                editor.PutString("tmptokui_cd", kamotsu_no.Substring(7, 4));
                editor.PutString("syuka_date", result.syukaDate);
                editor.PutString("motomate_cd", matehan);
                editor.PutStringSet("kamotuList", kamotsuList);
                editor.Apply();

                Activity.RunOnUiThread(() => mConfirmButton.Enabled = true);

            }
            catch {
                ShowDialog("エラー", "梱包Noが見つかりません。", () => { });
            }
        }
        
        private void SetMatehanValue(string bunrui, int addValue)
        {
            string addedValue = ""; //加算した値を保存
            Activity.RunOnUiThread(() =>
            {
                switch (bunrui)
                {
                    case "01":
                        addedValue = (int.Parse(mCaseTextView.Text) + addValue).ToString();
                        mCaseTextView.Text = addedValue.ToString();
                        break;
                    case "02":
                        addedValue = (int.Parse(mOriconTextView.Text) + addValue).ToString();
                        mOriconTextView.Text = addedValue.ToString();
                        break; // case 03は存在しない
                    case "03":
                        addedValue = (int.Parse(mFuteikeiTextView.Text) + addValue).ToString();
                        mFuteikeiTextView.Text = addedValue.ToString();
                        break;
                    case "05":
                        addedValue = (int.Parse(mTcTextView.Text) + addValue).ToString();
                        mTcTextView.Text = addedValue.ToString();
                        break;
                    case "06":
                        addedValue = (int.Parse(mHansokuTextView.Text) + addValue).ToString();
                        mHansokuTextView.Text = addedValue.ToString();
                        break;
                    case "07":
                        addedValue = (int.Parse(mMailTextView.Text) + addValue).ToString();
                        mMailTextView.Text = addedValue.ToString();
                        break;
                    case "08":
                        addedValue = (int.Parse(mIdoTextView.Text) + addValue).ToString();
                        mIdoTextView.Text = addedValue.ToString();
                        break;
                    default:
                        addedValue = (int.Parse(mSonotaTextView.Text) + addValue).ToString();
                        mSonotaTextView.Text = addedValue.ToString();
                        break;
                }

                mKosuTextView.Text = (int.Parse(mKosuTextView.Text) + addValue).ToString();
            });
        }

        [System.Obsolete]
        private async Task<IDOU033> GetKamotsuInfo(string kamotsu_no)
        {
            var param = new Dictionary<string, string>()
            {
                {"kamotsuNo", kamotsu_no }
            };

            var idou033 = await WebService.PostRestAPI<IDOU033>("ido/ido033", param);
            if (idou033.todokesaki_cd == "" || idou033.todokesaki_cd == null)
            {
                idou033.todokesaki_cd = "0";
            }

            if (idou033.errorMsg != "")
            {
                ShowDialog("エラー", idou033.errorMsg, () => { });
                return null;
            }

            editor.PutString("syuka_date", idou033.SyukaDate);
            editor.PutString("tmptokui_cd", idou033.tokuisaki_cd);
            editor.PutString("btvBunrui", idou033.bunrui);
            editor.PutString("bin_no", idou033.torikomi_bin);
            editor.Apply();

            return idou033;
        }
    }
}