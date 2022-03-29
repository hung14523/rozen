using System; 
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Com.Beardedhen.Androidbootstrap;
using Com.Densowave.Bhtsdk.Barcode;
using HHT;
using HHT.Resources.Model;
using Java.Lang;
using Newtonsoft.Json;

namespace HHT_Rozen.Fragment
{
    public class NohinSelectFragment : BaseFragment
    {
        BootstrapEditText mTokEditText;
        BootstrapEditText mTdkEditText;
        BootstrapEditText mNohinDateEditText;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region #View Setting
            View view = inflater.Inflate(Resource.Layout.fragment_nohin_select, container, false);

            SetActionBarTitle("納品検品");
            mTokEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_nohinSelect_tokuisaki);
            mTdkEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_nohinSelect_todokesaki);
            mNohinDateEditText = view.FindViewById<BootstrapEditText>(Resource.Id.et_nohinDate);
            
            mTdkEditText.KeyPress += (sender, e) => {
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    Confirm();
                }
                else
                {
                    e.Handled = false;
                }
            };
            BootstrapButton confirm = view.FindViewById<BootstrapButton>(Resource.Id.btn_nohinSelect_confirm);
            confirm.FocusChange += delegate { if (confirm.IsFocused) CommonUtils.HideKeyboard(this.Activity); };
            confirm.Click += delegate { Confirm();};
            #endregion

            var today = DateTime.Now.ToString("yyyy/MM/dd");
            var hour = int.Parse(DateTime.Now.ToString("HH"));
            if (hour >= 7)
            {
                today = DateTime.Now.AddDays(1).ToString("yyyy/MM/dd");
            }

            mNohinDateEditText.Text = today;
            mNohinDateEditText.FocusChange += (sender, e) => {
                if (e.HasFocus)
                {
                    mNohinDateEditText.Text = mNohinDateEditText.Text.Replace("/", "");
                    mNohinDateEditText.SetSelection(mNohinDateEditText.Text.Length);
                }
                else
                {
                    ChangeDateFormat();
                }
            };

            mTokEditText.RequestFocus();
            
            return view;
        }

        [Obsolete]
        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                this.Activity.RunOnUiThread(() =>
                {
                    string data = barcodeData.Data;
                    string tok = Integer.ParseInt(data.Substring(4, 4)).ToString();
                    mTokEditText.Text = tok;
                    mTdkEditText.Text = "0";

                    Confirm();

                });
            }
        }

        [Obsolete]
        private bool ChangeDateFormat()
        {
            try
            {
                mNohinDateEditText.Text = CommonUtils.GetDateYYYYMMDDwithSlash(mNohinDateEditText.Text);
            }
            catch
            {
                ShowDialog("エラー", "正しい日付を入力してください。", () =>
                {
                    mNohinDateEditText.Text = "";
                    mNohinDateEditText.RequestFocus();
                });

                return false;
            }

            return true;
        }

        [Obsolete]
        private void Confirm()
        {
            // 得意先チェック
            if (mTokEditText.Text == "")
            {
                ShowDialog("エラー", "得意先コードを入力してください。", () => { mTokEditText.RequestFocus(); });
                return;
            }

            
            // 届先チェック
            
            ((MainActivity)this.Activity).ShowProgress("納品情報を確認しています。");

            Task.Run(async () =>
            {
                // 店着時間を更新
                var param = new Dictionary<string, string>
                {
                    {"nohinDate", mNohinDateEditText.Text.Replace("/", "") },
                    {"tokcd", mTokEditText.Text },
                    {"tdkcd", mTdkEditText.Text == ""? "0": mTdkEditText.Text }
                };

                return await WebService.PostRestAPI<List<Nohin010>>("nohin/nohin010", param);
            }).ContinueWith(t =>
            {
                List<Nohin010> result = t.Result;
                
                if (result== null || result.Count == 0)
                {
                    Activity.RunOnUiThread(() => { 
                        // "得意先コードが見つかりません。"
                        ShowDialog("エラー", "納品先情報が見つかりません。", () => {
                            ((MainActivity)this.Activity).DismissDialog();
                            mTdkEditText.RequestFocus(); 
                        });
                    });
                    return;
                }

                List<Nohin010> nohinMise = result.Where(t => t.daisu != t.sumi).ToList();

                if (nohinMise.Count == 0)
                {
                    Activity.RunOnUiThread(() => {
                        ShowDialog("エラー", "納品先情報が見つかりません。", () => {
                            ((MainActivity)this.Activity).DismissDialog();
                            mTdkEditText.RequestFocus();
                        });
                    });
                    return;
                }
                else if(nohinMise.Count == 1)
                {
                    string tokNm = nohinMise[0].tokuisaki_rk;
                    string syukaDate = nohinMise[0].syuka_date;
                    string course = nohinMise[0].course;
                    string binNo = nohinMise[0].binNo;
                    string jun = nohinMise[0].jun;
                    string code = nohinMise[0].code;
                    string seq = nohinMise[0].seq;

                    string dialogMsg = CommonUtils.GetDateYYYYMMDDwithSlash(syukaDate);
                    dialogMsg += "\n" + course; 
                    dialogMsg += "\n" + tokNm;
                    dialogMsg += "\n\n" + "よろしいですか？";

                    Activity.RunOnUiThread(() => {

                        ((MainActivity)this.Activity).DismissDialog();

                        ConfirmDialog(dialogMsg, (flag) =>
                        {
                            if (flag == false) return;

                            Task.Run(async () =>
                            {
                                // 店着時間を更新
                                var param = new Dictionary<string, string>
                                {
                                    {"nohinDate", mNohinDateEditText.Text.Replace("/", "") },
                                    {"tokcd", nohinMise[0].tokuisaki_cd },
                                    {"tdkcd", nohinMise[0].todokesaki_cd },
                                    { "course", course },
                                    { "bin", binNo },
                                    { "jun", jun },
                                    { "code", code },
                                    { "seq", seq },
                                    { "pSagyosyaCD", prefs.GetString("sagyousya_cd", "") },
                                    { "nohinFlag", "0" },
                                };

                                return await WebService.PostRestAPI("nohin/nohin080", param);

                            }).ContinueWith(t => {

                                if (t.Result == null || t.Result["status"] != "1")
                                {
                                    Activity.RunOnUiThread(() => {
                                        ShowDialog("エラー", "店着時間更新に失敗しました。", () => {
                                        });
                                    });

                                    return;
                                }

                                editor.PutString("course", course);
                                editor.PutString("binNo", binNo);
                                editor.PutString("jun", jun);
                                editor.PutString("code", code);
                                editor.PutString("seq", seq);

                                editor.PutString("tokuisaki_cd", nohinMise[0].tokuisaki_cd);
                                editor.PutString("todokesaki_cd", nohinMise[0].todokesaki_cd);
                                editor.PutString("tokuisaki_nm", tokNm);
                                editor.PutString("todokesaki_nm", tokNm);
                                editor.PutString("nohin_date", mNohinDateEditText.Text.Replace("/", ""));
                                editor.PutString("nohin_time", DateTime.Now.ToString("HHmm"));
                                editor.Apply();

                                StartFragment(FragmentManager, typeof(NohinMenuFragment));
                            });
                        });
                    });
                }
                else
                {
                    Activity.RunOnUiThread(() =>
                    {
                        ((MainActivity)this.Activity).DismissDialog();
                    });

                    var serializedStuff = JsonConvert.SerializeObject(nohinMise);

                    Bundle bundle = new Bundle();
                    bundle.PutString("list", serializedStuff);

                    StartFragment(FragmentManager, typeof(NohinSearchFragment), bundle);
                }
            });
        }
    }
}