using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using Newtonsoft.Json;
using System.Collections.Generic;
using HHT.Resources.Model;
using System.Threading.Tasks;
using System.Linq;

namespace HHT_Rozen.Fragment
{
    public class TsumikaeIdouMatehanFragment : BaseFragment
    {
        private List<Ido> motoInfoList;
        private Button mKagosyaButton;
        private Button mCarryButton;

        private string handyId;
        private string sagyousyaCd;
        private string syukaDate;

        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_tsumikae_idou_matehan, container, false);

            mKagosyaButton = view.FindViewById<Button>(Resource.Id.kagosyaButton);
            mCarryButton = view.FindViewById<Button>(Resource.Id.carryButton);

            motoInfoList = JsonConvert.DeserializeObject<List<Ido>>(Arguments.GetString("motoInfo"));
            handyId = prefs.GetString("terminal_id", "");
            sagyousyaCd = prefs.GetString("sagyousya_cd", "");
            syukaDate = prefs.GetString("syuka_date", "");

            mKagosyaButton.Click += delegate { SelectListViewItem(0); };
            mCarryButton.Click += delegate { SelectListViewItem(1); };
          
            return view;
        }

        [System.Obsolete]
        public override bool OnKeyDown(Keycode keycode, KeyEvent paramKeyEvent)
        {
            if (keycode == Keycode.Num1)
            {
                SelectListViewItem(0);
            }
            else if(keycode == Keycode.Num2)
            {
                SelectListViewItem(1);
            }

            return true;
        }

        [System.Obsolete]
        public void SelectListViewItem(int index)
        {
            string msg = "";
            string bunrui = "";
            if (index == 0) 
            { 
                msg = "かご車でよろしいですか？";
                bunrui = "2";
            }
            else
            {
                msg = "キャリーでよろしいですか？";
                bunrui = "1";
            }

            ShowDialog("警告", msg, async (okFlag) =>
            {
                if (okFlag)
                {
                    await GroupingMatehanAsync(bunrui);
                }
            });
        }

        [System.Obsolete]
        private async Task GroupingMatehanAsync(string bunrui)
        {
            var param = new Dictionary<string, string>
            {
                {"pTerminalID", handyId },
                {"pSagyosyaCD", sagyousyaCd },
                {"pSyukaDate", syukaDate },
                {"bunrui", bunrui.ToString() }
            };

            // 1：キャリー,  2: かご車
            string kamotsuList = string.Join(",", motoInfoList.Select(x => x.kamotsuNo));
            param.Add("pMotoKamotsuNo", kamotsuList);

            var idou090 = await WebService.PostRestAPI("ido/ido090", param);
            string errorMsg = "";

            if (idou090 == null || idou090["poRet"] != "0")
            {
                errorMsg = "エラーが発生しました。";
            }

            Activity.RunOnUiThread(() => {
                if (string.IsNullOrEmpty(errorMsg))
                {
                    ShowDialog("報告", "移動処理が\n完了しました。",
                        () => FragmentManager.PopBackStack(FragmentManager.GetBackStackEntryAt(0).Id, 0));
                }
                else
                {
                    ShowDialog("エラー", errorMsg, () => { });
                }
            });
            
        }
    }
}
 