using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent (typeof(Collider2D))]
public class Character : MonoBehaviour
{
	public float jumpHeight = 20;
	public float timeToJumpApex = .4f;

	private Rigidbody2D body;
	private bool onTheGround = false;
	private float gravity;
	private float jumpVelocity;

	void Awake ()
	{
		body = GetComponent<Rigidbody2D> ();
	}

	void Start ()
	{
		CalculateGravity ();
		AdjustJump ();
	}

	void Update ()
	{
		if (Input.GetButtonDown ("Jump") || Input.GetKeyDown (KeyCode.UpArrow))
			Jump ();
		else if (Input.GetKeyDown (KeyCode.DownArrow))
			Duck ();

		CalculateGravity ();
		AdjustJump ();
	}

	private void CalculateGravity ()
	{
		gravity = -(2 * jumpHeight) / Mathf.Pow (timeToJumpApex, 2);

		float velocityY = body.velocity.y + gravity * Time.deltaTime;
		SetVelocityY (velocityY);
	}

	private void AdjustJump ()
	{
		jumpVelocity = Mathf.Abs (gravity) * timeToJumpApex;
	}

	private void SetVelocityY (float velocityY)
	{
		Vector2 velocity = body.velocity;
		velocity.y = velocityY;
		body.velocity = velocity;
	}

	public void Jump ()
	{
		if (!onTheGround)
			return;
		
		SetVelocityY (jumpVelocity);
		onTheGround = false;
	}

	public void Duck ()
	{
		if (!onTheGround)
			return;

		//TODO
	}

	void OnCollisionEnter2D (Collision2D other)
	{
		if ("Ground" == other.gameObject.tag) {
			SetVelocityY (0);
			onTheGround = true;
		}
	}

}
