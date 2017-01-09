using UnityEngine;
using System.Collections;

public class Blank : MonoBehaviour {

	public void OnClickBlank()
	{
		ThirdManager.instance.PlayButtonSound ();
		if (no < R.result.Length) {
			if(no<R.result.Length-1)
				R.result = R.result.Substring (0, no) + " " + R.result.Substring (no + 1);
			else
				R.result = R.result.Substring (0, no);
			string ch = gameObject.transform.Find ("Label").gameObject.GetComponent<UILabel> ().text;
			for (int i = 0; i < 16; i++) {
				if (!GameUI.instance.key [i].gameObject.transform.parent.gameObject.activeSelf) {
					if (GameUI.instance.key [i].text.Equals (ch)) {
						GameUI.instance.key [i].gameObject.transform.parent.gameObject.SetActive (true);
						break;
					}
				}
			}
		}
	}

	void Update()
	{
		transform.Find ("Label").gameObject.GetComponent<UILabel> ().text = letter;
		transform.Find ("Label").gameObject.GetComponent<UILabel> ().width = letterSize;
		transform.Find ("Label").gameObject.GetComponent<UILabel> ().height = letterSize;
		transform.Find ("Label").gameObject.GetComponent<UILabel> ().fontSize = letterSize;
	}

	public string letter;
	public int letterSize = 100;
	[HideInInspector]public int no = 0;
}
