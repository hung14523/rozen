using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using HHT;
using HHT.Resources.Model;
using Newtonsoft.Json;

namespace HHT_Rozen.Fragment
{
    public class NohinSearchFragment : BaseFragment
    {
        private NohinCourseListAdapter todokesakiAdapter;
        
        ListView tenpoListView;

        [Obsolete]
        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_nohin_search, container, false);
            tenpoListView = view.FindViewById<ListView>(Resource.Id.listView1);

            var list = Arguments.GetString("list");
            var todokeList = JsonConvert.DeserializeObject<List<Nohin010>>(list);

            if (todokeList.Count == 0)
            {
                ShowDialog("エラー", "表示データがありません", () => { FragmentManager.PopBackStack(); });
            }
            else
            {
                tenpoListView.ItemClick += ListView_ItemClick;
                todokesakiAdapter = new NohinCourseListAdapter(todokeList);
                tenpoListView.Adapter = todokesakiAdapter;
                tenpoListView.RequestFocus();
                tenpoListView.SetSelection(0);
            }

            SetActionBarTitle("納品検品");
            
            tenpoListView = view.FindViewById<ListView>(Resource.Id.listView1);

            return view;
        }

        [Obsolete]
        void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var item = todokesakiAdapter[e.Position];

            string tokNm = item.tokuisaki_rk;
            string syukaDate = item.syuka_date;
            string course = item.course;
            string binNo = item.binNo;
            string jun = item.jun;
            string code = item.code;
            string seq = item.seq;

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
                            {"nohinDate", item.syuka_date.Replace("/", "") },
                            {"tokcd", item.tokuisaki_cd },
                            {"tdkcd", item.todokesaki_cd },
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
                        editor.PutString("tokuisaki_cd", item.tokuisaki_cd);
                        editor.PutString("todokesaki_cd", item.todokesaki_cd);
                        editor.PutString("tokuisaki_nm", tokNm);
                        editor.PutString("todokesaki_nm", tokNm);
                        editor.PutString("nohin_date", item.syuka_date.Replace("/", ""));
                        editor.PutString("nohin_time", DateTime.Now.ToString("HHmm"));
                        editor.Apply();

                        StartFragment(FragmentManager, typeof(NohinMenuFragment));
                    });
                });
            });
        }
    }
}
 