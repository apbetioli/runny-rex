using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class Character : MonoBehaviour
{
    public float maxJumpHeight = 40;
    public float minJumpHeight = 10;
    public float timeToJumpApex = .4f;

    private Rigidbody2D body;
    private bool onTheGround = false;
    private bool duck = false;
	private bool dead = false;
    private float gravity;
    private float maxJumpVelocity;
    private float minJumpVelocity;

    void Awake()
    {
        body = GetComponent<Rigidbody2D>();
    }

    void Start()
    {
        CalculateGravity();
        AdjustJump();
    }

    void Update()
    {

        duck = false;
		if (IsJumpPressed())
            Jump();
		else if (IsJumpReleased())
            ReleaseJump();
		else if (IsDuckPressed())
            Duck();

        GetComponentInChildren<Animator>().SetBool("Ground", onTheGround);
        GetComponentInChildren<Animator>().SetBool("Duck", duck);
		GetComponentInChildren<Animator>().SetBool("Dead", dead);	

        CalculateGravity();
        AdjustJump();
    }

	private bool IsJumpPressed() {
		return Input.GetButton ("Jump") || Input.GetKey (KeyCode.UpArrow);
	}

	private bool IsJumpReleased() {
		return Input.GetButtonUp ("Jump") || Input.GetKeyUp (KeyCode.UpArrow);
	}

	private bool IsDuckPressed() {
		return Input.GetKey (KeyCode.DownArrow);
	}

    private void CalculateGravity()
    {
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);

        float velocityY = body.velocity.y + gravity * Time.deltaTime;
        SetVelocityY(velocityY);
    }

    private void AdjustJump()
    {
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);
    }

    private void SetVelocityY(float velocityY)
    {
        Vector2 velocity = body.velocity;
        velocity.y = velocityY;
        body.velocity = velocity;
    }

    public void Jump()
    {
        if (!onTheGround)
            return;

        SetVelocityY(maxJumpVelocity);
        onTheGround = false;
    }

    public void ReleaseJump()
    {
        if (body.velocity.y > minJumpVelocity)
        {
            SetVelocityY(minJumpVelocity);
        }
    }

    public void Duck()
    {
        if (!onTheGround)
            return;
        duck = true;
        //TODO
    }

	void OnCollisionEnter2D (Collision2D other)
	{
		if ("Ground" == other.gameObject.tag) {
			SetVelocityY (0);
			onTheGround = true;
		} else if ("Enemy" == other.gameObject.tag) {
			GameManager.instance.Die ();
			dead = true;
		}
	}


}
