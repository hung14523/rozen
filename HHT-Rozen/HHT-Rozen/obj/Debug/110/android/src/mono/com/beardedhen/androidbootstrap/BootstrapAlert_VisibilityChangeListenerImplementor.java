package mono.com.beardedhen.androidbootstrap;


public class BootstrapAlert_VisibilityChangeListenerImplementor
	extends java.lang.Object
	implements
		mono.android.IGCUserPeer,
		com.beardedhen.androidbootstrap.BootstrapAlert.VisibilityChangeListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onAlertAppearCompletion:(Lcom/beardedhen/androidbootstrap/BootstrapAlert;)V:GetOnAlertAppearCompletion_Lcom_beardedhen_androidbootstrap_BootstrapAlert_Handler:Com.Beardedhen.Androidbootstrap.BootstrapAlert/IVisibilityChangeListenerInvoker, Android-Bootstrap\n" +
			"n_onAlertAppearStarted:(Lcom/beardedhen/androidbootstrap/BootstrapAlert;)V:GetOnAlertAppearStarted_Lcom_beardedhen_androidbootstrap_BootstrapAlert_Handler:Com.Beardedhen.Androidbootstrap.BootstrapAlert/IVisibilityChangeListenerInvoker, Android-Bootstrap\n" +
			"n_onAlertDismissCompletion:(Lcom/beardedhen/androidbootstrap/BootstrapAlert;)V:GetOnAlertDismissCompletion_Lcom_beardedhen_androidbootstrap_BootstrapAlert_Handler:Com.Beardedhen.Androidbootstrap.BootstrapAlert/IVisibilityChangeListenerInvoker, Android-Bootstrap\n" +
			"n_onAlertDismissStarted:(Lcom/beardedhen/androidbootstrap/BootstrapAlert;)V:GetOnAlertDismissStarted_Lcom_beardedhen_androidbootstrap_BootstrapAlert_Handler:Com.Beardedhen.Androidbootstrap.BootstrapAlert/IVisibilityChangeListenerInvoker, Android-Bootstrap\n" +
			"";
		mono.android.Runtime.register ("Com.Beardedhen.Androidbootstrap.BootstrapAlert+IVisibilityChangeListenerImplementor, Android-Bootstrap", BootstrapAlert_VisibilityChangeListenerImplementor.class, __md_methods);
	}


	public BootstrapAlert_VisibilityChangeListenerImplementor ()
	{
		super ();
		if (getClass () == BootstrapAlert_VisibilityChangeListenerImplementor.class)
			mono.android.TypeManager.Activate ("Com.Beardedhen.Androidbootstrap.BootstrapAlert+IVisibilityChangeListenerImplementor, Android-Bootstrap", "", this, new java.lang.Object[] {  });
	}


	public void onAlertAppearCompletion (com.beardedhen.androidbootstrap.BootstrapAlert p0)
	{
		n_onAlertAppearCompletion (p0);
	}

	private native void n_onAlertAppearCompletion (com.beardedhen.androidbootstrap.BootstrapAlert p0);


	public void onAlertAppearStarted (com.beardedhen.androidbootstrap.BootstrapAlert p0)
	{
		n_onAlertAppearStarted (p0);
	}

	private native void n_onAlertAppearStarted (com.beardedhen.androidbootstrap.BootstrapAlert p0);


	public void onAlertDismissCompletion (com.beardedhen.androidbootstrap.BootstrapAlert p0)
	{
		n_onAlertDismissCompletion (p0);
	}

	private native void n_onAlertDismissCompletion (com.beardedhen.androidbootstrap.BootstrapAlert p0);


	public void onAlertDismissStarted (com.beardedhen.androidbootstrap.BootstrapAlert p0)
	{
		n_onAlertDismissStarted (p0);
	}

	private native void n_onAlertDismissStarted (com.beardedhen.androidbootstrap.BootstrapAlert p0);

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
