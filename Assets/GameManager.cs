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
		speed = 0;
	}
}
