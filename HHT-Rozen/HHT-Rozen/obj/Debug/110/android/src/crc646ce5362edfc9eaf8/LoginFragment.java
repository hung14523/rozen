package crc646ce5362edfc9eaf8;


public class LoginFragment
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
		mono.android.Runtime.register ("HHT_Rozen.Fragment.LoginFragment, HHT-Rozen", LoginFragment.class, __md_methods);
	}


	public LoginFragment ()
	{
		super ();
		if (getClass () == LoginFragment.class)
			mono.android.TypeManager.Activate ("HHT_Rozen.Fragment.LoginFragment, HHT-Rozen", "", this, new java.lang.Object[] {  });
	}


	public LoginFragment (int p0)
	{
		super (p0);
		if (getClass () == LoginFragment.class)
			mono.android.TypeManager.Activate ("HHT_Rozen.Fragment.LoginFragment, HHT-Rozen", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
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
