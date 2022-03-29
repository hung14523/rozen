using Android.Content;
using Android.OS;
using Android.Preferences;
using Android.Views;
using Android.Widget;

namespace HHT_Rozen.Fragment
{
    public class TsumikaeMenuFragment : BaseFragment
    {
        private Button tanpinButton;
        private Button zenpinButton;
        private Button matehanButton;

        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_menu_tsumikae, container, false);
            tanpinButton = view.FindViewById<Button>(Resource.Id.btn_tsumikaeMenu_tanpin);
            zenpinButton = view.FindViewById<Button>(Resource.Id.btn_tsumikaeMenu_zenpin);
            matehanButton = view.FindViewById<Button>(Resource.Id.btn_tsumikaeMenu_matehan);

            SetActionBarTitle("積替移動");
            
            tanpinButton.Click += delegate {
                editor.PutString("tmptokui_cd", "");
                editor.PutInt("menuFlag", 1);
                editor.Apply();
                StartFragment(FragmentManager, typeof(TsumikaeIdouMotoFragment));
            };
            
            zenpinButton.Click += delegate {
                editor.PutString("tmptokui_cd", "");
                editor.PutInt("menuFlag", 2);
                editor.Apply();
                StartFragment(FragmentManager, typeof(TsumikaeIdouMotoFragment));
            };

            matehanButton.Click += delegate {
                editor.PutString("tmptokui_cd", "");
                editor.PutInt("menuFlag", 3);
                editor.Apply();
                StartFragment(FragmentManager, typeof(TsumikaeIdouMotoFragment));
            };

            return view;
        }

        public override bool OnKeyDown(Keycode keycode, KeyEvent paramKeyEvent)
        {
            if (keycode == Keycode.Num1)
            {
                tanpinButton.CallOnClick();
            }
            else if (keycode == Keycode.Num2)
            {
                zenpinButton.CallOnClick();
            }
            else if (keycode == Keycode.Num3)
            {
                matehanButton.CallOnClick();
            }

            return true;
        }

    }
}