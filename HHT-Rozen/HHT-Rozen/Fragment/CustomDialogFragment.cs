using System;
using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Beardedhen.Androidbootstrap;
using Com.Beardedhen.Androidbootstrap.Api.Defaults;

namespace HHT_Rozen.Fragment
{
    public class CustomDialogFragment : AndroidX.Fragment.App.DialogFragment
    {
        public event DialogEventHandler Dismissed;

        private static readonly string ARG_DIALOG_MAIN_MSG = "dialog_main_msg";
        private String mMainMsg;
        private Dialog dialog;

        public static CustomDialogFragment NewInstance(String mainMsg)
        {
            Bundle bundle = new Bundle();
            bundle.PutString(ARG_DIALOG_MAIN_MSG, mainMsg);
            CustomDialogFragment fragment = new CustomDialogFragment
            {
                Arguments = bundle
            };

            return fragment;
        }

        public override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            if (Arguments != null) mMainMsg = Arguments.GetString("ARG_DIALOG_MAIN_MSG");
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            AlertDialog.Builder builder = new AlertDialog.Builder(Activity);
            View view = Activity.LayoutInflater.Inflate(Resource.Layout.dialog_custom2, null);

            string title = Arguments.GetString("title");
            string body = Arguments.GetString("body");

            TextView _TitleTextView = ((TextView)view.FindViewById(Resource.Id.title));
            TextView _MessageTextView = ((TextView)view.FindViewById(Resource.Id.message));
            _TitleTextView.Text = title;
            _MessageTextView.Text = body;

            BootstrapButton button = view.FindViewById<BootstrapButton>(Resource.Id.okButton);
            button.Click += delegate {
                Dismissed?.Invoke(this, new DialogEventArgs
                {
                    Text = "true"
                });
                Dismiss();
            };

            BootstrapButton cancelButton = view.FindViewById<BootstrapButton>(Resource.Id.cancelButton);
            cancelButton.BootstrapBrand = DefaultBootstrapBrand.Regular;
            cancelButton.Click += delegate {
                Dismissed(this, new DialogEventArgs
                {
                    Text = "false"
                });
                Dismiss();
            };

            if (title == "エラー")
            {
                _TitleTextView.SetBackgroundColor(Color.Red);
                button.BootstrapBrand = DefaultBootstrapBrand.Danger;
                cancelButton.Visibility = ViewStates.Gone;                
            }
            else if(title == "警告")
            {
                _TitleTextView.SetBackgroundColor(Color.Yellow);
                _TitleTextView.SetTextColor(Color.Black);
                button.BootstrapBrand = DefaultBootstrapBrand.Warning;
            }
            else if(title == "報告")
            {
                cancelButton.Visibility = ViewStates.Gone;
            }
            else if (title == "満タン完了")
            {
                _TitleTextView.SetBackgroundColor(Color.Yellow);
                _TitleTextView.SetTextColor(Color.Black);
                button.BootstrapBrand = DefaultBootstrapBrand.Warning;
            }
            else if (title == "noButton")
            {
                _TitleTextView.Text = "回収入力しますか？";
                _TitleTextView.SetBackgroundColor(Color.Yellow);
                _TitleTextView.SetTextColor(Color.Black);
                button.BootstrapBrand = DefaultBootstrapBrand.Warning;
                button.Visibility = ViewStates.Gone;
                cancelButton.Visibility = ViewStates.Gone;
            }


            //　確認ダイアログではない場合、振動を起こす
            if (title == "エラー"　|| title == "報告")
            {
                Vibrate();
            }

            builder.SetView(view);

            dialog = builder.Create();
            dialog.KeyPress += (sender, e) =>{

                if (title == "noButton")
                {
                    if(e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Num9)
                    {
                        button.CallOnClick();
                    }
                    else if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Num1)
                    {
                        cancelButton.CallOnClick();
                    }

                    e.Handled = false;
                }
                else if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    button.CallOnClick();
                    e.Handled = false;
                }
                else
                {
                    e.Handled = false;
                }
            };
            
            return dialog;
        }
        
        public class DialogEventArgs : EventArgs
        {
            public string Text { get; set; }
        }

        public delegate void DialogEventHandler(object sender, DialogEventArgs args);

        private void Vibrate()
        {
#pragma warning disable CS0618 // 型またはメンバーが古い形式です
            //((Vibrator)Activity.GetSystemService(Android.Content.Context.VibratorService)).Vibrate(1000);
#pragma warning restore CS0618 // 型またはメンバーが古い形式です
        }
    }
}