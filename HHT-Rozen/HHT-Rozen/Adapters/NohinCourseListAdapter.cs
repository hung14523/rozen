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
    class NohinCourseListAdapter : BaseAdapter<Nohin010>
    {
        private List<Nohin010> items;

        public NohinCourseListAdapter(List<Nohin010> items)
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
                view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_list_nohin_todoke, parent, false);

            view = LayoutInflater.From(parent.Context).Inflate(Resource.Layout.adapter_list_nohin_todoke, parent, false);
            //view.FindViewById<TextView>(Resource.Id.text_yttime).Text = item;
            view.FindViewById<TextView>(Resource.Id.text_tokuiname).Text = item.tokuisaki_rk;
            view.FindViewById<TextView>(Resource.Id.text_berth).Text = item.course;
            view.FindViewById<TextView>(Resource.Id.tv_progress_horizontal).Text = item.sumi + "/" + item.daisu;

            int progress = 0;

            if (item.sumi != 0 && item.daisu != 0)
            {
                progress = Convert.ToInt32((double.Parse(item.sumi.ToString()) / double.Parse(item.daisu.ToString())) * 100);
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

        public override Nohin010 this[int position] => items[position]; 
    }
}