using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIController : MonoBehaviour
{
	private Character character;

	void Awake ()
	{
		//DontDestroyOnLoad (gameObject);
		//SceneManager.LoadScene ("Run", LoadSceneMode.Additive);
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
