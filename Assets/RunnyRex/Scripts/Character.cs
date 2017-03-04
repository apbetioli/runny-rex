using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Rigidbody2D))]

public class Character : MonoBehaviour
{
	public float maxJumpHeight = 40;
	public float minJumpHeight = 10;
	public float timeToJumpApex = .4f;
	public AudioSource jumpSound;
	public AudioSource duckSound;
	public AudioSource deadSound;

	public GameObject standingColliders;
	public GameObject duckingColliders;

	private Rigidbody2D body;
	private bool onTheGround = false;
	private bool duck = false;
	private bool dead = false;
	private float gravity;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private Animator animator;

	void Awake ()
	{
		body = GetComponent<Rigidbody2D> ();
		animator = GetComponentInChildren<Animator> ();
		dead = false;
		animator.SetBool ("Playing", GameManager.Playing);
	}

	void Start ()
	{
		dead = false;
		CalculateGravity ();
		AdjustJump ();
	}

	void Update ()
	{
		animator.SetBool ("Duck", duck);
		animator.SetBool ("Ground", onTheGround);
		animator.SetBool ("Dead", dead);

		standingColliders.SetActive(!duck);
		duckingColliders.SetActive (duck);
		 
		if (dead) {
			SetVelocityY (0);
			return;
		}

		if (IsJumpPressed ())
			Jump ();
		else if (IsJumpReleased ())
			ReleaseJump ();
		else if (IsDuckPressed ())
			Duck ();
		else if (IsDuckReleased ())
			ReleaseDuck ();
		
		CalculateGravity ();
		AdjustJump ();
	}

	private bool IsJumpPressed ()
	{
		return Input.GetButton ("Jump") || Input.GetKey (KeyCode.UpArrow);
	}

	private bool IsJumpReleased ()
	{
		return Input.GetButtonUp ("Jump") || Input.GetKeyUp (KeyCode.UpArrow);
	}

	private bool IsDuckPressed ()
	{
		return Input.GetKey (KeyCode.DownArrow);
	}

	private bool IsDuckReleased()
	{
		return Input.GetKeyUp (KeyCode.DownArrow);
	}

	private void CalculateGravity ()
	{
		gravity = -(2 * maxJumpHeight) / Mathf.Pow (timeToJumpApex, 2);

		float velocityY = body.velocity.y + gravity * Time.deltaTime;
		SetVelocityY (velocityY);
	}

	private void AdjustJump ()
	{
		maxJumpVelocity = Mathf.Abs (gravity) * timeToJumpApex;
		minJumpVelocity = Mathf.Sqrt (2 * Mathf.Abs (gravity) * minJumpHeight);
	}

	private void SetVelocityY (float velocityY)
	{
		body.velocity = new Vector2 (0, velocityY);
	}

	public void Jump ()
	{
		if (!onTheGround || duck)
			return;
		
		jumpSound.Play ();
		SetVelocityY (maxJumpVelocity);
		onTheGround = false;
	}

	public void ReleaseJump ()
	{
		if (body.velocity.y > minJumpVelocity)
			SetVelocityY (minJumpVelocity);
	}

	public void Duck ()
	{
		if (!onTheGround || duck)
			return;
		
		duckSound.Play ();
		duck = true;
	}

	public void ReleaseDuck ()
	{
		duck = false;
	}

	void OnCollisionEnter2D (Collision2D other)
	{
		if ("Ground" == other.gameObject.tag) {
			SetVelocityY (0);
			onTheGround = true;

		} else {
			GameManager.Instance.Die (other.gameObject);
			dead = true;
			deadSound.Play ();
		}
	}

}
