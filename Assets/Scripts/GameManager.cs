using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public float speed = 200;

	public static GameManager instance;

	void OnEnable ()
	{
		GameManager.instance = this;
	}

	void Awake ()
	{
		DontDestroyOnLoad (this);
		Time.timeScale = 1;
		UI ();
	}

	public static void Die ()
	{
		Time.timeScale = 0;
		SceneManager.LoadScene ("GameOver", LoadSceneMode.Additive);
	}

	public static void Restart ()
	{
		SceneManager.LoadScene ("Run");
	}

	public static void UI ()
	{
		SceneManager.LoadScene ("UI", LoadSceneMode.Additive);
	}
}
