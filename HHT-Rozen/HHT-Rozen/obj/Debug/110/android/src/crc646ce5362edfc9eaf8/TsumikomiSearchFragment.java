package crc646ce5362edfc9eaf8;


public class TsumikomiSearchFragment
	extends crc646ce5362edfc9eaf8.BaseFragment
	implements
		mono.android.IGCUserPeer
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreateView:(Landroid/view/LayoutInflater;Landroid/view/ViewGroup;Landroid/os/Bundle;)Landroid/view/View;:GetOnCreateView_Landroid_view_LayoutInflater_Landroid_view_ViewGroup_Landroid_os_Bundle_Handler\n" +
			"n_onResume:()V:GetOnResumeHandler\n" +
			"";
		mono.android.Runtime.register ("HHT_Rozen.Fragment.TsumikomiSearchFragment, HHT-Rozen", TsumikomiSearchFragment.class, __md_methods);
	}


	public TsumikomiSearchFragment ()
	{
		super ();
		if (getClass () == TsumikomiSearchFragment.class)
			mono.android.TypeManager.Activate ("HHT_Rozen.Fragment.TsumikomiSearchFragment, HHT-Rozen", "", this, new java.lang.Object[] {  });
	}


	public TsumikomiSearchFragment (int p0)
	{
		super (p0);
		if (getClass () == TsumikomiSearchFragment.class)
			mono.android.TypeManager.Activate ("HHT_Rozen.Fragment.TsumikomiSearchFragment, HHT-Rozen", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
	}


	public android.view.View onCreateView (android.view.LayoutInflater p0, android.view.ViewGroup p1, android.os.Bundle p2)
	{
		return n_onCreateView (p0, p1, p2);
	}

	private native android.view.View n_onCreateView (android.view.LayoutInflater p0, android.view.ViewGroup p1, android.os.Bundle p2);


	public void onResume ()
	{
		n_onResume ();
	}

	private native void n_onResume ();

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
