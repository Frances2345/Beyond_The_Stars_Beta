using UnityEngine;
using System;

public class EnemyChase : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 75f;
    private float currentHealth;
    public int scoreValue = 100;

    public bool IsAlive => currentHealth > 0;
    public event Action OnDied;

    private GameObject jugador;
    private Rigidbody2D rb;

    [Header("Chase")]
    public float moveSpeed = 2f;
    public float initialWaitTime = 2f;
    public float visionRange = 20f;

    [Header("Dash Settings")]
    public float dashRange = 4f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1f;

    private bool isWaiting = true;
    private float initialTimer = 0f;

    private bool isDashing = false;
    private bool canDash = true;

    private Vector2 dashDirection;

    // ------------------------
    // CHARGING BOOLEAN
    // ------------------------
    public bool IsCharging = false;
    // ------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador == null)
        {
            Debug.LogWarning("No se encontró el jugador con tag Player");
            enabled = false;
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    void Update()
    {
        if (jugador == null || !IsAlive) return;

        if (isWaiting)
        {
            HandleInitialWait();
            return;
        }

        float dist = Vector2.Distance(transform.position, jugador.transform.position);

        if (dist <= dashRange && canDash && !isDashing)
        {
            Dash();
            return;
        }

        if (!isDashing)
        {
            ChasePlayer();
        }
    }

    private void HandleInitialWait()
    {
        initialTimer += Time.deltaTime;
        if (initialTimer >= initialWaitTime) isWaiting = false;
    }

    private void ChasePlayer()
    {
        float dist = Vector2.Distance(transform.position, jugador.transform.position);

        if (dist <= visionRange)
        {
            Vector3 direction = (jugador.transform.position - transform.position).normalized;
            rb.linearVelocity = direction * moveSpeed;
        }
        else
        {
            StopMovement();
        }
    }

    private void Dash()
    {
        StartCoroutine(DashCoroutine());
    }

    private System.Collections.IEnumerator DashCoroutine()
    {
        // ------------------------
        // AQUI INICIA EL DASH
        // prende el charging
        // ------------------------
        IsCharging = true;

        isDashing = true;
        canDash = false;

        dashDirection = (jugador.transform.position - transform.position).normalized;

        float timer = 0f;
        while (timer < dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            timer += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        // ------------------------
        // AQUI TERMINA EL DASH
        // apaga el charging
        // ------------------------
        IsCharging = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }

    public void Die()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        currentHealth = 0;
        OnDied?.Invoke();
        Destroy(gameObject);
    }
}
