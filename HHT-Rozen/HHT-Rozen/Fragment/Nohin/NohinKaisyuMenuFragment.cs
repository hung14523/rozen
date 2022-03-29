using Android.OS;
using Android.Views;
using Android.Widget;
using HHT;

namespace HHT_Rozen.Fragment
{
    public class NohinKaisyuMenuFragment : BaseFragment
    {
        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            var view = inflater.Inflate(Resource.Layout.fragment_menu_nohin_kaisyu, container, false);

            SetActionBarTitle("回収業務");

            Button button1 = view.FindViewById<Button>(Resource.Id.btn_kaisyuMenu_sohin);
            button1.Click += delegate { StartFragment(FragmentManager, typeof(NohinKaisyuShohinFragment)); }; 

            Button button2 = view.FindViewById<Button>(Resource.Id.btn_kaisyuMenu_matehan);
            button2.Click += delegate { StartFragment(FragmentManager, typeof(NohinKaisyuMatehanFragment)); };

            return view;
        }

        [System.Obsolete]
        public override bool OnKeyDown(Keycode keycode, KeyEvent paramKeyEvent)
        {
            if (keycode == Keycode.Num1)
            {
                StartFragment(FragmentManager, typeof(NohinKaisyuShohinFragment));
            }
            else if (keycode == Keycode.Num2)
            {
                StartFragment(FragmentManager, typeof(NohinKaisyuMatehanFragment));
            }

            return true;
        }
    }
}