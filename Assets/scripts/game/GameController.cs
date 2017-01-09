using System;
using System.Threading;
using System.Collections;
using UnityEngine;
using System.IO;

public class GameController : MonoBehaviour {

	void Awake()
	{
		instance = this;
	}

	// Use this for initialization
	void Start ()
	{
		//initialize
		R.result = "";
		tmpx = tmpy = 0;
		scratchF = true;

		//make game scene
		Texture2D newTex = new Texture2D (512, 444);
		scratch = Sprite.Create(newTex, new Rect(0f, 0f, 512, 444), new Vector2(0.5f, 0.5f));
		scratchPanel.sprite = scratch;

		//decide level of selected world
		R.level = R.worlds[R.world_selected].opened-1;
		image = R.worlds [R.world_selected].images [R.level];
		answer = R.worlds [R.world_selected].answers [R.level];
		Sprite spr = Resources.Load<Sprite> ("world/"+image.Substring(0, image.Length-4));
		float rateX = scratchPanel.sprite.bounds.size.x / spr.bounds.size.x;
		float rateY = scratchPanel.sprite.bounds.size.y / spr.bounds.size.y;
		targetSprite.sprite = spr;
		targetSprite.gameObject.transform.localScale = new Vector3 (rateX*scratchPanel.gameObject.transform.localScale.x-0.01f, rateY*scratchPanel.gameObject.transform.localScale.y-0.01f, 1);

		int startX = (int)scratch.textureRect.x;
		int startY = (int)scratch.textureRect.y;
		width = (int)scratch.textureRect.width;
		height = (int)scratch.textureRect.height;

		//get intermediate results
		pixels = scratch.texture.GetPixels (startX, startY, width, height);
//		var n_pixels = PlayerPrefsX.GetIntArray("scratches"+R.world_selected, 1, 227328);
		if (File.Exists (Application.persistentDataPath + "/save" + R.world_selected)) {
			byte[] mbyte = File.ReadAllBytes (Application.persistentDataPath + "/save" + R.world_selected);
			for (int i = 0; i < pixels.Length; i++) {
				if (mbyte [i] == 1)
					pixels [i] = new Color32 (194, 194, 194, 255);
				else
					pixels [i] = new Color32 (194, 194, 194, 0);
			}
		} else {
			for (int i = 0; i < pixels.Length; i++) {
				pixels [i] = new Color32 (194, 194, 194, 255);
			}
		}
		scratchPanel.sprite.texture.SetPixels (pixels);
		scratchPanel.sprite.texture.Apply (true, false);

		Vector3 pos = scratchPanel.transform.position;

		wHei = 2 * Camera.main.orthographicSize;
		wWit = wHei * Screen.width / Screen.height;

		sWid = (int)(4.5f * Screen.width / wWit);
		sHei = (int)(3.3f * Screen.height / wHei);
		sx = (int)((wWit / 2 - 2.3f) * Screen.width / wWit);
		sy = (int)((wHei / 2 + 3.02f) * Screen.height / wHei);

		//assign key and blank box
		for (int i = 0; i < GameUI.instance.key.Length; i++)
			GameUI.instance.key [i].text = "";
		for (int i = 0; i < answer.Length; i++) {
			bool seted = false;
			while (!seted) {
				int rnd = UnityEngine.Random.Range (0, GameUI.instance.key.Length);
				if (GameUI.instance.key [rnd].text == "") {
					GameUI.instance.key[rnd].text = answer.Substring (i, 1).ToUpper();
					seted = true;
				}
			}
		}
		for (int i = 0; i < GameUI.instance.key.Length; i++) {
			int rnd = UnityEngine.Random.Range (0, 26);
			if(GameUI.instance.key[i].text == "")
				GameUI.instance.key [i].text = sampleKey [rnd];
		}

		float blankW = 1000 / answer.Length;
		float gapW = blankW;
		if (blankW > 250)
			blankW = 250;
		GameUI.instance.blanks = new GameObject[answer.Length];
		for (int i = 0; i < answer.Length; i++) {
			GameUI.instance.blanks[i] = GameObject.Instantiate (GameUI.instance.blank);
			GameUI.instance.blanks[i].GetComponent<Blank> ().no = i;
			GameUI.instance.blanks[i].GetComponent<Blank> ().letterSize = (int)(blankW*0.8f);
			GameUI.instance.blanks[i].transform.parent = GameUI.instance.blankPanel.transform;
			if(R.worlds [R.world_selected].spaces[R.level] < 0)
				GameUI.instance.blanks[i].transform.localPosition = new Vector3 (-495+i*gapW+gapW/2, -360 ,0);
			else
			{
				if(i>=R.worlds [R.world_selected].spaces [R.level])
				{
					GameUI.instance.blanks[i].transform.localPosition = new Vector3 (-495+i*gapW+gapW/2+10, -360 ,0);
				}
				else
				{
					GameUI.instance.blanks[i].transform.localPosition = new Vector3 (-495+i*gapW+gapW/2, -360 ,0);
				}
			}
			GameUI.instance.blanks[i].transform.localScale = new Vector3 (1, 1, 1);
			GameUI.instance.blanks[i].GetComponent<UI2DSprite> ().width = (int)blankW;
			GameUI.instance.blanks[i].GetComponent<UI2DSprite> ().height = (int)blankW;
		}

		save = false;
		R.gamestatus = "none";
		StartCoroutine (RealTimeSave());
		_thread = new Thread(ThreadedWork);
		_thread.Start();
	}

	IEnumerator RealTimeSave()
	{
		while (R.gamestatus == "none") {
			yield return new WaitForSeconds (1.0f);
			if (save) {
				save = false;
//				PlayerPrefsX.SetIntArray ("scratches" + R.world_selected, m_pixels);
				PlayerPrefs.SetFloat ("scratchme_scratching" + R.world_selected, R.scratching);
				File.WriteAllBytes (Application.persistentDataPath+"/save"+R.world_selected, m_pixels);
			}
		}
	}

	bool save;
	Thread _thread;
	void ThreadedWork()
	{
		while (true) {
			Thread.Sleep (1000);
			SaveStatus ();
		}
	}

	byte[] m_pixels;
	public void SaveStatus()
	{
		//save intermediate results
		m_pixels = new byte[227328];
		for (int i = 0; i < 227328; i++)
			if (pixels [i].a != 0)
				m_pixels [i] = 1;
			else
				m_pixels [i] = 0;
		save = true;
	}

	public void EraseStatus()
	{
		//save intermediate results
		byte[] n_pixels = new byte[227328];
		for (int i = 0; i < 227328; i++)
			n_pixels [i] = 1;	

//		PlayerPrefsX.SetIntArray ("scratches" + R.world_selected, n_pixels);
		PlayerPrefs.SetFloat ("scratchme_scratching" + R.world_selected, 1.0f);
		File.Delete(Application.persistentDataPath+"/save"+R.world_selected);
		print (Application.persistentDataPath + "/save" + R.world_selected);
	}

	void OnApplicationQuit()
	{
		SaveStatus ();
	}

	void OnApplicationPause()
	{
		SaveStatus ();
	}

	public bool scratchF;
	// Update is called once per frame
	void Update ()
	{
		if (Input.GetMouseButton (0) && scratchF)
		{
			float x = Input.mousePosition.x;
			float y = Input.mousePosition.y;
			if (x > sx && x < sx + sWid && y < sy && y > sy - sHei)
			{
				int tx = (int)((x - sx)*width/sWid);
				int ty = (int)((y-(sy-sHei))*height/sHei);

				if (pixels [ty * width + tx].a != 0 && pixels [ty * width + tx].r != 0 && pixels [ty * width + tx].g != 0 && pixels [ty * width + tx].b != 0) {
					if (tmpx == 0 && tmpy == 0) {
						R.scratching -= scratching_rate;
					} else {
						R.scratching -= scratching_rate * Vector2.Distance (new Vector2 (tmpx, tmpy), new Vector2 (tx, ty)) / 10;
					}

					if (R.scratching > 0) {
						if (tmpx != 0 && tmpy != 0) {
							float dx = ((float)tx - (float)tmpx) / 30;
							float dy = ((float)ty - (float)tmpy) / 30;
							for (int i = 0; i < 30; i++) {
								tx = (int)(tmpx + dx * i);
								ty = (int)(tmpy + dy * i);
								for (int r = 3; r >= 0; r--) {
									int alpha = 0;
									while (alpha <= 360) {
										try {
											int xx = Mathf.CeilToInt(tx + r * Mathf.Cos (alpha * Mathf.Deg2Rad));
											int yy = Mathf.CeilToInt(ty + r * Mathf.Sin (alpha * Mathf.Deg2Rad));
											pixels [yy * width + xx] = new Color32 (0, 0, 0, 0);
											alpha += 10;
										} catch (Exception e) {
											alpha += 10;
											continue;
										}
									}
								}
							}
						}
						scratchPanel.sprite.texture.SetPixels (pixels);
						scratchPanel.sprite.texture.Apply ();
					}
				} else {
					
				}
				tmpx = tx;
				tmpy = ty;
			}
		}
		if (Input.GetMouseButtonUp (0)) {
			tmpx = tmpy = 0;
		}
		if (Input.GetKeyUp (KeyCode.Escape)) {
			SaveStatus ();
		}

		CheckGameEnd ();
	}

	void CheckGameEnd()
	{
		if (R.result.Length == answer.Length) {
			if (R.result.ToLower().Equals (answer.ToLower())) {
				R.gamestatus = "success";
			}
		}
	}

	static public GameController instance;

	public SpriteRenderer scratchPanel;
	public SpriteRenderer targetSprite;
	public SpriteRenderer gamePanel;
	public float scratching_rate;

	[HideInInspector]public Sprite scratch;

	Color[] pixels;
	int width, height;		//scratch width/height
	int sx, sy, sWid, sHei;		//start point, width and height of scratch on screen
	float wWit, wHei;
	int tmpx, tmpy;
	string image;
	string answer;
	string[] sampleKey = { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z" };
}
