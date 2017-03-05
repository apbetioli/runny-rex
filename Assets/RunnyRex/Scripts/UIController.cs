using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIController : MonoBehaviour
{
	public Text score;
	public Text highScore;

	private Character[] character;
	private bool duck;
	private bool jump;

	void Awake ()
	{
		character = Resources.FindObjectsOfTypeAll<Character> ();
	}

	public void Update ()
	{
		score.text = GameManager.Instance.Score.ToString ();
		highScore.text = "HI " + GameManager.Instance.Highscore;
		if (duck)
			foreach (Character c in character) {
				if(c.isActiveAndEnabled)
					c.Duck ();
			}
		if (jump)
			foreach (Character c in character) {
				if(c.isActiveAndEnabled)
					c.Jump ();
			}
	}

	public void Duck ()
	{
		duck = true;
	}

	public void DuckUp ()
	{
		duck = false;
		foreach (Character c in character) {
			if(c.isActiveAndEnabled)
				c.ReleaseDuck ();
		}
	}

	public void Jump ()
	{
		jump = true;
	}

	public void JumpUp ()
	{
		jump = false;
		foreach (Character c in character) {
			if(c.isActiveAndEnabled)
				c.ReleaseJump ();
		}
	}
}

