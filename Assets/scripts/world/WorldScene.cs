using UnityEngine;
using System;
using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;
using System.IO;
using SimpleJSON;

public class WorldScene : MonoBehaviour {

	// Use this for initialization
	void Start () 
	{
		string worldsPath = System.IO.Path.Combine(Application.streamingAssetsPath, "worlds.json");
		if(R.worlds == null)
			StartCoroutine(GetWorlds (worldsPath));
		else
			StartCoroutine(InitializeWorlds ());
	}

	IEnumerator GetWorlds(string filePath)
	{
		string result = "";
		if (filePath.Contains ("://")) {
			WWW www = new WWW (filePath);
			yield return www;
			result = www.text;
		} else {
			result = System.IO.File.ReadAllText (filePath);
		}

		JSONNode jNode = JSON.Parse (result);
		int worldCount = jNode.Count;

		R.worlds = new Worlds[worldCount];
		for (int i = 0; i < R.worlds.Length; i++) {
			R.worlds [i] = new Worlds ();
			StartCoroutine (GetWorldContent(i));
		}

		StartCoroutine(InitializeWorlds ());
	}

	IEnumerator GetWorldContent(int id)
	{
		int reordered = PlayerPrefs.GetInt ("scratchme_reorder"+id, 0);
		string filePath = "";
		string result = "";
		if (reordered == 0) {
			filePath = System.IO.Path.Combine (Application.streamingAssetsPath, "world" + (id + 1) + ".json");
			if (filePath.Contains ("://")) {
				WWW www = new WWW (filePath);
				yield return www;
				result = www.text;
				Debug.Log (result);
			} else {
				result = System.IO.File.ReadAllText (filePath);
			}
		} else {
			result = System.IO.File.ReadAllText (Application.persistentDataPath+"/world"+(id+1)+".json");
		}


		JSONNode jNode = JSON.Parse (result);
		int levelCount = jNode.Count;

		R.worlds[id].images = new string[levelCount];
		R.worlds[id].answers = new string[levelCount];
		R.worlds[id].spaces = new int[levelCount];

		//reorder
		Worlds world = new Worlds ();
		world.images = new string[levelCount];
		world.answers = new string[levelCount];
		world.spaces = new int[levelCount];

		Debug.Log ("Unity: Reordered"+id+": "+reordered+" "+filePath);
		if (reordered == 0) {
			PlayerPrefs.SetInt ("scratchme_reorder" + id, 1);
			for (int i = 0; i < levelCount; i++) {
				world.images [i] = jNode [i] ["image"];
				world.answers [i] = jNode [i] ["answer"];
				world.spaces [i] = world.answers [i].IndexOf (" ");
				world.answers [i] = world.answers [i].Replace (" ", "");
			}

			int[] rnds = new int[levelCount];
			for (int i = 0; i < levelCount; i++)
				rnds [i] = i;
			System.Random rnd = new System.Random ();
			rnds = rnds.OrderBy (x => rnd.Next ()).ToArray ();

			string json = "[";
			for (int i = 0; i < levelCount; i++) {
				R.worlds [id].images [i] = world.images [rnds[i]];
				R.worlds [id].answers [i] = world.answers [rnds[i]];
				R.worlds [id].spaces [i] = world.spaces [rnds[i]];

				string answer = R.worlds [id].answers [i];
				if (R.worlds [id].spaces [i] != -1) {
					answer = answer.Insert (R.worlds [id].spaces [i], " ");
				}
				//make json string
				json += "{\"image\":\""+R.worlds[id].images[i]+"\", \"answer\":\""+answer+"\"},";
			}
			json = json.Substring (0, json.Length - 1)+"]";
			Debug.Log (json);
			File.WriteAllBytes(Application.persistentDataPath+"/world"+(id+1)+".json", Encoding.ASCII.GetBytes(json));
		} else {
			for (int i = 0; i < levelCount; i++) {
				R.worlds [id].images [i] = jNode [i] ["image"];
				R.worlds [id].answers [i] = jNode [i] ["answer"];
				R.worlds [id].spaces [i] = R.worlds[id].answers [i].IndexOf (" ");
				R.worlds [id].answers [i] = R.worlds[id].answers [i].Replace (" ", "");
			}
		}
	}

	IEnumerator InitializeWorlds()
	{
		int count = 0;
		while(count < R.worlds.Length)
		{
			yield return new WaitForSeconds(0.1f);
			for (int i = 0; i < R.worlds.Length; i++) {
				if (R.worlds [i] != null)
					count++;
			}
		}
		for (int i = 0; i < R.worlds.Length; i++) {
			if (i == 0) {
				R.worlds [i].opened = PlayerPrefs.GetInt ("ScratchMe_opened" + i, 1);
				R.worlds [i].successed = PlayerPrefs.GetInt ("ScratchMe_successed" + i, 0);
			} else {
				R.worlds [i].opened = PlayerPrefs.GetInt ("ScratchMe_opened" + i, 0);
				R.worlds [i].successed = PlayerPrefs.GetInt ("ScratchMe_successed" + i, 0);
			}

			GameObject worldrow = GameObject.Instantiate (worldrow_ori);
			worldrow.transform.Find ("big label").gameObject.GetComponent<UILabel> ().text = "" + (i + 1);
			if (R.worlds [i].opened == 0) {
				string tmp = worldrow.transform.Find ("small label").gameObject.GetComponent<UILabel> ().text;
				tmp = tmp.Replace ("*X*", (int)(R.worlds [i - 1].answers.Length * 0.8f) + "");
				worldrow.transform.Find ("small label").gameObject.GetComponent<UILabel> ().text = tmp;
				worldrow.transform.Find ("button").gameObject.SetActive (false);
			} else {
				worldrow.transform.Find ("small label").gameObject.GetComponent<UILabel> ().text = R.worlds [i].opened + "/" + R.worlds [i].answers.Length;
				worldrow.transform.Find ("success").gameObject.SetActive (true);
				worldrow.transform.Find ("success_value").gameObject.GetComponent<UILabel> ().text = ""+(int)((R.worlds[i].successed)*100/(R.worlds[i].answers.Length-1))+"%";
				worldrow.transform.Find ("lock").gameObject.SetActive (false);
			}
			worldrow.transform.parent = worldGrid.transform;
			worldrow.transform.localPosition = new Vector3 (0, 0, 0);
			worldrow.transform.localScale = new Vector3 (1, 1, 1);
		}
		worldGrid.Reposition ();
		worldGrid.repositionNow = true;
	}

	void Update()
	{
		if (Input.GetKeyUp (KeyCode.Escape)) {
			SceneManager.LoadScene ("start");
		}
	}

	public void OnBack()
	{
		ThirdManager.instance.PlayButtonSound ();
		SceneManager.LoadScene ("start");
	}

	public GameObject worldrow_ori;
	public UIGrid worldGrid;
}

public class Worlds{
	public string[] images;
	public string[] answers;
	public int[] spaces;
	public int opened;
	public int successed;
}

static class RandomStringArrayTool
{
	static System.Random _random = new System.Random();

	public static Worlds[] RandomizeStrings(Worlds[] arr)
	{
		List<KeyValuePair<int, Worlds>> list = new List<KeyValuePair<int, Worlds>>();
		// Add all strings from array
		// Add new random int each time
		foreach (Worlds s in arr)
		{
			list.Add(new KeyValuePair<int, Worlds>(_random.Next(), s));
		}
		// Sort the list by the random number
		var sorted = from item in list
			orderby item.Key
			select item;
		// Allocate new string array
		Worlds[] result = new Worlds[arr.Length];
		// Copy values to array
		int index = 0;
		foreach (KeyValuePair<int, Worlds> pair in sorted)
		{
			result[index] = pair.Value;
			index++;
		}
		// Return copied array
		return result;
	}
}
