using UnityEngine;
using System.Collections;

public class TouchKey : MonoBehaviour {

	public void OnKeyButton()
	{
		ThirdManager.instance.PlayButtonSound ();
		string key = gameObject.transform.FindChild ("Label").GetComponent<UILabel> ().text;
		print (key);

		bool ins = false;
		int space = R.result.IndexOf (" ");
		if (space >= 0) {
			R.result = R.result.Substring (0, space) + key + R.result.Substring (space + 1);
			gameObject.SetActive (false);
		} else if (R.result.Length < R.worlds [R.world_selected].answers [R.level].Length) {
			R.result += key;
			gameObject.SetActive (false);
		}
	}

	[HideInInspector]public string key;
}
