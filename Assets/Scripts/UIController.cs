using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIController : MonoBehaviour
{
	private Player player;

	void Awake ()
	{
		player = FindObjectOfType<Player> ();
	}

	public void Duck ()
	{
		player.Duck ();
	}

	public void Jump ()
	{
		player.Jump ();
	}
}
