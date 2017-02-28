using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {

	public float speed = 200;

	public static GameManager instance;

	void OnEnable() {
		GameManager.instance = this;
	}

	void Awake() {
		DontDestroyOnLoad (this);
	}

	public void Die() {
		Debug.Log ("Morreu");
		Time.timeScale = 0;
	}
}
