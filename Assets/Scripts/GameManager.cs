using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
	public static GameManager Instance;

	public AudioSource highScoreSound;
	[HideInInspector]
	public int score = 0;
	public int level = 0;
	public float speed = 100f;
	public float acceleration = 0.001f;
	public float timeScale = 1;

	private float acumTime = 0;
	private bool playedSoundHiScore = false;

	void OnEnable ()
	{
		GameManager.Instance = this;
	}

	void Awake ()
	{
		UI ();
	}

	void Start ()
	{
		Time.timeScale = 1;
		acumTime = level * 100;
		score = (int)acumTime;
	}

	void Update ()
	{
		if (GetHighscore () == score + 1 && !playedSoundHiScore) {
			highScoreSound.Play ();
			playedSoundHiScore = true;
		}

		if (Time.timeScale == 0)
			return;
		
		acumTime += Time.deltaTime * Time.timeScale * 10;
		score = (int)acumTime;
		level = score / 100;
		Time.timeScale = 1 + score * acceleration;
		timeScale = Time.timeScale;
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

	public static int GetHighscore ()
	{
		return PlayerPrefs.GetInt ("highscore", 0);
	}

	public static void SetHighscore (int highscore)
	{
		PlayerPrefs.SetInt ("highscore", highscore);
	}

}
