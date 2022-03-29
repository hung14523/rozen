using Android.OS;
using Android.Views;
using Android.Widget;

namespace HHT_Rozen.Fragment
{
    public class MeterMenuFragment : BaseFragment
    {
        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_menu_meter, container, false);

            SetActionBarTitle("メーター登録");
            Bundle bundle = new Bundle();

            Button button1 = view.FindViewById<Button>(Resource.Id.btn_meterMenu_start);
            button1.Click += delegate
            {
                bundle.PutBoolean("registFlg", true);
                StartFragment(FragmentManager, typeof(MeterSelectFragment), bundle);
            };

            Button button2 = view.FindViewById<Button>(Resource.Id.btn_meterMenu_end);
            button2.Click += delegate
            {
                bundle.PutBoolean("registFlg", false);
                StartFragment(FragmentManager, typeof(MeterSelectFragment), bundle);
            };

            return view;
        }

        [System.Obsolete]
        public override bool OnKeyDown(Keycode keycode, KeyEvent paramKeyEvent)
        {
            if (keycode == Keycode.Num1)
            {
                Bundle bundle = new Bundle();
                bundle.PutBoolean("registFlg", true);
                StartFragment(FragmentManager, typeof(MeterSelectFragment));
            }
            else if (keycode == Keycode.Num2)
            {
                Bundle bundle = new Bundle();
                bundle.PutBoolean("registFlg", false);
                StartFragment(FragmentManager, typeof(MeterSelectFragment));
            }

            return true;
        }
    }
}