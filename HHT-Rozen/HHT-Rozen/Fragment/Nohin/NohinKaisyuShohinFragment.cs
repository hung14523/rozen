using Android.OS;
using Android.Views;
using Com.Beardedhen.Androidbootstrap;
using HHT;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class NohinKaisyuShohinFragment : BaseFragment
    {
        private BootstrapEditText mTenidoEditText;
        private BootstrapEditText mHenpinEditText;
        private BootstrapEditText mMailEditText;
        private BootstrapButton mConFirmButton;
        
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region #ViewSetting
            View view = inflater.Inflate(Resource.Layout.fragment_nohin_kaisyu_shohin, container, false);
            mTenidoEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_Kaisyu_tenido);
            mHenpinEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_Kaisyu_henpin);
            mMailEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_Kaisyu_mail);
            mConFirmButton = view.FindViewById<BootstrapButton>(Resource.Id.btn_nohinKaisyuShohin_confirm);
            SetActionBarTitle("商品回収");
            #endregion

            mConFirmButton.Click += MConFirmButton_Click;
            mMailEditText.KeyPress += (sender, e) => {
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    e.Handled = true;
                    CommonUtils.HideKeyboard(Activity);
                    mConFirmButton.CallOnClick();
                }
                else
                {
                    e.Handled = false;
                }
            };

            return view;
        }

        [System.Obsolete]
        private void MConFirmButton_Click(object sender, System.EventArgs e)
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
                { "kbn", "0" },
                { "tenidoSu", mTenidoEditText.Text == "" ? "0" : mTenidoEditText.Text},
                { "henpinSu", mHenpinEditText.Text == "" ? "0" : mHenpinEditText.Text},
                { "daikoSu", "0"},
                { "hansokuSu", "0"},
                { "mailSu", mMailEditText.Text == "" ? "0" : mMailEditText.Text},
                { "yabuzaSu", "0"},

            };

            ShowDialog("確認", "よろしいですか？", (okFlag) => {
                if (okFlag == false)
                {
                    return;
                }

                Task.Run(async () => {
                    var result = await WebService.PostRestAPI("nohin/nohin060", param);
                    if (result == null || result.ContainsKey("msg"))
                    {
                        ErrorDialog("システムエラーが発生しました。", () => { });
                    }
                    else
                    {
                        NoticeDialog("正常に登録できました。", () => { FragmentManager.PopBackStack(); });
                    }
                });
            });
        }
    }
}