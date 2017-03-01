using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public float speed = 200f;
	public int score = 0;
	public float acumTime = 0;
	public AudioSource highScoreSound;

	public static GameManager instance;

	void OnEnable ()
	{
		GameManager.instance = this;
		score = 0;
	}

	void Awake ()
	{
		DontDestroyOnLoad (this);
		UI ();
	}

	void Start() {
		Time.timeScale = 1;
		score = 0;
	}

	void Update() {
		acumTime += Time.deltaTime * Time.timeScale;
		score = Mathf.RoundToInt (acumTime * speed / 10) ;		
		if (GetHighscore () == score +1)
			highScoreSound.Play();
	}

	public void Die ()
	{
		Time.timeScale = 0;
		SceneManager.LoadScene ("GameOver", LoadSceneMode.Additive);
		if (score > GetHighscore ())
			SetHighscore (score);
	}

	public static void Restart ()
	{
		SceneManager.LoadScene ("Run");
	}

	public static void UI ()
	{
		SceneManager.LoadScene ("UI", LoadSceneMode.Additive);
	}

	public static int GetHighscore() {
		return PlayerPrefs.GetInt("highscore", 0);
	}

	public static void SetHighscore(int highscore) {
		PlayerPrefs.SetInt("highscore", highscore);
	}

}
