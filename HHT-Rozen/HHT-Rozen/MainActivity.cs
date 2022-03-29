using System;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using AndroidX.AppCompat.Widget;
using AndroidX.AppCompat.App;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Snackbar;
using HHT_Rozen.Fragment;
using Com.Densowave.Bhtsdk.Barcode;
using System.Collections.ObjectModel;
using Java.Nio.Charset;
using Android.Util;
using Android.Widget;
using System.Collections.Generic;
using Android.Media;
using Android.Content;
using AndroidX.Preference;
using Com.Densowave.Bhtsdk.Devicesettings;

namespace HHT_Rozen
{
    [Activity(Theme = "@style/AppTheme", ScreenOrientation = Android.Content.PM.ScreenOrientation.Portrait)]
    public class MainActivity : AppCompatActivity, BarcodeManager_.IBarcodeManagerListener_, BarcodeScanner_.IBarcodeDataListener_
    {
        private const string TAG = "MainActivity";

        protected PowerManager.WakeLock mWakeLock;

        private SoundPool soundPool;

        private BarcodeManager_ mBarcodeManager = null;
        private BarcodeScanner_ mBarcodeScanner = null;
        private BarcodeScannerSettings_ mSettings = null;
        private BarcodeScannerInfo_.BarcodeScannerType_ mScannerType = null;
        DeviceSettingsLibrary_ mDeviceSettingslib = null;
        private bool mResumed = false;

        readonly ReadOnlyCollection<ScanSettings_.TriggerMode_> TRIGGER_MODE = Array.AsReadOnly(new ScanSettings_.TriggerMode_[] {
                ScanSettings_.TriggerMode_.AutoOff,
                ScanSettings_.TriggerMode_.Momentary,
                ScanSettings_.TriggerMode_.Alternate,
                ScanSettings_.TriggerMode_.Continuous,
                ScanSettings_.TriggerMode_.TriggerRelease
        });

        readonly ReadOnlyCollection<BarcodeScannerInfo_.BarcodeScannerType_> SCANNER_TYPE = Array.AsReadOnly(new BarcodeScannerInfo_.BarcodeScannerType_[] {
                BarcodeScannerInfo_.BarcodeScannerType_.Type1d,
                BarcodeScannerInfo_.BarcodeScannerType_.Type2d,
                BarcodeScannerInfo_.BarcodeScannerType_.Type2dLong
        });

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            SetContentView(Resource.Layout.activity_main);

            try
            {
                MainActivity mainActivity = this;
                BarcodeManager_.Create(this, this);

                AudioAttributes audioAttrib = new AudioAttributes.Builder()
                .SetUsage(AudioUsageKind.Game)
                .SetContentType(AudioContentType.Sonification)
                .Build();

                soundPool = new SoundPool.Builder().SetAudioAttributes(audioAttrib).SetMaxStreams(6).Build();

                this.SupportActionBar.SetDisplayShowCustomEnabled(true);
                this.SupportActionBar.SetCustomView(Resource.Layout.custom_toolbar);
            }
            catch
            {
                mScannerType = null;
                mSettings = null;
            }

            ISharedPreferences prefs = PreferenceManager.GetDefaultSharedPreferences(Application);
            ISharedPreferencesEditor editor = prefs.Edit();

            SupportFragmentManager.BeginTransaction()
                                  .Add(Resource.Id.fragmentContainer, new LoginFragment(), "fragment_login")
                                  .Commit();
        }

        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }

        protected override void OnResume()
        {
            base.OnResume();

            try
            {
                if (mBarcodeScanner != null)
                {
                    mBarcodeScanner.AddDataListener(this);

                    if (mSettings == null)
                    {
                        mSettings = mBarcodeScanner.Settings;
                        SetScanner();
                    }
                    mBarcodeScanner.Settings = mSettings;
                    mBarcodeScanner.Claim();
                }

            }
            catch (BarcodeException_ e)
            {
                Log.Error(TAG, "ErrorCode is " + e.ErrorCode, e);
            }
            catch
            {
                //Toast.MakeText(this, Resource.String.error_message_symbol_settings, ToastLength.Long).Show();
            }

            mResumed = true;
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            AndroidX.Fragment.App.Fragment localFragment = SupportFragmentManager.FindFragmentById(Resource.Id.fragmentContainer);

            if ((localFragment is BaseFragment baseFragment) && (((BaseFragment)localFragment).OnKeyDown(keyCode, e)))
            {
                return base.OnKeyDown(keyCode, e);
            }
            return base.OnKeyDown(keyCode, e);
        }

        protected override void OnPause()
        {
            base.OnPause();

            // Release scanner resources
            if (mBarcodeScanner != null)
            {
                try
                {
                    // Disable Scanner
                    mBarcodeScanner.Close();
                    // Remove Listener Event
                    mBarcodeScanner.RemoveDataListener(this);
                }
                catch (BarcodeException_ e)
                {
                    Log.Error(TAG, "Error Code = " + e.ErrorCode, e);
                }
            }
        }

        public void ShowProgress(string message)
        {
            LinearLayout progressBar = FindViewById<LinearLayout>(Resource.Id.llProgressBar);

            progressBar.Visibility = ViewStates.Visible;

            Window.SetFlags(WindowManagerFlags.NotTouchable, WindowManagerFlags.NotTouchable);
        }

        public void DismissDialog()
        {
            LinearLayout progressBar = FindViewById<LinearLayout>(Resource.Id.llProgressBar);
            progressBar.Visibility = ViewStates.Gone;

            Window.ClearFlags(WindowManagerFlags.NotTouchable);
        }

        public void DisableScanning()
        {
            if (mBarcodeScanner != null)
            {
                try
                {
                    // Remove Scanner instance
                    mBarcodeScanner.Destroy();
                }
                catch (BarcodeException_ e)
                {
                    Log.Error(TAG, "Error Code = " + e.ErrorCode, e);
                }
            }

            if (mBarcodeManager != null)
            {
                // Remove Scanner Manager
                mBarcodeManager.Destroy();
                mBarcodeManager = null;
            }

        }

        public void EnableScanning()
        {
            try
            {
                BarcodeManager_.Create(this, this);
            }
            catch (BarcodeException_ e)
            {
                Log.Error(TAG, "ErrorCode is " + e.ErrorCode, e);
            }
            catch
            {
                Toast.MakeText(this, Resource.String.error_message_symbol_settings, ToastLength.Long).Show();
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (mBarcodeScanner != null)
            {
                try
                {
                    // Remove Scanner instance
                    mBarcodeScanner.Destroy();
                }
                catch (BarcodeException_ e)
                {
                    Log.Error(TAG, "Error Code = " + e.ErrorCode, e);
                }
            }

            if (mBarcodeManager != null)
            {
                // Remove Scanner Manager
                mBarcodeManager.Destroy();
                mBarcodeManager = null;
            }
        }
        public void OnBarcodeManagerCreated(BarcodeManager_ barcodeManager)
        {
            // When barcode scanner manager created.
            mBarcodeManager = barcodeManager;
            try
            {
                IList<BarcodeScanner_> listScanner = barcodeManager.BarcodeScanners;
                if (listScanner.Count > 0)
                {
                    // Get BarcodeScanner instance
                    mBarcodeScanner = listScanner[0];   // 0 is default scanner

                    if (mResumed)
                    {
                        // Register Data Received event
                        mBarcodeScanner.AddDataListener(this);

                        // Setting for Scanner
                        if (mScannerType == null)
                        {
                            mScannerType = mBarcodeScanner.Info.Type;
                        }
                        if (mSettings == null)
                        {
                            mSettings = mBarcodeScanner.Settings;
                            this.SetScanner();
                        }

                        mBarcodeScanner.Settings = mSettings;

                        // Enable Scanner
                        mBarcodeScanner.Claim();

                    }
                }
            }
            catch (BarcodeException_ e)
            {
                Log.Error(TAG, "ErrorCode is " + e.ErrorCode, e);
            }
        }

        public void OnBarcodeDataReceived(BarcodeDataReceivedEvent_ dataReceivedEvent)
        {
            AndroidX.Fragment.App.Fragment localFragment = SupportFragmentManager.FindFragmentById(Resource.Id.fragmentContainer);
            ((BaseFragment)localFragment).OnBarcodeDataReceived(dataReceivedEvent);
        }
        public void PlayBeep(int beepId)
        {
            int soundId = soundPool.Load(this, beepId, 1);
            soundPool.Play(soundId, 1, 1, 0, 0, 1);
        }

        public void OnTouchDeviceSettingsCreated(DeviceSettingsLibrary_ devicesettinglib)
        {
            mDeviceSettingslib = devicesettinglib;
        }

        protected class OnLoadCompleteListener : Java.Lang.Object, SoundPool.IOnLoadCompleteListener
        {
            public void OnLoadComplete(SoundPool soundPool, int sampleId, int status)
            {
                int streamId = soundPool.Play(sampleId, 100, 100, 1, 0, 1.0f);
                soundPool.SetVolume(streamId, 100, 100);
            }
        }
        public void ResizeBarcodeLength(int length)
        {

            try
            {
                // Release scanner resources
                if (mBarcodeScanner != null)
                {
                    try
                    {
                        // Disable Scanner
                        mBarcodeScanner.Close();

                        if (mSettings == null)
                        {
                            mSettings = mBarcodeScanner.Settings;
                            SetScanner();
                        }

                        mSettings.Decode.Symbologies.Itf.LengthMin = length;
                        mSettings.Decode.Symbologies.Itf.LengthMax = length;

                        mSettings.Decode.Symbologies.Code128.LengthMin = length;
                        mSettings.Decode.Symbologies.Code128.LengthMax = length;

                        mBarcodeScanner.Settings = mSettings;
                        mBarcodeScanner.Claim();

                    }
                    catch (BarcodeException_ e)
                    {
                        Log.Error(TAG, "Error Code = " + e.ErrorCode, e);
                    }

                }

            }
            catch (BarcodeException_ e)
            {
                Log.Error(TAG, "ErrorCode is " + e.ErrorCode, e);
            }
            catch
            {
                Toast.MakeText(this, Resource.String.error_message_symbol_settings, ToastLength.Long).Show();
            }

        }
        private void SetScanner()
        {
            // Scanner default settings
            BarcodeScannerSettings_ settings = mBarcodeScanner.Settings;

            // Trigger Mode
            settings.Scan.TriggerMode = ScanSettings_.TriggerMode_.AutoOff;
            //settings.Scan.TriggerMode = ScanSettings_.TriggerMode_.Momentary;
            //settings.Scan.TriggerMode = ScanSettings_.TriggerMode_.Alternate;
            //settings.Scan.TriggerMode = ScanSettings_.TriggerMode_.Continuous;
            //settings.Scan.TriggerMode = ScanSettings_.TriggerMode_.TriggerRelease;

            // For 2D Module Settings
            if (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d)
            {
                // Light Mode
                settings.Scan.LightMode = ScanSettings_.LightMode_.Auto;
                //settings.Scan.LightMode = ScanSettings_.LightMode_.AlwaysOn;
                //settings.Scan.LightMode = ScanSettings_.LightMode_.Off;

                //Marker Mode
                settings.Scan.MarkerMode = ScanSettings_.MarkerMode_.Normal;
                //settings.Scan.MarkerMode = ScanSettings_.MarkerMode_.Ahead;
                //settings.Scan.MarkerMode = ScanSettings_.MarkerMode_.Off;
            }

            // For 2D LONG Module Settings
            if (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2dLong)
            {
                settings.Scan.SideLightMode = ScanSettings_.SideLightMode_.Off;
                //settings.Scan.SideLightMode = ScanSettings_.SideLightMode_.On;
            }

            // Notification Sound Settings
            settings.Notification.Sound.Enabled = true;
            //settings.Notification.Sound.Enabled = false;

            if (settings.Notification.Sound.Enabled)
            {
                settings.Notification.Sound.UsageType = NotificationSettings_.UsageType_.Ringtone;
                //settings.Notification.Sound.UsageType = NotificationSettings_.UsageType_.Media;
                //settings.Notification.Sound.UsageType = NotificationSettings_.UsageType_.Alarm;

                if (settings.Notification.Sound.UsageType == NotificationSettings_.UsageType_.Media)
                {
                    //TO BE Implement
                    settings.Notification.Sound.GoodDecodeFilePath = "";
                }
            }

            //Notification Vibrator
            //settings.Notification.Vibrate.Enabled = false;
            settings.Notification.Vibrate.Enabled = true;


            // Decode Settings

            // Decode interval
            settings.Decode.SameBarcodeIntervalTime = 255; // 1,000msec

            // For 1D & 2D Module Settings
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                // Decode Level
                settings.Decode.DecodeLevel = 4;              // Decode Level
            }

            // Invert Mode
            settings.Decode.InvertMode = DecodeSettings_.InvertMode_.Disabled;
            //settings.Decode.InvertMode = DecodeSettings_.InvertMode_.InversionOnly;
            //settings.Decode.InvertMode = DecodeSettings_.InvertMode_.Auto;

            // For 2D & 2D LONG Module Settings
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2dLong))
            {
                // Point Scan Mode
                settings.Decode.PointScanMode = DecodeSettings_.PointScanMode_.Disabled;
                //settings.Decode.PointScanMode = DecodeSettings_.PointScanMode_.Enabled;
            }

            // For 2D Module Settings
            if (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d)
            {
                // Reverse Mode
                settings.Decode.ReverseMode = DecodeSettings_.ReverseMode_.Disabled;
                //settings.Decode.ReverseMode = DecodeSettings_.ReverseMode_.Enabled;
            }

            // Encode Charset
            settings.Decode.Charset = Charset.ForName("Shift-JIS");
            //settings.Decode.Charset = Charset.ForName("UTF-8");


            // Symbology Settings

            // For 2D Module Settings
            if (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d)
            {
                // Multi Line
                settings.Decode.MultiLineMode.Enabled = false;
            }

            //JAN-13(EAN-13), UPC-A
            settings.Decode.Symbologies.Ean13UpcA.Enabled = true;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                settings.Decode.Symbologies.Ean13UpcA.FirstCharacter = "";
                settings.Decode.Symbologies.Ean13UpcA.SecondCharacter = "";
            }
            settings.Editing.Ean13.ReportCheckDigit = true;
            settings.Editing.UpcA.ReportCheckDigit = true;
            settings.Editing.UpcA.AddLeadingZero = true;

            // EAN-13 add on
            settings.Decode.Symbologies.Ean13UpcA.AddOn.Enabled = false;
            settings.Decode.Symbologies.Ean13UpcA.AddOn.OnlyWithAddOn = false;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                settings.Decode.Symbologies.Ean13UpcA.AddOn.AddOn2Digit = false;
                settings.Decode.Symbologies.Ean13UpcA.AddOn.AddOn5Digit = false;
            }

            // JAN-8(EAN-8)
            settings.Decode.Symbologies.Ean8.Enabled = true;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                settings.Decode.Symbologies.Ean8.FirstCharacter = "";
                settings.Decode.Symbologies.Ean8.SecondCharacter = "";
            }
            settings.Editing.Ean8.ReportCheckDigit = true;

            // EAN-8 add on
            settings.Decode.Symbologies.Ean8.AddOn.Enabled = false;
            settings.Decode.Symbologies.Ean8.AddOn.OnlyWithAddOn = false;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                settings.Decode.Symbologies.Ean8.AddOn.AddOn2Digit = false;
                settings.Decode.Symbologies.Ean8.AddOn.AddOn5Digit = false;
            }

            // UPC-E
            settings.Decode.Symbologies.UpcE.Enabled = true;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                settings.Decode.Symbologies.UpcE.FirstCharacter = "";
                settings.Decode.Symbologies.UpcE.SecondCharacter = "";
            }
            settings.Editing.UpcE.ReportCheckDigit = true;
            settings.Editing.UpcE.AddLeadingZero = false;
            settings.Editing.UpcE.ConvertToUpcA = false;
            settings.Editing.UpcE.ReportNumberSystemCharacterOfConvertedUpcA = true;

            // UPC-E add on
            settings.Decode.Symbologies.UpcE.AddOn.Enabled = false;
            settings.Decode.Symbologies.UpcE.AddOn.OnlyWithAddOn = false;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                settings.Decode.Symbologies.UpcE.AddOn.AddOn2Digit = false;
                settings.Decode.Symbologies.UpcE.AddOn.AddOn5Digit = false;
            }

            // ITF
            settings.Decode.Symbologies.Itf.Enabled = true;
            settings.Decode.Symbologies.Itf.LengthMin = 20;
            settings.Decode.Symbologies.Itf.LengthMax = 20;
            settings.Decode.Symbologies.Itf.VerifyCheckDigit = false;
            //settings.Decode.Symbologies.itf.verifyCheckDigit = true;
            settings.Editing.Itf.ReportCheckDigit = true;

            // STF
            settings.Decode.Symbologies.Stf.Enabled = true;
            settings.Decode.Symbologies.Stf.LengthMin = 4;
            settings.Decode.Symbologies.Stf.LengthMax = 99;
            settings.Decode.Symbologies.Stf.VerifyCheckDigit = false;
            //settings.Decode.Symbologies.stf.verifyCheckDigit = true;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2dLong))
            {
                settings.Decode.Symbologies.Stf.StartStopCharacter = "";
                //settings.Decode.Symbologies.Stf.StartStopCharacter = "S";
                //settings.Decode.Symbologies.Stf.StartStopCharacter = "N";
            }
            settings.Editing.Stf.ReportCheckDigit = true;

            // Codabar
            settings.Decode.Symbologies.Codabar.Enabled = true;
            settings.Decode.Symbologies.Codabar.LengthMin = 4;
            settings.Decode.Symbologies.Codabar.LengthMax = 99;
            settings.Decode.Symbologies.Codabar.VerifyCheckDigit = false;
            //settings.Decode.Symbologies.Codabar.VerifyCheckDigit = true;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                settings.Decode.Symbologies.Codabar.StartStopCharacter = "";
            }
            settings.Editing.Codabar.ReportCheckDigit = true;
            settings.Editing.Codabar.ReportStartStopCharacter = true;
            settings.Editing.Codabar.ConvertToUpperCase = false;

            // Code39
            settings.Decode.Symbologies.Code39.Enabled = true;
            settings.Decode.Symbologies.Code39.LengthMin = 1;
            settings.Decode.Symbologies.Code39.LengthMax = 99;
            settings.Decode.Symbologies.Code39.VerifyCheckDigit = false;
            //settings.Decode.Symbologies.Code39.VerifyCheckDigit = true;
            settings.Editing.Code39.ReportCheckDigit = true;
            settings.Editing.Code39.ReportStartStopCharacter = false;

            // Code93
            settings.Decode.Symbologies.Code93.Enabled = true;
            settings.Decode.Symbologies.Code93.LengthMin = 1;
            settings.Decode.Symbologies.Code93.LengthMax = 99;

            // Code128
            settings.Decode.Symbologies.Code128.Enabled = true;
            settings.Decode.Symbologies.Code128.LengthMin = 1;
            settings.Decode.Symbologies.Code128.LengthMax = 99;

            // MSI
            if (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d)
            {
                settings.Decode.Symbologies.Msi.Enabled = true;
                settings.Decode.Symbologies.Msi.LengthMin = 1;
                settings.Decode.Symbologies.Msi.LengthMax = 99;
                settings.Decode.Symbologies.Msi.NumberOfCheckDigitVerification = 1;
                //settings.Decode.Symbologies.Msi.NumberOfCheckDigitVerification = 2;
            }

            // GS1 Databar
            settings.Decode.Symbologies.Gs1DataBar.Enabled = true;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                settings.Decode.Symbologies.Gs1DataBar.Stacked = false;
            }

            // Gs1 Databar Limited
            settings.Decode.Symbologies.Gs1DataBarLimited.Enabled = false;

            // Gs1 Databar Expanded
            settings.Decode.Symbologies.Gs1DataBarExpanded.Enabled = false;
            settings.Decode.Symbologies.Gs1DataBarExpanded.LengthMin = 1;
            settings.Decode.Symbologies.Gs1DataBarExpanded.LengthMax = 99;
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type1d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d))
            {
                settings.Decode.Symbologies.Gs1DataBarExpanded.Stacked = false;
            }

            // Gs1 Composite
            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2dLong))
            {
                settings.Decode.Symbologies.Gs1Composite.Enabled = false;
            }

            if ((mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d) ||
                    (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2dLong))
            {

                // QR Code
                settings.Decode.Symbologies.QrCode.Enabled = false;

                if (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d)
                {
                    settings.Decode.Symbologies.QrCode.SplitMode = Symbologies_.SplitModeQr_.Disabled;
                    //settings.Decode.Symbologies.QrCode.SplitMode = Symbologies_.SplitModeQr_.Edit;
                    //settings.Decode.Symbologies.QrCode.SplitMode = Symbologies_.SplitModeQr_.BatchEdit;
                    //settings.Decode.Symbologies.QrCode.SplitMode = Symbologies_.SplitModeQr_.NonEdit;

                    // QR Code Model1
                    settings.Decode.Symbologies.QrCode.Model1.Enabled = true;
                    settings.Decode.Symbologies.QrCode.Model1.VersionMin = 1;
                    settings.Decode.Symbologies.QrCode.Model1.VersionMax = 22;

                    // QR Code Model2
                    settings.Decode.Symbologies.QrCode.Model2.Enabled = true;
                    settings.Decode.Symbologies.QrCode.Model2.VersionMin = 1;
                    settings.Decode.Symbologies.QrCode.Model2.VersionMax = 40;

                    // Micro QR Code
                    settings.Decode.Symbologies.MicroQr.Enabled = true;
                    settings.Decode.Symbologies.MicroQr.VersionMin = 1;
                    settings.Decode.Symbologies.MicroQr.VersionMax = 4;

                    // iQR Code
                    settings.Decode.Symbologies.IqrCode.Enabled = true;
                    settings.Decode.Symbologies.IqrCode.SplitMode = Symbologies_.SplitModeIqr_.Disabled;
                    //settings.Decode.Symbologies.IqrCode.SplitMode = Symbologies_.SplitModeIqr_.Edit;
                    //settings.Decode.Symbologies.IqrCode.SplitMode = Symbologies_.SplitModeIqr_.NonEdit;

                    // Square iQR Code
                    settings.Decode.Symbologies.IqrCode.Square.Enabled = true;
                    settings.Decode.Symbologies.IqrCode.Square.VersionMin = 1;
                    settings.Decode.Symbologies.IqrCode.Square.VersionMax = 61;

                    // Rectangle iQR Code
                    settings.Decode.Symbologies.IqrCode.Rectangle.Enabled = true;
                    settings.Decode.Symbologies.IqrCode.Rectangle.VersionMin = 1;
                    settings.Decode.Symbologies.IqrCode.Rectangle.VersionMax = 15;
                }
                else
                {
                    //For 2D Long model
                    settings.Decode.Symbologies.MicroQr.Enabled = false;
                    settings.Decode.Symbologies.IqrCode.Enabled = false;
                }

                // Data Matrix
                settings.Decode.Symbologies.DataMatrix.Enabled = true;

                if (mScannerType == BarcodeScannerInfo_.BarcodeScannerType_.Type2d)
                {
                    // DataMatrix Square
                    settings.Decode.Symbologies.DataMatrix.Square.Enabled = true;
                    settings.Decode.Symbologies.DataMatrix.Square.CodeNumberMin = 1;
                    settings.Decode.Symbologies.DataMatrix.Square.CodeNumberMax = 24;

                    // DataMatrix ReactAngle
                    settings.Decode.Symbologies.DataMatrix.Rectangle.Enabled = true;
                    settings.Decode.Symbologies.DataMatrix.Rectangle.CodeNumberMin = 1;
                    settings.Decode.Symbologies.DataMatrix.Rectangle.CodeNumberMax = 6;
                }

                // PDF417
                settings.Decode.Symbologies.Pdf417.Enabled = true;

                // Micro PDF 417
                settings.Decode.Symbologies.MicroPdf417.Enabled = true;

                // Maxi
                settings.Decode.Symbologies.MaxiCode.Enabled = true;
            }
            else
            {
                //For 1D model
                settings.Decode.Symbologies.QrCode.Enabled = false;
                settings.Decode.Symbologies.MicroQr.Enabled = false;
                settings.Decode.Symbologies.IqrCode.Enabled = false;
                settings.Decode.Symbologies.Pdf417.Enabled = false;
                settings.Decode.Symbologies.MicroPdf417.Enabled = false;
                settings.Decode.Symbologies.MaxiCode.Enabled = false;
                settings.Decode.Symbologies.DataMatrix.Enabled = false;
            }

            mBarcodeScanner.Settings = settings;
        }
    }
}
