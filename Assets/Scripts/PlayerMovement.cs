using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Movement")]
    public float moveSpeed = 10f;
    float horizontalMovement;

    [Header("Jumping")]
    public float jumpPower = 15f;
    public int maxJumps = 2;
    int _jumpsRemaining;

    [Header("Jump Buffer")]
    public float jumpBufferTime = 0.15f;
    float _jumpBufferCounter;

    [Header("Ground Check")]
    public Transform groundCheckPos;
    public Vector2 groundCheckSize = new Vector2(0.65f, 0.05f);
    public LayerMask groundLayer;

    [Header("Gravity")]
    public float baseGravity = 5;
    public float maxFallSpeed = 18f;
    public float fallSpeedMultiplier = 2f;

    void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontalMovement * moveSpeed, rb.linearVelocityY);

        Gravity();
    }

    void Update()
    {
        GroundCheck();

        if (_jumpBufferCounter > 0)
        {
            _jumpBufferCounter -= Time.deltaTime;
        }

        ExecuteJump();
    }

    public void Gravity()
    {
        if (rb.linearVelocity.y < 0)
        {
            rb.gravityScale = baseGravity * fallSpeedMultiplier;
            rb.linearVelocity = new Vector2(rb.linearVelocityX, Mathf.Max(rb.linearVelocityY, -maxFallSpeed));
        }
        else
        {
            rb.gravityScale = baseGravity;
        }
    }


    public void Move(InputAction.CallbackContext context)
    {
        horizontalMovement = context.ReadValue<Vector2>().x;
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            _jumpBufferCounter = jumpBufferTime;
        }
    }

    private void ExecuteJump()
    {
        if (_jumpBufferCounter > 0 && _jumpsRemaining > 0)
        {
            rb.linearVelocity = new Vector2(rb.linearVelocityX, jumpPower);
            _jumpsRemaining--;

            _jumpBufferCounter = 0f;
        }
    }

    private void GroundCheck()
    {
        bool _grounded = Physics2D.OverlapBox(groundCheckPos.position, groundCheckSize, 0, groundLayer);

        if (_grounded && rb.linearVelocityY < 0.1f)
        {
            _jumpsRemaining = maxJumps;
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.white;
        Gizmos.DrawWireCube(groundCheckPos.position, groundCheckSize);
    }
}
