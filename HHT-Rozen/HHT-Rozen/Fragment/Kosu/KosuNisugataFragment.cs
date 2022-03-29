using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class KosuNisugataFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_kosu_nisugata_kubun, container, false);

            Button mOriconButton = view.FindViewById<Button>(Resource.Id.oriconButton);
            Button mCaseButton = view.FindViewById<Button>(Resource.Id.caseButton);
            Button mFuteikeiButton = view.FindViewById<Button>(Resource.Id.futeikeiButton);
            TextView mKamotsuNoTextView = view.FindViewById<TextView>(Resource.Id.kamotsuNo);

            mKamotsuNoTextView.Text = Arguments.GetString("barcode");

            mOriconButton.Click += delegate { ConfirmButton_clicked(0); };
            mCaseButton.Click += delegate { ConfirmButton_clicked(1); };
            mFuteikeiButton.Click += delegate { ConfirmButton_clicked(2); };

            return view;
        }

        [System.Obsolete]
        private void ConfirmButton_clicked(int index)
        {
            // index 0 : オリコン, 1: ケース, 2:不定形
            string barcode = Arguments.GetString("barcode");
            string matehancd = Arguments.GetString("matehancd");
            string kbn = Arguments.GetString("matekbn");

            if (kbn == "1")
            {
                // キャリー
                string mCarryBarcodes = prefs.GetString("mCarryBarcodes", "");
                string mCarryCaseKbns = prefs.GetString("mCarryCaseKbns", "");

                if (mCarryCaseKbns.Length != 0)
                {
                    mCarryCaseKbns += "," + index.ToString();
                    mCarryBarcodes += "," + barcode;
                }
                else
                {
                    mCarryCaseKbns = index.ToString();
                    mCarryBarcodes = barcode;
                }
                
                editor.PutString("mCarryBarcodes", mCarryBarcodes);
                editor.PutString("mCarryCaseKbns", mCarryCaseKbns);
                editor.Apply();

                FragmentManager.PopBackStack();
                return;
            }
            else
            {
                // かご車
                var param = new Dictionary<string, string>
                {
                    { "pSyukaDate", prefs.GetString("syuka_date","")},
                    { "pKamotsuNo", barcode},
                    { "tantoCd", prefs.GetString("sagyousya_cd","")},
                    { "pMatehan", matehancd},
                    { "handyId", Build.Serial},
                    { "caseKbn", index.ToString() }
                };

                var task1 = Task.Run(async () => { return await PostRestAPI("", "kosu/kosu020", param); });

                try
                {
                    task1.Wait();
                    var result = task1.Result;

                    if (result.ContainsKey("errorMsg"))
                    {
                        ShowDialog("エラー", result["errorMsg"], () => { });
                        return;
                    }

                    string kosu = result["kosu"];
                    string tenpoName = result["tokNm"];

                    ShowDialog("報告", tenpoName + "\n" + kosu + "個口", () => {
                        FragmentManager.PopBackStack();
                    });
                }
                catch
                {
                    ShowDialog("エラー", "エラーが発生しました。", () => { });
                }
            }
        }
    }
}