using Android.OS;
using Android.Views;
using Android.Widget;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class KosuMenuFragment : BaseFragment
    {
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_menu_kosu, container, false);

            view.FindViewById<Button>(Resource.Id.arataCaseInspButton).Click += delegate { GoSelectPage(KOSUMENU.ARATA_CASE); };
            view.FindViewById<Button>(Resource.Id.arataOriconInspButton).Click += delegate { GoSelectPage(KOSUMENU.ARATA_ORICON); };

            SetActionBarTitle("紐づけ");

            return view;
        }

        public override bool OnKeyDown(Keycode keycode, KeyEvent paramKeyEvent)
        {
            if (keycode == Keycode.Num1)
            {
                GoSelectPage(KOSUMENU.ARATA_CASE);
            }
            else if (keycode == Keycode.Num2)
            {
                GoSelectPage(KOSUMENU.ARATA_ORICON);
            }

            return true;
        }

        private void GoSelectPage(KOSUMENU kosumenu)
        {
            Bundle bundle = new Bundle();
            bundle.PutInt("menu", (int)kosumenu);

            editor.Remove("mCarryBarcodes");
            editor.Remove("mCarryCaseKbns");
            editor.Apply();

            StartFragment(FragmentManager, typeof(KosuSelectFragment), bundle);
        }

        public override bool OnBackPressed()
        {
            Task.Run(async () => {
                // 業務終了
                var param = new Dictionary<string, string>() { { "scd", prefs.GetString("sagyousya_cd", "") }, { "kbn", "4" } };
                var res = await PostRestAPI("", "UpdateSagyoTime", param);
            });

            return base.OnBackPressed();

        }

    }
}