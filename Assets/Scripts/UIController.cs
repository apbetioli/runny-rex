using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
	private Character character;

	void Awake ()
	{
		character = FindObjectOfType<Character> ();
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
