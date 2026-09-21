using UnityEngine;
using System;
using System.Collections;

public class AstroSpecialist : MonoBehaviour, IDamageable
{
    //------------------------
    public Animator AstroSpecialistAnimator;

    //----------------
    [SerializeField] private float maxHealth = 75f;
    private float currentHealth;
    public int scoreValue = 100;

    public bool IsAlive => currentHealth > 0;
    public event Action OnDied;

    private GameObject jugador;
    public Transform player;
    private Rigidbody2D rb;

    [Header("Chase & Orbit")]
    public float moveSpeed = 2f;
    public float initialWaitTime = 2f;
    public float visionRange = 20f;

    public float detectionRadius = 8f;
    public float orbitSpeed = 50f;
    public float orbitDistance = 3f;

    [Header("Dash Settings")]
    public float dashRange = 4f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1f;
    public float dashForce = 20f;

    [Header("Esquiva de Asteroides")]
    public float avoidanceRadius = 7f;
    public float avoidanceStrength = 10f;

    private bool isWaiting = true;
    private float initialTimer = 0f;

    private bool isDashing = false;
    private bool canDash = true;
    private bool isSoundActive = false;

    private Vector2 dashDirection;

    public bool IsCharging = false; // ya no se usa realmente

    // ------------------------
    void Start()
    {
        AstroSpecialistAnimator = GetComponent<Animator>();

        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (Player1.Instance != null)
        {
            player = Player1.Instance.transform;
            jugador = Player1.Instance.gameObject;
        }
        else
        {
            jugador = GameObject.FindGameObjectWithTag("Player");
            if (jugador != null) player = jugador.transform;

            if (jugador == null)
            {
                Debug.LogWarning("No se encontró el jugador con tag Player");
                enabled = false;
            }
        }
    }

    void Update()
    {
        if (player == null || !IsAlive) return;

        if (isWaiting)
        {
            HandleInitialWait();
            return;
        }

        float dist = Vector2.Distance(transform.position, player.position);

        if (dist <= visionRange && !isSoundActive)
        {
            isSoundActive = true;
            PlaySpecialSound();
        }
        else if (dist > visionRange && isSoundActive)
        {
            isSoundActive = false;
        }

        if (dist <= dashRange && canDash && !isDashing)
        {
            StartCoroutine(DashCoroutine());
            return;
        }

        if (!isDashing)
        {
            if (dist <= visionRange)
            {
                OrbitAroundPlayer();
            }
            else
            {
                StopMovement();
            }
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    public void Die()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.SpecialDeath != null)
        {
            Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.SpecialDeath, transform.position);
        }

        currentHealth = 0;
        OnDied?.Invoke();
        Destroy(gameObject);
    }

    // ----------------------------------------------------
    // ------------------- DASH ARREGLADO -----------------
    // ----------------------------------------------------
    private IEnumerator DashCoroutine()
    {
        isDashing = false;
        canDash = false;

        // ACTIVAR PREPARACION
        AstroSpecialistAnimator.SetBool("EstaPreparandose", true);
        IsCharging = true;

        // Pequeño delay opcional
        yield return new WaitForSeconds(0.3f);

        // ACTIVAR EMBISTE
        AstroSpecialistAnimator.SetBool("EstaPreparandose", false);
        AstroSpecialistAnimator.SetBool("Estaembistiendo", true);

        isDashing = true;

        dashDirection = (player.position - transform.position).normalized;

        float timer = 0f;
        while (timer < dashDuration)
        {
            rb.linearVelocity = dashDirection * dashSpeed;
            timer += Time.deltaTime;
            yield return null;
        }

        // FIN DEL DASH
        rb.linearVelocity = Vector2.zero;
        isDashing = false;

        AstroSpecialistAnimator.SetBool("Estaembistiendo", false);
        IsCharging = false;

        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    private void PlaySpecialSound()
    {
        if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.SpecialSound != null)
        {
            Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.SpecialSound, transform.position);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            IDamageable playerDamageable = collision.gameObject.GetComponent<IDamageable>();
            if (playerDamageable != null)
            {
                playerDamageable.TakeDamage(70f);
            }
            TakeDamage(20f);
        }
    }

    private void HandleInitialWait()
    {
        initialTimer += Time.deltaTime;
        if (initialTimer >= initialWaitTime) isWaiting = false;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, visionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, avoidanceRadius);
    }
    private void OrbitAroundPlayer()
    {
        Vector2 dir = (transform.position - player.position).normalized;
        Vector2 desiredPos = (Vector2)player.position + dir * orbitDistance;

        transform.position = Vector2.MoveTowards(transform.position, desiredPos, Time.deltaTime * 6f);

        transform.RotateAround(player.position, Vector3.forward, orbitSpeed * Time.deltaTime);

        // Esquiva de asteroides: empuja al specialist fuera de asteroides cercanos
        Vector2 avoid = ObstacleAvoidanceOffset();
        transform.position += (Vector3)avoid * Time.deltaTime;
    }

    // Repulsión reactiva desde los asteroides cercanos
    private Vector2 ObstacleAvoidanceOffset()
    {
        Vector2 offset = Vector2.zero;
        int count = 0;

        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, avoidanceRadius);
        foreach (Collider2D hit in hits)
        {
            if (hit == null || !hit.CompareTag("Asteroid")) continue;

            Vector2 away = (Vector2)transform.position - (Vector2)hit.transform.position;
            if (away.sqrMagnitude < 0.001f) continue;

            // Mientras más cerca está el asteroide, más fuerte el empuje
            float influence = 1f - Mathf.Clamp01(away.magnitude / avoidanceRadius);
            offset += away.normalized * influence * avoidanceStrength;
            count++;
        }

        return count > 0 ? offset / count : Vector2.zero;
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

}
