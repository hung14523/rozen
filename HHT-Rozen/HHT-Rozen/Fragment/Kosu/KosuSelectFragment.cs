using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.OS;
using Android.Views;
using Com.Densowave.Bhtsdk.Barcode;
using Com.Beardedhen.Androidbootstrap;
using HHT;

namespace HHT_Rozen.Fragment
{
    public class KosuSelectFragment : BaseFragment
    {
        BootstrapEditText mSyukaDateEditText;
        BootstrapButton mConfirmButton;

        private int kosuMenuflag;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_kosu_select, container, false);

            mSyukaDateEditText = view.FindViewById<BootstrapEditText>(Resource.Id.todoke_et_deliveryDate);
            mConfirmButton = view.FindViewById<BootstrapButton>(Resource.Id.btn_todoke_confirm);

            // PARAM
            kosuMenuflag = Arguments.GetInt("menu");

            // INIT
            InitComponent();

            return view;
        }

        [Obsolete]
        private void InitComponent()
        {
            // 検品作業初期化
            editor.PutString("ko_su", "0");
            editor.PutString("dai_su", "0");
            editor.Apply();

            // 配送日設定
            var today = DateTime.Now.ToString("yyyy/MM/dd");
            var hour = int.Parse(DateTime.Now.ToString("HH"));
            if (hour >= 7)
            {
                today = DateTime.Now.AddDays(1).ToString("yyyy/MM/dd");
            }

            mSyukaDateEditText.Text = today;
            mSyukaDateEditText.FocusChange += (sender, e) =>
            {
                if (e.HasFocus)
                {
                    mSyukaDateEditText.Text = mSyukaDateEditText.Text.Replace("/", "");
                    mSyukaDateEditText.SetSelection(mSyukaDateEditText.Text.Length);
                }
                else
                {
                    ChangeDateFormat();
                }
            };

            // 確定ボタン
            mConfirmButton.Click += delegate { Confirm(); };

            if (kosuMenuflag == (int)KOSUMENU.ARATA_CASE)
            {
                SetActionBarTitle("かご車紐づけ");
            }
            else if (kosuMenuflag == (int)KOSUMENU.ARATA_ORICON)
            {
                SetActionBarTitle("キャリー紐づけ");
            }

            mSyukaDateEditText.KeyPress += (sender, e) =>
            {
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    e.Handled = true;
                    Confirm();
                }
                else
                {
                    e.Handled = false;
                }
            };

            //mSyukaDateEditText.SetSelection(mSyukaDateEditText.Text.Length);
            //mSyukaDateEditText.RequestFocus();
        }

        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                this.Activity.RunOnUiThread(() =>
                {
                    string data = barcodeData.Data;
                });
            }
        }

        [Obsolete]
        private void Confirm()
        {
            Task.Run(() =>
            {
                return CheckValidation();
            }).ContinueWith(t =>
            {

                if (t.Result == false)
                {
                    return;
                }

                if (kosuMenuflag == (int)KOSUMENU.ARATA_CASE)
                {
                    editor.PutString("syuka_date", mSyukaDateEditText.Text.Replace("/", ""));
                    editor.Apply();
                    StartFragment(FragmentManager, typeof(KosuKagosyaFragment));
                }
                else if (kosuMenuflag == (int)KOSUMENU.ARATA_ORICON)
                {
                    editor.PutString("syuka_date", mSyukaDateEditText.Text.Replace("/", ""));
                    editor.Remove("mCarryBarcodes");
                    editor.Remove("mCarryCaseKbns");
                    editor.Apply();

                    StartFragment(FragmentManager, typeof(KosuCarryFragment));
                }
            });
        }

        [Obsolete]
        private bool CheckValidation()
        {

            if (mSyukaDateEditText.Text == "")
            {
                ShowDialog("エラー", "配送日を確認してください。", () => mSyukaDateEditText.RequestFocus());
                return false;
            }

            // Type Check
            string syukaDate = mSyukaDateEditText.Text.Replace("/", "");
            try
            {
                CommonUtils.GetDateYYYYMMDDwithSlash(mSyukaDateEditText.Text);
            }
            catch
            {
                ShowDialog("エラー", "正しい日付を入力してください。", () =>
                {
                    mSyukaDateEditText.Text = "";
                    mSyukaDateEditText.RequestFocus();
                });

                return false;
            }

            return true;
        }

        private bool ChangeDateFormat()
        {
            try
            {
                mSyukaDateEditText.Text = CommonUtils.GetDateYYYYMMDDwithSlash(mSyukaDateEditText.Text);
            }
            catch
            {
                ShowDialog("エラー", "正しい日付を入力してください。", () =>
                {
                    mSyukaDateEditText.Text = "";
                    mSyukaDateEditText.RequestFocus();
                });

                return false;
            }

            return true;
        }
    }
}