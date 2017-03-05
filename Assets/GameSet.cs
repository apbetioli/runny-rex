using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSet : MonoBehaviour {

	public Color backgroundColor = Color.black;

	public void Select() {
		gameObject.SetActive (true);
		FindObjectOfType<Camera> ().backgroundColor = backgroundColor;
	}

	public void Unselect() {
		gameObject.SetActive (false);
	}

}
