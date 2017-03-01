using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	public Text score;
	public Text highScore;

	private Character character;
	private bool duck;

	void Awake ()
	{
		character = FindObjectOfType<Character> ();
		duck = false;
	}

	public void Update ()
	{
		score.text = GameManager.instance.score.ToString ();
		highScore.text = "HI " + GameManager.GetHighscore ();
		if (duck)
			character.Duck ();
	}

	public void Duck ()
	{
		duck = true;
	}

	public void DuckUp ()
	{
		duck = false;
	}

	public void Jump ()
	{
		character.Jump ();
	}

	public void JumpUp ()
	{
		character.ReleaseJump ();
	}
}

