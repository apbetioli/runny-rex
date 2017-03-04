using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Storage : MonoBehaviour {

	public static int Highscore
	{
		get { return PlayerPrefs.GetInt ("highscore", 0); }
		set { PlayerPrefs.SetInt("highscore", value); }
	}

	public static int DeathByGroundObstacles
	{
		get { return PlayerPrefs.GetInt ("death-ground", 0); }
		set { PlayerPrefs.SetInt("death-ground", value); }
	}

	public static int DeathBySkyObstacles
	{
		get { return PlayerPrefs.GetInt ("death-sky", 0); }
		set { PlayerPrefs.SetInt("death-sky", value); }
	}

	public static int NumberOfPlayedTimes
	{
		get { return PlayerPrefs.GetInt ("played-times", 0); }
		set { PlayerPrefs.SetInt("played-times", value); }
	}
}
