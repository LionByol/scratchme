using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;
using AudienceNetwork;

public class GameUI : MonoBehaviour {

	void Awake()
	{
		instance = this;
		end = false;
		LoadInterstitial ();
		ThirdManager.instance.LoadInterstitialAdmob ();
	}

	// Use this for initialization
	void Start () {
		R.scratching = PlayerPrefs.GetFloat ("scratchme_scratching"+R.world_selected, 1);
		giveletter = 0;
	}

	void OnDestroy ()
	{
		// Dispose of interstitial ad when the scene is destroyed
		if (this.interstitialAd != null) {
			this.interstitialAd.Dispose ();
		}
		Debug.Log("InterstitialAd was destroyed!");
	}

	//----------------------------- FB Interstitial ADS
	private InterstitialAd interstitialAd;
	private bool isLoaded;

	// Load button
	public void LoadInterstitial ()
	{
		// Create the interstitial unit with a placement ID (generate your own on the Facebook app settings).
		// Use different ID for each ad placement in your app.
		InterstitialAd interstitialAd = new InterstitialAd ("1802412156640208_1802413249973432");
		this.interstitialAd = interstitialAd;
		this.interstitialAd.Register (this.gameObject);

		// Set delegates to get notified on changes or when the user interacts with the ad.
		this.interstitialAd.InterstitialAdDidLoad = (delegate() {
			Debug.Log("Interstitial ad loaded.");
			this.isLoaded = true;
		});
		interstitialAd.InterstitialAdDidFailWithError = (delegate(string error) {
			Debug.Log("Interstitial ad failed to load with error: " + error);
		});
		interstitialAd.InterstitialAdWillLogImpression = (delegate() {
			Debug.Log("Interstitial ad logged impression.");
		});
		interstitialAd.InterstitialAdDidClick = (delegate() {
			Debug.Log("Interstitial ad clicked.");
		});

		// Initiate the request to load the ad.
		this.interstitialAd.LoadAd ();
	}

	// Show button
	public void ShowInterstitial ()
	{
		if (this.isLoaded) 
		{
			this.interstitialAd.Show ();
			this.isLoaded = false;
			Debug.Log ("Show AD");
		} 
		else
		{
			ThirdManager.instance.ShowInterstitialAdmob ();
		}
	}
	//--------------------------------------------------------------------
	
	// Update is called once per frame
	void Update ()
	{
		if (!end && R.gamestatus != "none") {
			end = true;
			ThirdManager.instance.PlayFinlevelSound ();
			GameController.instance.scratchPanel.gameObject.SetActive (false);

			//interstitial 
			R.interstitialAD ++;
			if (R.interstitialAD % 2 == 0)
				ShowInterstitial ();
			
			StartCoroutine (ShowGameEnd());
		}

		if (Input.GetKeyUp (KeyCode.Escape)) {
			if (clueLab.activeSelf) {
				clueLab.SetActive (false);
				GameController.instance.scratchF = true;
			}else if (earnpoints.activeSelf) {
				earnpoints.SetActive (false);
				GameController.instance.scratchF = true;
			}else 
				SceneManager.LoadScene ("world");
		}

		//change scratch slider
		scratchSlider.value = R.scratching;
		worldLab.text = "" + (R.world_selected + 1);
		levelLab.text = "" + (R.level+1);
		coinLab.text = "" + R.coin;

		//update answer letters
		for (int i = 0; i < blanks.Length; i++) {
			if(i<R.result.Length)
				blanks [i].GetComponent<Blank> ().letter = R.result.Substring (i, 1);
			else
				blanks [i].GetComponent<Blank> ().letter = "";
		}
	}

	IEnumerator ShowGameEnd()
	{
		yield return new WaitForSeconds (1.0f);
		gameEnd.SetActive (true);

		GameController.instance.EraseStatus ();
		//calculate game score
		int score = 0;
		if (R.scratching < 0.01f) {
			score = 10;
		} else {
			score = 10 + (int)(R.scratching * 50 / (0.95f - 0.01f));
		}

		R.coin += score;
		PlayerPrefs.SetInt ("scratchme_coin", R.coin);

		//update ui
		clearLab.text = "" + (R.level + 1);
		scoreLab.text = "" + score;
		worldscoreLab.text = "" + R.coin;

		R.worlds [R.world_selected].successed++;

		float rat = (float)(R.worlds [R.world_selected].successed) / R.worlds [R.world_selected].answers.Length;
		if (rat > 0.8f) {
			if (R.world_selected < R.worlds.Length - 1 && R.worlds [R.world_selected + 1].opened == 0) {
				ThirdManager.instance.PlayUnlockWSound ();
				R.worlds [R.world_selected + 1].opened = 1;
				PlayerPrefs.SetInt ("ScratchMe_opened" + (R.world_selected+1), R.worlds [R.world_selected+1].opened);
			}
		}

		//calculate level
		R.level++;
		if (R.level >= R.worlds [R.world_selected].answers.Length) {
			R.level = 0;
			if (R.world_selected < R.worlds.Length - 1)
				R.world_selected++;
		} else {
			R.worlds [R.world_selected].opened = R.level+1;
			PlayerPrefs.SetInt ("ScratchMe_opened" + R.world_selected, R.worlds [R.world_selected].opened);
			PlayerPrefs.SetInt ("ScratchMe_successed" + R.world_selected, R.worlds [R.world_selected].successed);
		}

		//update stars
		if (R.scratching < 0.33f) {
			starSprites [0].sprite2D = okSprite;
			starSprites [1].sprite2D = noSprite;
			starSprites [2].sprite2D = noSprite;
		} else if (R.scratching < 0.67f) {
			starSprites [0].sprite2D = okSprite;
			starSprites [1].sprite2D = okSprite;
			starSprites [2].sprite2D = noSprite;
		} else {
			starSprites [0].sprite2D = okSprite;
			starSprites [1].sprite2D = okSprite;
			starSprites [2].sprite2D = okSprite;
		}

	}

	public void OnGameShare()
	{
		ThirdManager.instance.PlayButtonSound ();
		ThirdManager.instance.OnGameShare ();

		int shareAV = PlayerPrefs.GetInt ("scratchme_share", 0);
		if (shareAV == 0) {
			R.coin += 25;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
			PlayerPrefs.SetInt ("scratchme_share", 1);
		}
		GameController.instance.scratchF = true;
	}

	public void OnShare()
	{
		ThirdManager.instance.PlayButtonSound ();
		ThirdManager.instance.OnShare ();

		int shareAV = PlayerPrefs.GetInt ("scratchme_share", 0);
		if (shareAV == 0) {
			R.coin += 25;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
			PlayerPrefs.SetInt ("scratchme_share", 1);
		}
		GameController.instance.scratchF = true;
	}

	public void OnFBShare()
	{
		ThirdManager.instance.PlayButtonSound ();
		ThirdManager.instance.OnFBShare ();
		earnpoints.SetActive (false);
		GameController.instance.scratchF = true;
	}

	public void OnGetHint()
	{
		ThirdManager.instance.PlayButtonSound ();
		clueLab.SetActive (true);
		GameController.instance.scratchF = false;
	}

	public void OnClose()
	{
		if (clueLab.activeSelf)
			clueLab.SetActive (false);
		else if (earnpoints.activeSelf)
			earnpoints.SetActive (false);
		GameController.instance.scratchF = true;
	}

	public void OnBack()
	{
		ThirdManager.instance.PlayButtonSound ();
		GameController.instance.SaveStatus ();
		SceneManager.LoadScene ("world");
	}

	public void OnMenu()
	{
		ThirdManager.instance.PlayButtonSound ();
		SceneManager.LoadScene ("world");
	}

	public void OnNext()
	{
		ThirdManager.instance.PlayButtonSound ();
		SceneManager.LoadScene ("game");
	}

	public void OnSkip()
	{
		GameController.instance.scratchF = true;
		ThirdManager.instance.PlayButtonSound ();
		if (R.coin >= 100) {
			R.gamestatus = "success";
			end = true;
			ThirdManager.instance.PlayFinlevelSound ();
			GameController.instance.scratchPanel.gameObject.SetActive (false);
			clueLab.SetActive (false);

			R.interstitialAD++;
			if (R.interstitialAD % 2 == 0)
				ShowInterstitial ();	
			StartCoroutine (ShowSkipEnd());						
		} else {
			clueLab.SetActive (false);
			OnOpenBuy ();
		}
	}

	IEnumerator ShowSkipEnd()
	{
		yield return new WaitForSeconds (1.0f);
		gameEnd.SetActive (true);

		GameController.instance.EraseStatus ();	
		//calculate game score
		int score = 0;
		if (R.scratching < 0.01f) {
			score = 10;
		} else {
			score = 10 + (int)(R.scratching * 50 / (0.95f - 0.01f));
		}
		R.coin += score;
		PlayerPrefs.SetInt ("scratchme_coin", R.coin);

		//update ui
		clearLab.text = "Level " + (R.level + 1) + " Cleared";
		scoreLab.text = "" + score;
		worldscoreLab.text = "" + R.coin;

		//calculate level
		R.level++;
		if (R.level >= R.worlds [R.world_selected].answers.Length) {
			R.level = 0;
			if (R.world_selected < R.worlds.Length - 1)
				R.world_selected++;
		} else {
			R.worlds [R.world_selected].opened = R.level+1;
			PlayerPrefs.SetInt ("ScratchMe_opened" + R.world_selected, R.worlds [R.world_selected].opened);
			PlayerPrefs.SetInt ("ScratchMe_successed" + R.world_selected, R.worlds [R.world_selected].successed);
		}

		R.worlds [R.world_selected].successed++;

		float rat = (float)(R.worlds [R.world_selected].successed) / R.worlds [R.world_selected].answers.Length;
		if (rat > 0.8f) {
			if (R.world_selected < R.worlds.Length - 1 && R.worlds [R.world_selected + 1].opened == 0) {
				ThirdManager.instance.PlayUnlockWSound ();
				R.worlds [R.world_selected + 1].opened = 1;
				PlayerPrefs.SetInt ("ScratchMe_opened" + (R.world_selected+1), R.worlds [R.world_selected+1].opened);
			}
		}

		R.worlds [R.world_selected].opened = R.level + 1;
		PlayerPrefs.SetInt ("ScratchMe_opened" + R.world_selected, R.worlds [R.world_selected].opened);
		PlayerPrefs.SetInt ("ScratchMe_successed" + R.world_selected, R.worlds [R.world_selected].successed);

		R.coin -= 100;
		PlayerPrefs.SetInt ("scratchme_coin", R.coin);

		//update stars
		if (R.scratching < 0.33f) {
			starSprites [0].sprite2D = okSprite;
			starSprites [1].sprite2D = noSprite;
			starSprites [2].sprite2D = noSprite;
		} else if (R.scratching < 0.67f) {
			starSprites [0].sprite2D = okSprite;
			starSprites [1].sprite2D = okSprite;
			starSprites [2].sprite2D = noSprite;
		} else {
			starSprites [0].sprite2D = okSprite;
			starSprites [1].sprite2D = okSprite;
			starSprites [2].sprite2D = okSprite;
		}

	}

	public void OnLetter()
	{
		GameController.instance.scratchF = true;
		ThirdManager.instance.PlayButtonSound ();
		if (R.coin >= 50) {
			giveletter = 0;
			string restStr = "";
			for (int i = 0; i < R.result.Length; i++) {
				string res = R.result.Substring (i, 1);
				string tgr = R.worlds [R.world_selected].answers [R.level].Substring (i, 1).ToUpper();
				if (res.Equals (tgr))
					giveletter++;
				else {
					restStr = R.result.Substring (i);
					break;
				}
			}
			if (giveletter < R.worlds [R.world_selected].answers [R.level].Length)
				giveletter++;
			R.result = R.worlds [R.world_selected].answers [R.level].Substring (0, giveletter).ToUpper ();

			//show all keys
			for(int i=0; i<GameUI.instance.key.Length; i++)
			{
				GameUI.instance.key [i].gameObject.transform.parent.gameObject.SetActive (true);
			}

			//hidden right keys
			for (int i = 0; i < R.result.Length; i++) {
				string ch = R.result.Substring (i, 1).ToUpper();
				for(int k=0; k<GameUI.instance.key.Length; k++)
				{
					if (GameUI.instance.key [k].gameObject.transform.parent.gameObject.activeSelf && GameUI.instance.key [k].text.Equals (ch)) {
						GameUI.instance.key [k].gameObject.transform.parent.gameObject.SetActive (false);
						break;
					}
				}
			}

			R.coin -= 50;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
			clueLab.SetActive (false);
			GameController.instance.scratchF = true;
		} else {
			clueLab.SetActive (false);
			OnOpenBuy ();
		}
	}

	public void OnRefill()
	{
		GameController.instance.scratchF = true;
		ThirdManager.instance.PlayButtonSound ();
		if (R.coin >= 50) {
			R.scratching = 1f;
			R.coin -= 50;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
			clueLab.SetActive (false);
			GameController.instance.scratchF = true;
		} else {
			clueLab.SetActive (false);
			OnOpenBuy ();
		}
	}

	void OnOpenBuy()
	{
		earnpoints.SetActive (true);
		GameController.instance.scratchF = false;
	}

	public void OnRate()
	{
		ThirdManager.instance.PlayButtonSound ();
		#if UNITY_ANDROID
		Application.OpenURL ("https://play.google.com/store/apps/details?id=com.silver.scratchme");
		#elif UNITY_IOS
		Application.OpenURL("");
		#endif
		int rateAV = PlayerPrefs.GetInt ("scratchme_rate", 0);
		if (rateAV == 0) {
			R.coin += 25;
			PlayerPrefs.SetInt ("scratchme_coin", R.coin);
			PlayerPrefs.SetInt ("scratchme_rate", 1);
		}
		GameController.instance.scratchF = true;
	}

	public void OnBuySmallPackage()
	{
		ThirdManager.instance.BuySmallPackage ();
		ThirdManager.instance.PlayButtonSound ();
		GameController.instance.scratchF = true;
	}

	public void OnBuyMediumPackage()
	{
		ThirdManager.instance.BuyMediumPackage ();
		ThirdManager.instance.PlayButtonSound ();
		GameController.instance.scratchF = true;
	}

	public void OnBuyLargePackage()
	{
		ThirdManager.instance.BuyLargePackage ();
		ThirdManager.instance.PlayButtonSound ();
		GameController.instance.scratchF = true;
	}

	bool end;
	int giveletter;
	[HideInInspector]public GameObject[] blanks;

	static public GameUI instance;

	public UISlider scratchSlider;
	public UILabel coinLab;
	public UILabel worldLab;
	public UILabel levelLab;
	public GameObject clueLab;
	public GameObject gameEnd;
	public UILabel[] key;
	public GameObject blankPanel;
	public GameObject blank;
	public UI2DSprite[] starSprites;
	public Sprite okSprite;
	public Sprite noSprite;
	public UILabel scoreLab;
	public UILabel worldscoreLab;
	public UILabel clearLab;
	public GameObject earnpoints;
}
