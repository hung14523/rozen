using Android.OS;
using Android.Views;
using Android.Widget;

namespace HHT_Rozen.Fragment
{
    public class NohinMenuFragment : BaseFragment
    {
        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_menu_nohin, container, false);

            SetActionBarTitle("納品検品");

            Button button2 = view.FindViewById<Button>(Resource.Id.btn_nohinMenu_nohin);
            button2.Click += delegate {
                StartFragment(FragmentManager, typeof(NohinWorkFragment));
            };

            Button button3 = view.FindViewById<Button>(Resource.Id.btn_nohinMenu_kaisyu); // 回収業務
            button3.Click += delegate {
                StartFragment(FragmentManager, typeof(NohinKaisyuMenuFragment));
            };

            return view;
        }

        [System.Obsolete]
        public override bool OnKeyDown(Keycode keycode, KeyEvent paramKeyEvent)
        {
            if (keycode == Keycode.Num1)
            {
                StartFragment(FragmentManager, typeof(NohinWorkFragment));
            }
            else if (keycode == Keycode.Num2)
            {
                StartFragment(FragmentManager, typeof(NohinKaisyuMenuFragment));
            }

            return true;
        }

        [System.Obsolete]
        public override bool OnBackPressed()
        {
            ShowDialog("noButton", "キーを押してください\n\n１：はい\n９：店舗選択へ", (flag) => {
                if (flag)
                {
                    FragmentManager.PopBackStack();
                }
            });
            return false;
        }
    }
}
 