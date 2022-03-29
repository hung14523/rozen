using Android.App;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Densowave.Bhtsdk.Barcode;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace HHT_Rozen.Fragment
{
    public class PmsGyomuFragment : BaseFragment
    {
        Button sStartButton;
        Button sEndButton;

        [System.Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_pms_gyomu, container, false);

            SetActionBarTitle("PMS");

            sStartButton = view.FindViewById<Button>(Resource.Id.sagyoStartButton);
            sEndButton = view.FindViewById<Button>(Resource.Id.sagyoEndButton);

            sStartButton.Click += SStartButton_Click;
            sEndButton.Click += SEndButton_Click;

            string sflg = prefs.GetString("sflg", "0");
            if (sflg == "0")
            {
                sEndButton.Visibility = ViewStates.Gone;
            }
            else if (sflg == "1")
            {
                sStartButton.Visibility = ViewStates.Gone;
            }
            else
            {
                sEndButton.Visibility = ViewStates.Gone;
                ErrorDialog("他の作業中です。作業終了してください。", () => { });
            }

            return view;
        }

        [System.Obsolete]
        private void SEndButton_Click(object sender, System.EventArgs e)
        {
            Task.Run(async () => {

                var param = new Dictionary<string, string>()
                {
                    {"kbn", "4" },
                    {"serial", Build.Serial },
                    {"scd", prefs.GetString("sagyousya_cd", "") },
                };

                var res = await PostRestAPI("", "SetSagyoStatus", param);
                if (res.ContainsKey("errMesg"))
                {
                    ErrorDialog(res["errMesg"], () => { });
                }

                Activity.RunOnUiThread(() => {
                    sStartButton.Visibility = ViewStates.Visible;
                    sEndButton.Visibility = ViewStates.Gone;

                    editor.PutString("sflg", "0");
                    editor.Apply();
                });
            });
        }

        [System.Obsolete]
        private void SStartButton_Click(object sender, System.EventArgs e)
        {
            Task.Run(async () => {
                var param = new Dictionary<string, string>()
                {
                    {"kbn", "1" },
                    {"serial", Build.Serial },
                    {"scd", prefs.GetString("sagyousya_cd", "") },
                };

                var res = await PostRestAPI("","SetSagyoStatus", param);
                if (res.ContainsKey("errMesg"))
                {
                    ErrorDialog(res["errMesg"], () => { });
                }

                Activity.RunOnUiThread(() => {
                    sStartButton.Visibility = ViewStates.Gone;
                    sEndButton.Visibility = ViewStates.Visible;
                    editor.PutString("sflg", "1");
                    editor.Apply();
                });
            });
        }

        public override void OnResume()
        {
            base.OnResume();
        }

        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {

        }

        public override bool OnBackPressed()
        {
            return true;
        }
    }
}
