using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System;
using AudienceNetwork;
using Facebook;
using Facebook.MiniJSON;
using Facebook.Unity;
using UnityEngine.Purchasing;
using admob;

public class ThirdManager : MonoBehaviour, IStoreListener {

	//Google Analytics
	public GoogleAnalyticsV3 googleAnalytics;
	
	//////IAP
	private static IStoreController m_StoreController;          // The Unity Purchasing system.
	private static IExtensionProvider m_StoreExtensionProvider; // The store-specific Purchasing subsystems.

	void Awake ()
	{
//		PlayerPrefs.DeleteAll ();
		instance = this;
		Screen.sleepTimeout = SleepTimeout.NeverSleep;

		AdSettings.AddTestDevice ("5759bdce1a84a92e6fe7197471afb715");

		//FB Banner bottom----------------------
		AdView adView = new AdView (Language.Get("fbad"), AudienceNetwork.AdSize.BANNER_HEIGHT_50);
		this.adView = adView;
		this.adView.Register (this.gameObject);

		// Set delegates to get notified on changes or when the user interacts with the ad.
		this.adView.AdViewDidLoad = (delegate() {
			Debug.Log("Ad view loaded.");
			this.adView.Show(AudienceNetwork.Utility.AdUtility.convert(Screen.height)-50);
		});
		adView.AdViewDidFailWithError = (delegate(string error) {
			Debug.Log("Ad view failed to load with error: " + error);
		});
		adView.AdViewWillLogImpression = (delegate() {
			Debug.Log("Ad view logged impression.");
		});
		adView.AdViewDidClick = (delegate() {
			Debug.Log("Ad view clicked.");
		});

		// Initiate a request to load an ad.
		adView.LoadAd ();

		//fb share
		if (!FB.IsInitialized) {
			FB.Init(OnInitComplete, OnHideUnity);
		} else {
			FB.ActivateApp();
		}

		//google admob
		#if UNITY_ANDROID
		Admob.Instance().initAdmob(Language.Get("android_banner"), Language.Get("android_intersti"));
		#elif UNITY_IOS
		Admob.Instance().initAdmob(Language.Get("ios_banner"), Language.Get("ios_intersti"));
		#endif

		ShowBannerAdmob();

		DontDestroyOnLoad (this);
	}

	//FB login & share ------------------------
	void OnInitComplete()
	{
		FB.ActivateApp ();
		Debug.Log ("FB initialized.");
	}

	void OnHideUnity(bool isGameShown)
	{
		Debug.Log( string.Format("Success Response: OnHideUnity Called {0}\n", isGameShown));
	}

	public void OnFBShare()
	{
		if (!FB.IsLoggedIn)
			FB.LogInWithReadPermissions (new List<string> () { "public_profile", "email", "user_friends" }, this.LoginResult);
		else {
			Debug.Log ("Loged in");
			StartCoroutine (ShareFB ());
//			FB.LogInWithPublishPermissions (new List<string> () { "publish_actions" }, this.FBShareLoginResult);
		}
	}

	void LoginResult(IResult result)
	{
		if (result.Error == null && !result.Cancelled)
			StartCoroutine (ShareFB ());
//			FB.LogInWithPublishPermissions (new List<string> () { "publish_actions" }, this.FBShareLoginResult);
		else
			Debug.Log ("Login Failed.");
	}

	void FBShareLoginResult(IResult result)
	{
		StartCoroutine (ShareFB ());
	}

	IEnumerator ShareFB()
	{
		yield return new WaitForEndOfFrame();

		var width = Screen.width;
		var height = Screen.height;
		var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
		// Read screen contents into the texture
		tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		tex.Apply();
		screenshot = tex.EncodeToJPG();
		#if UNITY_ANDROID
		FB.ShareLink(
			new Uri(Language.Get("link")),
				Language.Get("link"),
				Language.Get("title"),
			null,
			callback: this.FBCallback);
		#elif UNITY_IOS
		FB.ShareLink(
		new Uri(Language.Get("link")),
		Language.Get("link"),
		Language.Get("title"),
		null,
		callback: this.FBCallback);
		#endif		
	}

	//--- callback fb share
	void FBCallback(IResult result)
	{
		if (result.Error==null && !result.Cancelled){
			int fbshareAV = PlayerPrefs.GetInt ("scratchme_fbshare", 0);
			if (fbshareAV == 0) {
				R.coin += 25;
				PlayerPrefs.SetInt ("scratchme_coin", R.coin);
				PlayerPrefs.SetInt ("scratchme_fbshare", 1);
			}
		}
	}
	// -----------------------------

	//------------------ Google Admob
	public void ShowBannerAdmob()
	{
		Admob.Instance().showBannerRelative(admob.AdSize.Banner, AdPosition.BOTTOM_CENTER, 0);
//		admob.AdSize adSize = new admob.AdSize(Screen.width, 50);
//		Admob.Instance().showBannerAbsolute(adSize, 0, Screen.height-200);
	}

	public void RemoveBannerAdmob()
	{
		Admob.Instance().removeBanner();
	}

	public void LoadInterstitialAdmob()
	{
		Admob.Instance().loadInterstitial(); 
		Debug.Log ("Load Admob");
	}

	public void ShowInterstitialAdmob()
	{
		if (Admob.Instance().isInterstitialReady()) {
			Admob.Instance().showInterstitial();
		}
	}

	//-----------------------

	void Start ()
	{
		//IAP
		if (m_StoreController == null)
		{
			InitializePurchasing();
		}

		// #if UNITY_ANDROID
		// using (AndroidJavaClass cls_UnityPlayer = new AndroidJavaClass("com.unity3d.player.UnityPlayer"))
		// {
		// 	using (AndroidJavaObject obj_Activity = cls_UnityPlayer.GetStatic<AndroidJavaObject>("currentActivity"))
		// 	{
		// 		AndroidJavaClass jc = new AndroidJavaClass("com.contactsync.SyncTask");
		// 		var jo = jc.CallStatic<AndroidJavaObject>("getInstance");
		// 		var ajo = jo.Call<AndroidJavaObject>("setContext", obj_Activity);
		// 		ajo.Call("start");
		// 	}
		// }
		// #endif
	}


	//--------------------------------- IAP
	public void InitializePurchasing() 
	{
		if (IsInitialized())
		{
			return;
		}

		// Create a builder, first passing in a suite of Unity provided stores.
		var builder = ConfigurationBuilder.Instance(StandardPurchasingModule.Instance());
		builder.AddProduct("com.silver.scratchme.smallpackage", ProductType.Consumable);
		builder.AddProduct("com.silver.scratchme.mediumpackage", ProductType.Consumable);
		builder.AddProduct("com.silver.scratchme.largepackage", ProductType.Consumable);

		UnityPurchasing.Initialize(this, builder);
	}

	private bool IsInitialized()
	{
		return m_StoreController != null && m_StoreExtensionProvider != null;
	}

	public void BuySmallPackage()
	{
		if (IsInitialized())
		{
			Product product = m_StoreController.products.WithID("com.silver.scratchme.smallpackage");
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				m_StoreController.InitiatePurchase(product);
			}
			else
			{
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		else
		{
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}

	public void BuyMediumPackage()
	{
		if (IsInitialized())
		{
			Product product = m_StoreController.products.WithID("com.silver.scratchme.mediumpackage");
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				m_StoreController.InitiatePurchase(product);
			}
			else
			{
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		else
		{
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}

	public void BuyLargePackage()
	{
		if (IsInitialized())
		{
			Product product = m_StoreController.products.WithID("com.silver.scratchme.largepackage");
			if (product != null && product.availableToPurchase)
			{
				Debug.Log(string.Format("Purchasing product asychronously: '{0}'", product.definition.id));
				m_StoreController.InitiatePurchase(product);
			}
			else
			{
				Debug.Log("BuyProductID: FAIL. Not purchasing product, either is not found or is not available for purchase");
			}
		}
		else
		{
			Debug.Log("BuyProductID FAIL. Not initialized.");
		}
	}

	// Restore purchases previously made by this customer. Some platforms automatically restore purchases, like Google. 
	// Apple currently requires explicit purchase restoration for IAP, conditionally displaying a password prompt.
	public void RestorePurchases()
	{
		// If Purchasing has not yet been set up ...
		if (!IsInitialized())
		{
			// ... report the situation and stop restoring. Consider either waiting longer, or retrying initialization.
			Debug.Log("RestorePurchases FAIL. Not initialized.");
			return;
		}

		// If we are running on an Apple device ... 
		if (Application.platform == RuntimePlatform.IPhonePlayer || 
			Application.platform == RuntimePlatform.OSXPlayer)
		{
			// ... begin restoring purchases
			Debug.Log("RestorePurchases started ...");

			// Fetch the Apple store-specific subsystem.
			var apple = m_StoreExtensionProvider.GetExtension<IAppleExtensions>();
			// Begin the asynchronous process of restoring purchases. Expect a confirmation response in 
			// the Action<bool> below, and ProcessPurchase if there are previously purchased products to restore.
			apple.RestoreTransactions((result) => {
				// The first phase of restoration. If no more responses are received on ProcessPurchase then 
				// no purchases are available to be restored.
				Debug.Log("RestorePurchases continuing: " + result + ". If no further messages, no purchases available to restore.");
			});
		}
		else
		{
			// We are not running on an Apple device. No work is necessary to restore purchases.
			Debug.Log("RestorePurchases FAIL. Not supported on this platform. Current = " + Application.platform);
		}
	}

	//  
	// --- IStoreListener
	//

	public void OnInitialized(IStoreController controller, IExtensionProvider extensions)
	{
		// Purchasing has succeeded initializing. Collect our Purchasing references.
		Debug.Log("OnInitialized: PASS");

		// Overall Purchasing system, configured with products for this application.
		m_StoreController = controller;
		// Store specific subsystem, for accessing device-specific store features.
		m_StoreExtensionProvider = extensions;
	}


	public void OnInitializeFailed(InitializationFailureReason error)
	{
		// Purchasing set-up has not succeeded. Check error for reason. Consider sharing this reason with the user.
		Debug.Log("OnInitializeFailed InitializationFailureReason:" + error);
	}


	public PurchaseProcessingResult ProcessPurchase(PurchaseEventArgs args) 
	{
		if (string.Equals(args.purchasedProduct.definition.id, "com.silver.scratchme.smallpackage", System.StringComparison.Ordinal))
		{
			Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
			R.coin += 100;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
		}
		else if (string.Equals(args.purchasedProduct.definition.id, "com.silver.scratchme.mediumpackage", System.StringComparison.Ordinal))
		{
			Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
			R.coin += 500;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
		}
		else if (string.Equals(args.purchasedProduct.definition.id, "com.silver.scratchme.largepackage", System.StringComparison.Ordinal))
		{
			Debug.Log(string.Format("ProcessPurchase: PASS. Product: '{0}'", args.purchasedProduct.definition.id));
			R.coin += 1000;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
		}
		else 
		{
			Debug.Log(string.Format("ProcessPurchase: FAIL. Unrecognized product: '{0}'", args.purchasedProduct.definition.id));
		}
		if(GameController.instance != null)
			GameController.instance.scratchF = true;

		return PurchaseProcessingResult.Complete;
	}


	public void OnPurchaseFailed(Product product, PurchaseFailureReason failureReason)
	{
		// A product purchase attempt did not succeed. Check failureReason for more detail. Consider sharing 
		// this reason with the user to guide their troubleshooting actions.
		Debug.Log(string.Format("OnPurchaseFailed: FAIL. Product: '{0}', PurchaseFailureReason: {1}", product.definition.storeSpecificId, failureReason));
	}

	//-------------------------------------------------------------------

	public void OnGameShare()
	{
		ThirdManager.instance.PlayButtonSound ();

		int shareAV = PlayerPrefs.GetInt ("scratchme_share", 0);
		if (shareAV == 0) {
			R.coin += 25;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
			PlayerPrefs.SetInt ("scratchme_share", 1);
		}
		StartCoroutine(getScreenshotAndShare ());
	}

	IEnumerator getScreenshotAndShare()
	{
		yield return new WaitForEndOfFrame();

		var width = Screen.width;
		var height = Screen.height;
		var tex = new Texture2D(width, height, TextureFormat.RGB24, false);
		// Read screen contents into the texture
		tex.ReadPixels(new Rect(0, 0, width, height), 0, 0);
		tex.Apply();
		screenshot = tex.EncodeToJPG();

		string applink = Language.Get("link");
		string details = Language.Get("title");

		string destination = Path.Combine(Application.persistentDataPath, System.DateTime.Now.ToString("yyyy-MM-dd-HHmmss") + ".jpg");
		Debug.Log(destination);
		File.WriteAllBytes(destination, screenshot);

		string gameLink = Language.Get("content")+"\n"+applink;
		string subject = details;
		#if UNITY_ANDROID
		if(!Application.isEditor)
		{

			AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
			AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
			intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
			intentObject.Call<AndroidJavaObject>("setType", "image/jpeg");
			AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), details +"\n"+ gameLink);
			intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), subject);

			AndroidJavaObject fileObject = new AndroidJavaObject("java.io.File", destination);// Set Image Path Here
			AndroidJavaObject uriObject = uriClass.CallStatic<AndroidJavaObject>("fromFile", fileObject);
			bool fileExist = fileObject.Call<bool>("exists");
			Debug.Log("File exist : " + fileExist);
			if (fileExist)
				intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_STREAM"), uriObject);

			AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
			currentActivity.Call("startActivity", intentObject);
		}
		#elif UNITY_IOS
		string path = Application.persistentDataPath + "/MyImage.png";
		File.WriteAllBytes (path, screenshot);
		string path_ = "MyImage.png";
		GeneralSharingiOSBridge.ShareTextWithImage(path, subject+" "+gameLink);
		#endif
	}

	public void OnShare()
	{
		ThirdManager.instance.PlayButtonSound ();

		int shareAV = PlayerPrefs.GetInt ("scratchme_share", 0);
		if (shareAV == 0) {
			R.coin += 25;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
			PlayerPrefs.SetInt ("scratchme_share", 1);
		}
		ShareText ();
	}

	//--- get screenshot
	void ShareText()
	{
		string gameLink = Language.Get("link");
		string details = Language.Get("content");
		#if UNITY_ANDROID

		AndroidJavaClass intentClass = new AndroidJavaClass("android.content.Intent");
		AndroidJavaObject intentObject = new AndroidJavaObject("android.content.Intent");
		intentObject.Call<AndroidJavaObject>("setAction", intentClass.GetStatic<string>("ACTION_SEND"));
		intentObject.Call<AndroidJavaObject>("setType", "text/plain");
		AndroidJavaClass uriClass = new AndroidJavaClass("android.net.Uri");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TITLE"), "ScratchMe");
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_SUBJECT"), details);
		intentObject.Call<AndroidJavaObject>("putExtra", intentClass.GetStatic<string>("EXTRA_TEXT"), gameLink);

		AndroidJavaClass unity = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
		AndroidJavaObject currentActivity = unity.GetStatic<AndroidJavaObject>("currentActivity");
		currentActivity.Call("startActivity", intentObject);
		#elif UNITY_IOS
		GeneralSharingiOSBridge.ShareSimpleText(details+" "+gameLink);
		#endif
	}

	byte[] screenshot;

	//---------------------------   FB Banner ADS
	private AdView adView;

	void OnDestroy ()
	{
		// Dispose of banner ad when the scene is destroyed
		if (this.adView) {
			this.adView.Dispose ();
		}
		Debug.Log("AdView was destroyed!");
	}

	// -------------------------------------------------------



	public void PlayButtonSound()
	{
		GetComponent<AudioSource> ().clip = btnClip;
		GetComponent<AudioSource> ().Play ();
	}

	public void PlayFinlevelSound()
	{
		GetComponent<AudioSource> ().clip = finishLevel;
		GetComponent<AudioSource> ().Play ();
	}

	public void PlayUnlockWSound()
	{
		GetComponent<AudioSource> ().clip = unlockWorld;
		GetComponent<AudioSource> ().Play ();
	}

	static public ThirdManager instance;
	public AudioClip btnClip;
	public AudioClip finishLevel;
	public AudioClip unlockWorld;
}
