using UnityEngine;

public class AstroSpecialist : MonoBehaviour
{
    public Transform player;

    public float detectionRadius = 8f;
    public float orbitSpeed = 50f;
    public float orbitDistance = 3f;

    public float dashForce = 20f;
    public float dashDuration = 1.5f;
    public float dashCooldown = 2f;

    bool isDashing = false;
    bool canDash = true;

    Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if (!PlayerInRange())
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        if (!isDashing)
        {
            OrbitAroundPlayer();
        }

        if (canDash && !isDashing)
        {
            StartDash();
        }
    }

    bool PlayerInRange()
    {
        return Vector2.Distance(transform.position, player.position) <= detectionRadius;
    }

    void OrbitAroundPlayer()
    {
        Vector2 dir = (transform.position - player.position).normalized;

        Vector2 desiredPos = (Vector2)player.position + dir * orbitDistance;

        transform.position = Vector2.MoveTowards(transform.position, desiredPos, Time.deltaTime * 6f);

        transform.RotateAround(player.position, Vector3.forward, orbitSpeed * Time.deltaTime);
    }

    void StartDash()
    {
        canDash = false;
        isDashing = true;

        Vector2 direction = (player.position - transform.position).normalized;
        rb.linearVelocity = direction * dashForce;

        Invoke("EndDash", dashDuration);
    }

    void EndDash()
    {
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        Invoke("ResetCooldown", dashCooldown);
    }

    void ResetCooldown()
    {
        canDash = true;
    }
}