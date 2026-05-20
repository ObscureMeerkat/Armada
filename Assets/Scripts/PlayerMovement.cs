using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovement : MonoBehaviour
{
    public float accelerationForce = 10f;
    public float maxSpeed = 5f;
    public float rotationSpeed = 150f;
    public float linearDamping = 2f;

    private Rigidbody2D rb;
    private PlayerStats playerStats;
    private Vector2 moveInput;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.linearDamping = linearDamping;
        playerStats = GetComponent<PlayerStats>();
    }

    void FixedUpdate()
    {
        ApplyThrust();
        ApplyRotation();
    }

    void ApplyThrust()
    {
        float thrust = moveInput.y;
        Vector2 forwardDirection = transform.up;
        rb.AddForce(forwardDirection * thrust * accelerationForce);

        float currentMaxSpeed = playerStats != null ? 
                                playerStats.GetMoveSpeed() : maxSpeed;
        if (rb.linearVelocity.magnitude > currentMaxSpeed)
        {
            rb.linearVelocity = rb.linearVelocity.normalized * currentMaxSpeed;
        }
    }

    void ApplyRotation()
    {
        float rotation = moveInput.x;
        float currentRotationSpeed = playerStats != null ? 
                                     playerStats.GetRotationSpeed() : rotationSpeed;
        rb.MoveRotation(rb.rotation - rotation * currentRotationSpeed * Time.fixedDeltaTime);
    }

    void OnMove(InputValue value)
    {
        moveInput = value.Get<Vector2>();
    }
}