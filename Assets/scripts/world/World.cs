using UnityEngine;
using System.Collections;
using System;
using UnityEngine.SceneManagement;

public class World : MonoBehaviour {

	public void OnClick()
	{
		ThirdManager.instance.PlayButtonSound ();
		string worldtxt = gameObject.transform.Find ("big label").GetComponent<UILabel> ().text;
		int worldnum = Int32.Parse (worldtxt);
		if (R.worlds [worldnum - 1].opened != 0)
		{
			R.world_selected = worldnum - 1;
			print ("Selected World: " + worldnum);
			SceneManager.LoadScene ("game");
		}
	}

}
