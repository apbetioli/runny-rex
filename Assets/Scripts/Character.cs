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
    private bool crouching = false;
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

        crouching = false;
        if (Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.UpArrow))
            Jump();
        else if (Input.GetButtonUp("Jump") || Input.GetKeyUp(KeyCode.UpArrow))
        {
            ReleaseJump();
        }
        else if (Input.GetKey(KeyCode.DownArrow))
        {
            Duck();
        }

        GetComponentInChildren<Animator>().SetBool("Ground", onTheGround);
        GetComponentInChildren<Animator>().SetBool("Duck", crouching);
        CalculateGravity();
        AdjustJump();
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
        crouching = true;
        //TODO
    }

    void OnCollisionEnter2D(Collision2D other)
    {
        if ("Ground" == other.gameObject.tag)
        {
            SetVelocityY(0);
            onTheGround = true;
        }
    }

}
