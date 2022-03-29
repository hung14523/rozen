package mono.com.beardedhen.androidbootstrap;


public class BootstrapDropDown_OnDropDownItemClickListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.beardedhen.androidbootstrap.BootstrapDropDown.OnDropDownItemClickListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onItemClick:(Landroid/view/ViewGroup;Landroid/view/View;I)V:GetOnItemClick_Landroid_view_ViewGroup_Landroid_view_View_IHandler:Com.Beardedhen.Androidbootstrap.BootstrapDropDown/IOnDropDownItemClickListenerInvoker, Android-Bootstrap\n" +
			"";
		mono.android.Runtime.register ("Com.Beardedhen.Androidbootstrap.BootstrapDropDown+IOnDropDownItemClickListenerImplementor, Android-Bootstrap", BootstrapDropDown_OnDropDownItemClickListenerImplementor.class, __md_methods);
	}


	public BootstrapDropDown_OnDropDownItemClickListenerImplementor ()
	{
		super ();
		if (getClass () == BootstrapDropDown_OnDropDownItemClickListenerImplementor.class)
			mono.android.TypeManager.Activate ("Com.Beardedhen.Androidbootstrap.BootstrapDropDown+IOnDropDownItemClickListenerImplementor, Android-Bootstrap", "", this, new java.lang.Object[] {  });
	}


	public void onItemClick (android.view.ViewGroup p0, android.view.View p1, int p2)
	{
		n_onItemClick (p0, p1, p2);
	}

	private native void n_onItemClick (android.view.ViewGroup p0, android.view.View p1, int p2);

	private java.util.ArrayList refList;
	public void monodroidAddReference (java.lang.Object obj)
	{
		if (refList == null)
			refList = new java.util.ArrayList ();
		refList.add (obj);
	}

	public void monodroidClearReferences ()
	{
		if (refList != null)
			refList.clear ();
	}
}
