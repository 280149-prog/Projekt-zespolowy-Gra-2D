using UnityEngine;
using UnityEngine.EventSystems;

public class EnemyMovement : MonoBehaviour
{
    public Rigidbody2D rb;

    [Header("Patrolling")]
    public float patrolSpeed = 3f;
    bool _isMovingRight = true;

    //[Header("Chasing")]
    //public float chaseSpeed = 5f;
    //bool _isChasing = false;


    void Update()
    {
        //if (rb.position.x >= 20)
        //{
        //    _isMovingRight = false;
        //}

        //if (rb.position.x <= 15)
        //{
        //    _isMovingRight = true;
        //}
    }

    private void FixedUpdate()
    {
        //if (_isMovingRight)
        //{
        //    rb.linearVelocity = new Vector2(patrolSpeed, rb.linearVelocity.y);
        //}
        //else
        //{
        //    rb.linearVelocity = new Vector2(-patrolSpeed, rb.linearVelocity.y);
        //}
    }
}
