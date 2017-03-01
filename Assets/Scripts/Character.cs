using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent (typeof(Rigidbody2D))]
[RequireComponent (typeof(Collider2D))]
public class Character : MonoBehaviour
{
	public float maxJumpHeight = 40;
	public float minJumpHeight = 10;
	public float timeToJumpApex = .4f;
	public AudioSource jumpSound;
	public AudioSource duckSound;
	public AudioSource deadSound;

	private Rigidbody2D body;
	private bool onTheGround = false;
	private bool duck = false;
	private bool dead = false;
	private float gravity;
	private float maxJumpVelocity;
	private float minJumpVelocity;
	private Animator animator;
	private Collider2DDTO colliderBody;
	private Collider2DDTO colliderHead;
	private BoxCollider2D[] colliders;

	void Awake ()
	{
		body = GetComponent<Rigidbody2D> ();
		animator = GetComponentInChildren<Animator> ();
		colliders = GetComponents<BoxCollider2D> ();
		dead = false;
		createCollidersDTO ();
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
		RestoreCollidersPositions ();
		 
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
		else        
            duck = false;
        
		
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
		
 		jumpSound.Play();
		SetVelocityY (maxJumpVelocity);
		onTheGround = false;
	}

	public void ReleaseJump ()
	{
		if (body.velocity.y > minJumpVelocity) {
			SetVelocityY (minJumpVelocity);
		}
	}

	public void Duck ()
	{
		if (!onTheGround)
			return;
		if (!duck)
            duckSound.Play();

		duck = true;
		colliders [0].offset = colliderBody.OffsetDuck;
		colliders [0].size = colliderBody.SizeDuck;
		colliders [1].offset = colliderHead.OffsetDuck;
		colliders [1].size = colliderHead.SizeDuck;
	}

	void OnCollisionEnter2D (Collision2D other)
	{
		if ("Ground" == other.gameObject.tag) {
			SetVelocityY (0);
			onTheGround = true;

		} else if ("Enemy" == other.gameObject.tag) {
			GameManager.instance.Die ();
			dead = true;
			deadSound.Play();
		}
	}

	private void createCollidersDTO ()
	{
		colliderBody = new Collider2DDTO ();
		colliderBody.OffsetDefault = colliders [0].offset;
		colliderBody.SizeDefault = colliders [0].size;
		colliderBody.OffsetDuck = new Vector2 ((colliderBody.OffsetDefault.x + 2.910191f), (colliderBody.OffsetDefault.y - 1.8f));
		colliderBody.SizeDuck = new Vector2 ((colliderBody.SizeDefault.x + 3.36957f), (colliderBody.SizeDefault.y - 3f));

		colliderHead = new Collider2DDTO ();
		colliderHead.OffsetDefault = colliders [1].offset;
		colliderHead.SizeDefault = colliders [1].size;
		colliderHead.OffsetDuck = new Vector2 ((colliderHead.OffsetDefault.x - 0.5f), (colliderHead.OffsetDefault.y - 7.446012f));
		colliderHead.SizeDuck = new Vector2 ((colliderHead.SizeDefault.x + 1.51055f), (colliderHead.SizeDefault.y - 1.225291f));
	}

	private void RestoreCollidersPositions ()
	{
		colliders [0].offset = colliderBody.OffsetDefault;
		colliders [0].size = colliderBody.SizeDefault;
		colliders [1].offset = colliderHead.OffsetDefault;
		colliders [1].size = colliderHead.SizeDefault;
	}

	class Collider2DDTO
	{
		private Vector2 offsetDefault;
		private Vector2 sizeDefault;
		private Vector2 offsetDuck;
		private Vector2 sizeDuck;

		public Vector2 OffsetDefault {
			get { return offsetDefault; }
			set { offsetDefault = value; }
		}

		public Vector2 SizeDefault {
			get { return sizeDefault; }
			set { sizeDefault = value; }
		}

		public Vector2 OffsetDuck {
			get { return offsetDuck; }
			set { offsetDuck = value; }
		}

		public Vector2 SizeDuck {
			get { return sizeDuck; }
			set { sizeDuck = value; }
		}

	}
}
