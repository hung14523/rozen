using System.Collections.Generic;
using System.Threading.Tasks;
using Android.App;
using Android.OS;
using Android.Views;
using Android.Widget;
using HHT;
using HHT.Resources.Model;

namespace HHT_Rozen.Fragment
{
    public class TsumikomiSearchFragment : BaseFragment
    {
        private TsumikomiTenpoListAdapter todokesakiAdapter;
        
        ListView tenpoListView;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            View view = inflater.Inflate(Resource.Layout.fragment_tsumikomi_search, container, false);
            SetActionBarTitle("積込検品");
            
            tenpoListView = view.FindViewById<ListView>(Resource.Id.listView1);

            return view;
        }

        public override void OnResume()
        {
            base.OnResume();

            GetTenpoList();
        }

        [System.Obsolete]
        private void GetTenpoList()
        {
            Task.Run(async () =>
            {
                Activity.RunOnUiThread(() => ShowProgress("読み込み中..."));

                List<Haiso> todokeList = await GetTumikomiTenpoList();

                Activity.RunOnUiThread(() =>
                {
                    todokeList.RemoveAll(x => x.tumi_kei == 0 || x.tumi_kei == x.tumi_sumi);

                    if (todokeList.Count == 0)
                    {
                        ShowDialog("エラー", "表示データがありません", () => { FragmentManager.PopBackStack(); });
                    }
                    else
                    {
                        if (IsTest)
                        {
                            todokeList.ForEach(x => x.tokuisaki_rk = x.tokuisaki_cd + "-" + x.tokuisaki_rk);
                        }

                        tenpoListView.ItemClick += ListView_ItemClick;    
                        todokesakiAdapter = new TsumikomiTenpoListAdapter(todokeList);
                        tenpoListView.Adapter = todokesakiAdapter;
                        tenpoListView.RequestFocus();
                        tenpoListView.SetSelection(0);
                    }

                    DismissProgress();
                });
            });
        }

        private async Task<List<Haiso>> GetTumikomiTenpoList()
        {
            var param = new Dictionary<string, string>()
            {
                {"syuka_date", prefs.GetString("syuka_date", "")},
                {"bin_no", prefs.GetString("bin_no", "") },
                {"course", prefs.GetString("course", "") },
            };

            return await WebService.PostRestAPI<List<Haiso>>("tumi/tumi020", param);
        }

        void ListView_ItemClick(object sender, AdapterView.ItemClickEventArgs e)
        {
            var item = todokesakiAdapter[e.Position];

            editor.PutString("todokesaki_cd", item.todokesaki_cd);
            editor.PutString("tokuisaki_cd", item.tokuisaki_cd);
            editor.PutString("tokuisaki_nm", item.tokuisaki_rk);
            editor.PutString("kagosyaCount", item.kagosyaCount);
            editor.PutString("carryCount", item.carryCount);
            editor.Apply();

            Bundle bundle = new Bundle();
            bundle.PutInt("haisoCount", tenpoListView.Count);

            StartFragment(FragmentManager, typeof(TsumikomiWorkFragment), bundle);
        }

        public override bool OnBackPressed()
        {
            DismissProgress();
            return true;
        }

    }
}
 