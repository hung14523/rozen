using System;
using System.Collections.Generic;
using Android.Views;
using Android.Widget;
using Com.Beardedhen.Androidbootstrap;
using Com.Beardedhen.Androidbootstrap.Api.Defaults;
using HHT.Resources.Model;
using HHT_Rozen;

namespace HHT
{
    class TsumikomiTenpoListAdapter : BaseAdapter<Haiso>
    {
        private List<Haiso> items;

        public TsumikomiTenpoListAdapter(List<Haiso> items)
        {
            this.items = items;
        }

        public override Java.Lang.Object GetItem(int position)
        {
            return items[position].todokesaki_cd;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            var view = convertView;
            var item = items[position];

            if (view == null)
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_list_todoke, parent, false);

            view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_list_todoke, parent, false);
            view.FindViewById<TextView>(Resource.Id.text_yttime).Text = item.nohin_yti_time;
            view.FindViewById<TextView>(Resource.Id.text_tokuiname).Text = item.tokuisaki_rk;
            view.FindViewById<TextView>(Resource.Id.text_berth).Text = item.kansen_kbn;
            view.FindViewById<TextView>(Resource.Id.tv_progress_horizontal).Text = item.tumi_sumi + "/" + item.tumi_kei;

            int progress = 0;

            if (item.tumi_kei != 0 && item.tumi_sumi != 0)
            {
                progress = Convert.ToInt32((double.Parse(item.tumi_sumi.ToString()) / double.Parse(item.tumi_kei.ToString())) * 100);
                if (progress > 100) progress = 100;
            }
            
            var pgBar = view.FindViewById<BootstrapProgressBar>(Resource.Id.txt_adp_todoke_progressbar);
            pgBar.Progress = progress;
            pgBar.SetBootstrapSize(DefaultBootstrapSize.Xl);
            
            return view;
        }

        public override int Count
        {
            get { return items.Count; }
        }

        public override Haiso this[int position] => items[position]; 
    }
}