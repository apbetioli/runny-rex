using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	public Text score;
	public Text highScore;

	private Character character;

	void Awake ()
	{
		character = FindObjectOfType<Character> ();
	}

	public void Update() {
		score.text = GameManager.instance.score.ToString();
		highScore.text = "HI " + GameManager.GetHighscore ();
	}

	public void Duck ()
	{
		character.Duck ();
	}

	public void Jump ()
	{
		character.Jump ();
	}
}
