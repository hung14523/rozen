package mono.com.beardedhen.androidbootstrap;


public class BootstrapButton_OnCheckedChangedListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.beardedhen.androidbootstrap.BootstrapButton.OnCheckedChangedListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_OnCheckedChanged:(Lcom/beardedhen/androidbootstrap/BootstrapButton;Z)V:GetOnCheckedChanged_Lcom_beardedhen_androidbootstrap_BootstrapButton_ZHandler:Com.Beardedhen.Androidbootstrap.BootstrapButton/IOnCheckedChangedListenerInvoker, Android-Bootstrap\n" +
			"";
		mono.android.Runtime.register ("Com.Beardedhen.Androidbootstrap.BootstrapButton+IOnCheckedChangedListenerImplementor, Android-Bootstrap", BootstrapButton_OnCheckedChangedListenerImplementor.class, __md_methods);
	}


	public BootstrapButton_OnCheckedChangedListenerImplementor ()
	{
		super ();
		if (getClass () == BootstrapButton_OnCheckedChangedListenerImplementor.class)
			mono.android.TypeManager.Activate ("Com.Beardedhen.Androidbootstrap.BootstrapButton+IOnCheckedChangedListenerImplementor, Android-Bootstrap", "", this, new java.lang.Object[] {  });
	}


	public void OnCheckedChanged (com.beardedhen.androidbootstrap.BootstrapButton p0, boolean p1)
	{
		n_OnCheckedChanged (p0, p1);
	}

	private native void n_OnCheckedChanged (com.beardedhen.androidbootstrap.BootstrapButton p0, boolean p1);

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
