package crc6498113dccf5381c72;


public class MainActivity
	extends androidx.appcompat.app.AppCompatActivity
	implements
		mono.android.IGCUserPeer,
		com.densowave.bhtsdk.barcode.BarcodeManager.BarcodeManagerListener,
		com.densowave.bhtsdk.barcode.BarcodeScanner.BarcodeDataListener
{
/** @hide */
	public static final String __md_methods;
	static {
		__md_methods = 
			"n_onCreate:(Landroid/os/Bundle;)V:GetOnCreate_Landroid_os_Bundle_Handler\n" +
			"n_onRequestPermissionsResult:(I[Ljava/lang/String;[I)V:GetOnRequestPermissionsResult_IarrayLjava_lang_String_arrayIHandler\n" +
			"n_onResume:()V:GetOnResumeHandler\n" +
			"n_onKeyDown:(ILandroid/view/KeyEvent;)Z:GetOnKeyDown_ILandroid_view_KeyEvent_Handler\n" +
			"n_onPause:()V:GetOnPauseHandler\n" +
			"n_onDestroy:()V:GetOnDestroyHandler\n" +
			"n_onBarcodeManagerCreated:(Lcom/densowave/bhtsdk/barcode/BarcodeManager;)V:GetOnBarcodeManagerCreated_Lcom_densowave_bhtsdk_barcode_BarcodeManager_Handler:Com.Densowave.Bhtsdk.Barcode.BarcodeManager_/IBarcodeManagerListener_Invoker, bhtsdk_r1001300_v1.00.13\n" +
			"n_onBarcodeDataReceived:(Lcom/densowave/bhtsdk/barcode/BarcodeDataReceivedEvent;)V:GetOnBarcodeDataReceived_Lcom_densowave_bhtsdk_barcode_BarcodeDataReceivedEvent_Handler:Com.Densowave.Bhtsdk.Barcode.BarcodeScanner_/IBarcodeDataListener_Invoker, bhtsdk_r1001300_v1.00.13\n" +
			"";
		mono.android.Runtime.register ("HHT_Rozen.MainActivity, HHT-Rozen", MainActivity.class, __md_methods);
	}


	public MainActivity ()
	{
		super ();
		if (getClass () == MainActivity.class)
			mono.android.TypeManager.Activate ("HHT_Rozen.MainActivity, HHT-Rozen", "", this, new java.lang.Object[] {  });
	}


	public MainActivity (int p0)
	{
		super (p0);
		if (getClass () == MainActivity.class)
			mono.android.TypeManager.Activate ("HHT_Rozen.MainActivity, HHT-Rozen", "System.Int32, mscorlib", this, new java.lang.Object[] { p0 });
	}


	public void onCreate (android.os.Bundle p0)
	{
		n_onCreate (p0);
	}

	private native void n_onCreate (android.os.Bundle p0);


	public void onRequestPermissionsResult (int p0, java.lang.String[] p1, int[] p2)
	{
		n_onRequestPermissionsResult (p0, p1, p2);
	}

	private native void n_onRequestPermissionsResult (int p0, java.lang.String[] p1, int[] p2);


	public void onResume ()
	{
		n_onResume ();
	}

	private native void n_onResume ();


	public boolean onKeyDown (int p0, android.view.KeyEvent p1)
	{
		return n_onKeyDown (p0, p1);
	}

	private native boolean n_onKeyDown (int p0, android.view.KeyEvent p1);


	public void onPause ()
	{
		n_onPause ();
	}

	private native void n_onPause ();


	public void onDestroy ()
	{
		n_onDestroy ();
	}

	private native void n_onDestroy ();


	public void onBarcodeManagerCreated (com.densowave.bhtsdk.barcode.BarcodeManager p0)
	{
		n_onBarcodeManagerCreated (p0);
	}

	private native void n_onBarcodeManagerCreated (com.densowave.bhtsdk.barcode.BarcodeManager p0);


	public void onBarcodeDataReceived (com.densowave.bhtsdk.barcode.BarcodeDataReceivedEvent p0)
	{
		n_onBarcodeDataReceived (p0);
	}

	private native void n_onBarcodeDataReceived (com.densowave.bhtsdk.barcode.BarcodeDataReceivedEvent p0);

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
