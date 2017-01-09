using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class StartScene : MonoBehaviour {

	// Use this for initialization
	void Start ()
	{
		R.coin = PlayerPrefs.GetInt ("scratchme_coin", 100);
		R.mute = PlayerPrefs.GetInt("scratchme_mute", 0);
		if (R.mute==1) {
			muteBtn.sprite2D = mute;
			ThirdManager.instance.gameObject.GetComponent<AudioSource> ().mute = true;
		} else {
			muteBtn.sprite2D = unmute;
			ThirdManager.instance.gameObject.GetComponent<AudioSource> ().mute = false;
		}
	}
	
	// Update is called once per frame
	void Update ()
	{
		earnedLab.text = R.coin + "";
		if (Input.GetKeyUp (KeyCode.Escape)) {
			if (earnpoints.activeSelf)
				earnpoints.SetActive (false);
			else if (howtoplay.activeSelf)
				howtoplay.SetActive (false);
			else
				Application.Quit ();
		}
	}

	public void OnSound()
	{
		R.mute = R.mute==0?1:0;
		PlayerPrefs.SetInt("scratchme_mute", R.mute);
		if (R.mute==1) {
			muteBtn.sprite2D = mute;
			ThirdManager.instance.gameObject.GetComponent<AudioSource> ().mute = true;
		} else {
			muteBtn.sprite2D = unmute;
			ThirdManager.instance.gameObject.GetComponent<AudioSource> ().mute = false;
		}
		ThirdManager.instance.PlayButtonSound ();
	}

	public void OnHowToPlay()
	{
		ThirdManager.instance.PlayButtonSound ();
		howtoplay.SetActive (true);
	}

	public void OnEarnPoints()
	{
		ThirdManager.instance.PlayButtonSound ();
		earnpoints.SetActive (true);
	}

	public void OnPlay()
	{
		ThirdManager.instance.PlayButtonSound ();
		SceneManager.LoadScene ("world");
	}

	public void OnClose()
	{
		ThirdManager.instance.PlayButtonSound ();
		if (howtoplay.activeSelf)
			howtoplay.SetActive (false);
		if (earnpoints.activeSelf)
			earnpoints.SetActive (false);
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
	}

	public void OnFBShare()
	{
		ThirdManager.instance.PlayButtonSound ();
		ThirdManager.instance.OnFBShare ();
		earnpoints.SetActive (false);
	}

	public void OnBuySmallPackage()
	{
		ThirdManager.instance.BuySmallPackage ();
		ThirdManager.instance.PlayButtonSound ();
	}

	public void OnBuyMediumPackage()
	{
		ThirdManager.instance.BuyMediumPackage ();
		ThirdManager.instance.PlayButtonSound ();
	}

	public void OnBuyLargePackage()
	{
		ThirdManager.instance.BuyLargePackage ();
		ThirdManager.instance.PlayButtonSound ();
	}

	public GameObject howtoplay;
	public GameObject earnpoints;
	public UILabel earnedLab;
	public Sprite mute;
	public Sprite unmute;
	public UI2DSprite muteBtn;
	public GameObject shareBtn;
	public GameObject rateBtn;
}
