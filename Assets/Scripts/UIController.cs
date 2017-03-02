﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	public Text score;
	public Text highScore;

	private Character character;
	private bool duck;
	private bool jump;

	void Awake ()
	{
		character = FindObjectOfType<Character> ();
		duck = false;
		jump = false;
	}

	public void Update ()
	{
		score.text = GameManager.Instance.score.ToString ();
		highScore.text = "HI " + GameManager.GetHighscore ();
		if (duck)
			character.Duck ();
		if (jump)
			character.Jump ();
	}

	public void Duck ()
	{
		duck = true;
	}

	public void DuckUp ()
	{
		duck = false;
		character.ReleaseDuck ();
	}

	public void Jump ()
	{
		jump = true;
	}

	public void JumpUp ()
	{
		jump = false;
		character.ReleaseJump ();
	}
}

