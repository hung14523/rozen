using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Beardedhen.Androidbootstrap;
using HHT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class NohinKaisyuMatehanFragment : BaseFragment
    {
        BootstrapEditText mOriconSuEditText;
        BootstrapEditText mKagosyaSuEditText;
        BootstrapEditText mCarrySuEditText;
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_nohin_kaisyu_matehan, container, false);

            // コンポーネント初期化
            SetActionBarTitle("マテハン回収");

            mOriconSuEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_kaisyu_oricon);
            mKagosyaSuEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_kaisyu_Kagosya);
            mCarrySuEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_kaisyu_Carry);
            
            BootstrapButton _ConfirmButton = view.FindViewById<BootstrapButton>(Resource.Id.btn_nohinKaisyuMatehan_confirm);
            _ConfirmButton.Click += delegate { ConfirmMatehanKaisyu(); };

            mCarrySuEditText.KeyPress += (sender, e) => {
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    e.Handled = true;
                    CommonUtils.HideKeyboard(Activity);
                    ConfirmMatehanKaisyu();
                }
                else
                {
                    e.Handled = false;
                }
            };

            return view;
        }

        [System.Obsolete]
        private void ConfirmMatehanKaisyu()
        {
            ShowDialog("確認", "よろしいですか？", (okFlag) => {
                if (okFlag)
                {
                    Task.Run(async () =>
                    {
                        var param = new Dictionary<string, string>()
                        {
                            { "pTerminalID",  prefs.GetString("terminal_id","")},
                            { "pSagyosyaCD", prefs.GetString("sagyousya_cd","") },
                            { "pNohinDate", prefs.GetString("nohin_date", "")},
                            { "tokuisakiCd", prefs.GetString("tokuisaki_cd", "") },
                            { "todokesakiCd", prefs.GetString("todokesaki_cd", "") },
                            { "course", prefs.GetString("course","") },
                            { "bin", prefs.GetString("binNo","") },
                            { "jun", prefs.GetString("jun","") },
                            { "code", prefs.GetString("code","") },
                            { "seq", prefs.GetString("seq","") },
                            { "kbn", "1" },
                            { "oriconSu", mOriconSuEditText.Text == "" ? "0" : mOriconSuEditText.Text },
                            { "kargoSu", mKagosyaSuEditText.Text == "" ? "0" : mKagosyaSuEditText.Text},
                            { "carrySu", mCarrySuEditText.Text == "" ? "0" : mCarrySuEditText.Text}
                        };

                        return await WebService.PostRestAPI("nohin/nohin060", param);

                    }).ContinueWith(t => {

                        if (t.Result == null || t.Result.ContainsKey("msg"))
                        {
                            ErrorDialog("システムエラーが発生しました。", () => { });
                        }
                        else
                        {
                            NoticeDialog("正常に登録できました。", () => { FragmentManager.PopBackStack(); });
                        }
                    });
                }
            });
        }
    }
}