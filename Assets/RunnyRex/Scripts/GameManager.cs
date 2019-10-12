using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
	private static bool playing = false;

    public AudioSource highScoreSound;
    [HideInInspector]
    public int level = 0;
    public float speed = 100f;
    public float acceleration = 0.001f;
    public float timeScale = 1;

	private int score = 0;
    private float acumTime = 0;
    private bool playedSoundHiScore = false;
	private Leaderboard leaderboard;
	private int highscore = 0;
	private GameSet[] gameSets;

	public bool randomGameSet = true;
	public GameSet selectedGameSet;

	/*
	 * Used for keeping the same selected level the first time that enters the game, otherwise it becomes weird.
	 */
	private static int timesPlayedThisSession = 0;

	public static bool Playing
	{
		get { return playing; }
		private set { playing = value; }
	}

	public int Score
	{
		get { return score; }
	}

	public int Highscore
	{
		get { return highscore; }
	}

    void OnEnable()
    {
        GameManager.Instance = this;
    }

    void Awake()
    {
		leaderboard = GetComponent<Leaderboard> ();
		gameSets = Resources.FindObjectsOfTypeAll<GameSet> ();
		foreach (GameSet gameSet in gameSets)
			gameSet.Unselect ();

        UI();
    }

    void Start()
    {	
		timesPlayedThisSession++;

		SelectGameSet ();

		if (!Playing){
            SceneManager.LoadScene("Start", LoadSceneMode.Additive);
            return;
        }

        Time.timeScale = 1;
        acumTime = level * 100;
        score = (int)acumTime;
		highscore = Storage.Highscore;

		Storage.NumberOfPlayedTimes = Storage.NumberOfPlayedTimes + 1;
    }

    void Update()
    {
		if (!Playing)
            return;

		if (highscore == score + 1 && !playedSoundHiScore)
        {
            highScoreSound.Play();
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

	public void Die(GameObject other)
    {
        Time.timeScale = 0;

        SceneManager.LoadScene("Start", LoadSceneMode.Additive);

		if (score > highscore) {
			Storage.Highscore = score;
			leaderboard.ReportHighScore ();
		}

		if (other.transform.position.y == 0) {
			Storage.DeathByGroundObstacles = Storage.DeathByGroundObstacles + 1;
		} else {
			Storage.DeathBySkyObstacles = Storage.DeathBySkyObstacles + 1;
		}

	}

    public static void Restart()
    {
        SceneManager.LoadScene("Run");
		Playing = true;
    }

    public static void UI()
    {
        SceneManager.LoadScene("UI", LoadSceneMode.Additive);
    }

	public void ShowLeaderboard() 
	{
		leaderboard.ShowLeaderboard ();
	}

	public void ShowAchievements()
	{
		leaderboard.ShowAchievements ();
	}

	private void SelectGameSet() {
		if (selectedGameSet == null)
			selectedGameSet = gameSets [0];
			
		if (!randomGameSet || timesPlayedThisSession <= 2) {
			selectedGameSet.Select ();
			return;
		}
		
		selectedGameSet.Unselect ();
		
		int index = Random.Range (0, gameSets.Length);
		selectedGameSet = gameSets [index];
		selectedGameSet.Select ();
		Debug.Log ("Randomly selected game set: " + selectedGameSet.name);
	}
}
