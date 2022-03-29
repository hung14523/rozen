using Android.OS;
using Android.Views;
using Android.Widget;
using Com.Densowave.Bhtsdk.Barcode;
using System.Collections.Generic;
using Com.Beardedhen.Androidbootstrap;
using Android.Content.PM;
using System.Threading.Tasks;
using System;
using HHT;
using Android.Util;
using Newtonsoft.Json;

namespace HHT_Rozen.Fragment
{
    public class LoginFragment : BaseFragment
    {
        private readonly string TAG = "LoginFragment";
        private readonly string ERROR = "エラー";
        private readonly string ERR_NOT_FOUND_SOUKO = "センター情報が見つかりませんでした。\n再確認してください。";

        private BootstrapEditText mSoukoCodeEditText;
        private BootstrapEditText mTantoCodeEditText;
        private BootstrapButton mLoginButton;

        private TextView mSoukoNameTextView;
        private TextView mVersionTextView;
        private TextView mtantoTextView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            #region #ViewSetting
            View view = inflater.Inflate(Resource.Layout.fragment_login, container, false);

            SetActionBarTitle("ログイン");

            mSoukoCodeEditText = view.FindViewById<BootstrapEditText>(Resource.Id.soukoCode);
            mSoukoNameTextView = view.FindViewById<TextView>(Resource.Id.tv_login_soukoName);
            mTantoCodeEditText = view.FindViewById<BootstrapEditText>(Resource.Id.tantoCode);
            mLoginButton = view.FindViewById<BootstrapButton>(Resource.Id.loginButton);
            mVersionTextView = view.FindViewById<TextView>(Resource.Id.versionName);
            #endregion

            // バージョン情報設定
            PackageInfo packageInfo = Context.PackageManager.GetPackageInfo(Context.ApplicationContext.PackageName, 0);
            mVersionTextView.Text = "Version " + packageInfo.VersionName;

            // ログインユーザ名
            mtantoTextView = ((MainActivity)this.Activity).SupportActionBar.CustomView.FindViewById<TextView>(Resource.Id.toolbar_title2);
            mtantoTextView.Text = "";

            // 担当者コード
            mTantoCodeEditText.KeyPress += (sender, e) =>
            {
                if (e.Event.Action == KeyEventActions.Down && e.KeyCode == Keycode.Enter)
                {
                    e.Handled = true;
                    CommonUtils.HideKeyboard(Activity);
                    LoginBtn_Clicked();
                }
                else
                {
                    e.Handled = false;
                }
            };

            #region keyEvent
            // 倉庫コード
            mSoukoCodeEditText.FocusChange += delegate
            {
                if (!mSoukoCodeEditText.IsFocused)
                    SetSoukoName(mSoukoCodeEditText.Text);
            };

            // ログインボタン
            mLoginButton.Click += delegate { LoginBtn_Clicked(); };
            #endregion
            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            mSoukoCodeEditText.Text = prefs.GetString("souko_cd", "");
            mSoukoNameTextView.Text = prefs.GetString("soukoName", "");
            mtantoTextView.Text = "";

            if (mSoukoCodeEditText.Text != "")
                mTantoCodeEditText.RequestFocus();
        }

        /*WebLink使用*/
        /*
        [Obsolete]
        public void LoginBtn_Clicked()
        {
            // Validation Check
            if (LoginCheck() == false)
                return;

            // 担当者情報取得かつ無線管理テーブルへ情報を登録する
            var param = new Dictionary<string, string>
            {
                { "soukoCd", mSoukoCodeEditText.Text },
                { "tanto",   mTantoCodeEditText.Text },
                { "serial",  Build.Serial }
            };
            ShowProgress("");
            Task.Run(async () =>
            {
                try
                {
                    var response = await WebApi.Post("LoginUser", param);
                    if (response.errMesg != "")
                    {
                        ShowDialog("エラー", response.errMesg, () =>
                        {
                            mTantoCodeEditText.Text = "";
                            mTantoCodeEditText.RequestFocus();
                        });
                        return;
                    }
                    else
                    {
                        var result = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.data.ToString());
                        var syainNm = result["tantohsya_nm"];
                        var menu_kbn = result["menu_kbn"];
                        if (syainNm == "" || !int.TryParse(menu_kbn, out int temp))
                        {
                            ShowDialog("エラー", "認証できませんでした。\n入力内容をご確認下さい。", () =>
                            {
                                mTantoCodeEditText.Text = "";
                                mTantoCodeEditText.RequestFocus();
                            });
                        }
                        else
                        {
                            //WebApi.SetNameSpace(result["CacheNamespace"]);
                            Bundle bundle = new Bundle();
                            bundle.PutInt("menu_kbn", int.Parse(menu_kbn));
                            editor.PutString("terminal_id", Build.Serial);
                            editor.PutString("hht_no", Build.Serial);
                            editor.PutString("souko_cd", mSoukoCodeEditText.Text);
                            editor.PutString("soukoName", mSoukoNameTextView.Text);
                            editor.PutString("driver_cd", mTantoCodeEditText.Text);
                            editor.PutString("sagyousya_cd", mTantoCodeEditText.Text);
                            editor.Apply();

                            Activity.RunOnUiThread(() => {
                                mtantoTextView.Text = syainNm;
                            });

                            PlayBeepOk();
                            StartFragment(FragmentManager, typeof(MainMenuFragment), bundle);
                        }
                    }
                }
                catch
                {
                    ShowDialog("エラー", "認証できませんでした。\n入力内容をご確認下さい。", () =>
                    {
                        mTantoCodeEditText.Text = "";
                        mTantoCodeEditText.RequestFocus();

                        Activity.RunOnUiThread(() => DismissProgress());
                    });
                }
                finally
                {
                    Activity.RunOnUiThread(() => { DismissProgress(); });
                }
            });
        }*/
        /* WebLink使用しない*/
        
        [Obsolete]
        public void LoginBtn_Clicked()
        {
            // Validation Check
            if (LoginCheck() == false)
                return;

            // 担当者情報取得かつ無線管理テーブルへ情報を登録する
            var param = new Dictionary<string, string>
            {
                { "soukoCd", mSoukoCodeEditText.Text },
                { "tanto",   mTantoCodeEditText.Text },
                { "serial",  Build.Serial }
            };

            Task.Run(async () =>
            {
                try
                {
                    //string restUrl = prefs.GetString("REST_URL", "bms.jobs-logistics.jp");
                    string restUrl = prefs.GetString("REST_URL", "test.jobs-logistics.jp");
                    string nameSapce = prefs.GetString("CacheNamespace", "");


                    //restUrl = "test.jobs-logistics.jp";

                    if (mSoukoCodeEditText.Text == "9999")
                    {
                        nameSapce = "ara";
                        param["soukoCd"] = "11";
                    }

                    if (nameSapce == "")
                    {
                        ShowDialog("エラー", "認証できませんでした。\n入力内容をご確認下さい。", () =>
                        {
                            mTantoCodeEditText.Text = "";
                            mTantoCodeEditText.RequestFocus();
                        });
                        return;
                    }
                    else
                    {
                        WebService.SetRESTURLForArata(restUrl, nameSapce);
                    }

                    var result = await PostRestAPI("ログインしています。", "login", param);


                    if (result.ContainsKey("errMesg"))
                    {
                        ShowDialog("エラー", result["errMesg"], () =>
                        {
                            mTantoCodeEditText.Text = "";
                            mTantoCodeEditText.RequestFocus();
                        });
                        return;
                    }

                    string menu_kbn = result["menu_kbn"];
                    if (menu_kbn == "" || !int.TryParse(menu_kbn, out int temp))
                    {
                        ShowDialog("エラー", "認証できませんでした。\n入力内容をご確認下さい。", () =>
                        {
                            mTantoCodeEditText.Text = "";
                            mTantoCodeEditText.RequestFocus();
                        });
                    }
                    else
                    {
                        Bundle bundle = new Bundle();
                        bundle.PutInt("menu_kbn", int.Parse(menu_kbn));

                        editor.PutString("terminal_id", Build.Serial);
                        editor.PutString("hht_no", Build.Serial);
                        editor.PutString("souko_cd", mSoukoCodeEditText.Text);
                        editor.PutString("soukoName", mSoukoNameTextView.Text);
                        editor.PutString("driver_cd", mTantoCodeEditText.Text);
                        editor.PutString("sagyousya_cd", mTantoCodeEditText.Text);
                        editor.Apply();

                        Activity.RunOnUiThread(() =>
                        {
                            if (mSoukoCodeEditText.Text == "9999")
                            {
                                mtantoTextView.Text = "テスト";
                            }
                            else
                            {
                                mtantoTextView.Text = result["tantohsya_nm"];
                            }
                        });

                        PlayBeepOk();
                        StartFragment(FragmentManager, typeof(MainMenuFragment), bundle);
                    }
                }
                catch
                {
                    ShowDialog("エラー", "認証できませんでした。\n入力内容をご確認下さい。", () =>
                    {
                        mTantoCodeEditText.Text = "";
                        mTantoCodeEditText.RequestFocus();

                        Activity.RunOnUiThread(() => DismissProgress());
                    });
                }
            });
        }

        /* Weblink使用しない*/
        
        [Obsolete]
        public void SetSoukoName(string soukoCd)
        {
            if (string.IsNullOrEmpty(soukoCd)) return;

            Task.Run(async () =>
            {
                try
                {
                    //string restUrl = prefs.GetString("REST_URL", "bms.jobs-logistics.jp");
                    string restUrl = "test.jobs-logistics.jp"; // TODO Delete

                    // NameSpace取得専用
                    //WebService.SetRESTURLForArata(restUrl, "tmsmast");
                    //WebService.SetRESTURLForArata("test.jobs-logistics.jp", "butsu");
                    WebService.SetRESTURLForArata(restUrl, "butsu");

                    var result = await GetRestAPI("倉庫を確認しています。", "souko/" + soukoCd);
                    string soukoName = result["SoukoName"];
                    string cacheNamespace = result["CacheNamespace"];
                    //cacheNamespace = "ara";
                    //string soukoName = "butsu";
                    //string cacheNamespace = "butsu";

                    if (mSoukoCodeEditText.Text == "9999")
                    {
                        // TEST
                        soukoName = "ARA テスト";
                        cacheNamespace = "ara";
                    }

                    if (string.IsNullOrEmpty(soukoName))
                    {
                        ShowDialog(ERROR, ERR_NOT_FOUND_SOUKO, () =>
                        {
                            Activity.RunOnUiThread(() =>
                            {
                                mSoukoCodeEditText.Text = "";
                                mSoukoNameTextView.Text = "";
                                mSoukoCodeEditText.RequestFocus();
                            });
                        });
                    }
                    else
                    {
                        Activity.RunOnUiThread(() =>
                        {
                            mSoukoNameTextView.Text = soukoName;
                            //mNameSpace = cacheNamespace;
                            // MASTネームスペースに倉庫コードを確認する処理がまだできてないため、コメントアウトします。
                            //string restUrl = prefs.GetString("REST_URL", "bms.jobs-logistics.jp");
                            string restUrl = "test.jobs-logistics.jp";

                            editor.PutString("CacheNamespace", cacheNamespace);
                            editor.Apply();

                            WebService.SetRESTURLForArata(restUrl, cacheNamespace);
                        });
                    }
                }
                catch (Exception e)
                {
                    // ERR_NOT_FOUND_SOUKO
                    ShowDialog(ERROR, e.Message, () =>
                    {

                        Activity.RunOnUiThread(() =>
                        {
                            DismissProgress();

                            mSoukoCodeEditText.Text = "";
                            mSoukoNameTextView.Text = "";
                            mSoukoCodeEditText.RequestFocus();
                        });
                    });
                }

            });
        }

        /*WebLink使用*/
        /*
        [Obsolete]
        public void SetSoukoName(string soukoCd)
        {
            if (string.IsNullOrEmpty(soukoCd))
            {
                mSoukoCodeEditText.Text = "";
                return;
            };
            ShowProgress("");
            Task.Run(async () =>
            {
                try
                {
                    var param = new Dictionary<string, string>() { { "soukoCd", soukoCd } };
                    var response = await WebApi.Post("GetSoukoName", param);
                    var soukoInfo = JsonConvert.DeserializeObject<Dictionary<string, string>>(response.data.ToString());
                    string soukoName = soukoInfo["SoukoName"];
                    string cacheNamespace = soukoInfo["CacheNamespace"];

                    if (string.IsNullOrEmpty(soukoName))
                    {
                        ErrorDialog("センター情報が見つかりませんでした。\n再確認してください。", () =>
                        {
                            Activity.RunOnUiThread(() =>
                            {
                                mSoukoCodeEditText.Text = "";
                                mSoukoNameTextView.Text = "";
                                mSoukoCodeEditText.RequestFocus();
                            });
                        });
                    }
                    else
                    {
                        WebApi.SetNameSpace(cacheNamespace);

                        Activity.RunOnUiThread(() =>
                        {
                            mSoukoNameTextView.Text = soukoName;
                        });
                    }
                }
                catch (Exception)
                {
                    ErrorDialog("センター情報が見つかりませんでした。\n再確認してください。", () =>
                    {
                        Activity.RunOnUiThread(() =>
                        {
                            mSoukoCodeEditText.Text = "";
                            mSoukoNameTextView.Text = "";
                            mSoukoCodeEditText.RequestFocus();
                        });
                    });
                }
                finally
                {
                    Activity.RunOnUiThread(() => { DismissProgress(); });
                }
            });
        }*/
        [Obsolete]
        private bool LoginCheck()
        {
            string alertTitle = Resources.GetString(Resource.String.error);

            if (mSoukoCodeEditText.Text == "")
            {
                string alertBody = Resources.GetString(Resource.String.errorMsg002);
                ShowDialog(alertTitle, alertBody, () =>
                {
                    mSoukoNameTextView.Text = "";
                    mSoukoCodeEditText.RequestFocus();
                });

                Log.Debug(TAG, "倉庫コードがありません。");
                return false;
            }

            if (mTantoCodeEditText.Text == "")
            {
                ShowDialog(alertTitle, "担当者コードを\n入力して下さい。", () => { });

                Log.Warn(TAG, "担当者コードがありません。");
                return false;
            }

            return true;
        }

        // ログイン担当者コードスキャンのみできる
        public override void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            IList<BarcodeDataReceivedEvent_.BarcodeData_> listBarcodeData = dataReceivedEvent.BarcodeData;

            foreach (BarcodeDataReceivedEvent_.BarcodeData_ barcodeData in listBarcodeData)
            {
                // 担当者コードのみバーコードスキャン有効
                if (mTantoCodeEditText.IsFocused)
                {
                    this.Activity.RunOnUiThread(() => mTantoCodeEditText.Text = barcodeData.Data);
                }
            }
        }
    }
}