using UnityEngine;
using System;

public class AstroTrooper : MonoBehaviour, IDamageable
{
    public float maxHealth = 400f;
    private float currentHealth;
    public int scoreValue = 250;

    public bool IsAlive => currentHealth > 0;
    public event Action OnDied;

    public GameObject bulletPrefab;
    private GameObject jugador;
    private Rigidbody2D rb;

    public float moveSpeed = 2f;
    public float bulletSpeed = 8f;
    public float fireRate = 1f;
    public float attackRange = 5f;
    public float WaitTime = 2f;

    private bool isWaiting = true;
    private float fireTimer = 0f;
    private float initialTimer = 0f;
    private bool isSoundActive = false;

    public Transform spriteToRotate;

    // ----------------------------
    // Animación
    // ----------------------------
    public bool IsShooting = false;

    // ----------------------------
    // DASH
    // ----------------------------
    public float dashRange = 3f;
    public float dashSpeed = 12f;
    public float dashDuration = 0.25f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private bool canDash = true;

    // ----------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;

        jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador == null)
        {
            Debug.LogWarning("No se encontró el jugador de tag 'Player'");
            enabled = false;
        }

        if (spriteToRotate == null)
        {
            spriteToRotate = transform;
        }
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        currentHealth -= amount;
        Debug.Log(gameObject.name + " recibió daño. Vida restante " + currentHealth + ".");

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Update()
    {
        if (jugador == null || !IsAlive)
            return;

        HandleLookAtPlayer();

        fireTimer += Time.deltaTime;

        if (isWaiting)
        {
            InitialWait();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, jugador.transform.position);

        // Rango Sonido
        if (distanceToPlayer < attackRange && !isSoundActive)
        {
            isSoundActive = true;
            PlayAstroSound();
        }
        else if (distanceToPlayer >= attackRange && isSoundActive)
        {
            isSoundActive = false;
        }

        // ----------------------------
        // DASH
        // ----------------------------
        if (!isDashing && canDash && distanceToPlayer <= dashRange)
        {
            StartCoroutine(DoDash());
        }

        // Si está dashing, NO mover hacia otro lado
        if (isDashing)
            return;

        // ----------------------------
        // FOLLOW
        // ----------------------------
        if (distanceToPlayer < attackRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMovement();
        }

        // ----------------------------
        // SHOOT
        // ----------------------------
        if (fireTimer >= fireRate && distanceToPlayer <= attackRange)
        {
            PerformShoot();
            fireTimer = 0f;
        }
    }

    // Rotación del sprite hacia el jugador
    private void HandleLookAtPlayer()
    {
        if (spriteToRotate == null || jugador == null) return;

        Vector2 dir = jugador.transform.position - spriteToRotate.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        spriteToRotate.rotation = Quaternion.Euler(0, 0, angle + 90f);
    }

    private void PlayAstroSound()
    {
        if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.AstroSound != null)
        {
            Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.AstroSound, transform.position);
        }
    }

    private void InitialWait()
    {
        initialTimer += Time.deltaTime;

        if (fireTimer >= fireRate)
        {
            PerformShoot();
            fireTimer = 0f;
        }

        if (initialTimer >= WaitTime)
        {
            isWaiting = false;
        }
    }

    // FOLLOW
    private void ChasePlayer()
    {
        Vector3 direction = (jugador.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

    // SHOOT
    private void PerformShoot()
    {
        IsShooting = true;

        if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.AstroShoot != null)
        {
            Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.AstroShoot, transform.position);
        }

        if (bulletPrefab == null)
        {
            IsShooting = false;
            return;
        }

        Vector3 direction = (jugador.transform.position - transform.position).normalized;
        GameObject bala = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rbBullet = bala.GetComponent<Rigidbody2D>();

        if (rbBullet != null)
        {
            rbBullet.linearVelocity = direction * bulletSpeed;
        }

        IsShooting = false;
    }

    // ----------------------------
    // DASH (COROUTINE)
    // ----------------------------
    private System.Collections.IEnumerator DoDash()
    {
        isDashing = true;
        canDash = false;

        Vector2 dashDir = (jugador.transform.position - transform.position).normalized;

        rb.linearVelocity = dashDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;

        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }
    // ----------------------------

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, dashRange);
    }

    public void Die()
    {
        currentHealth = 0;
        OnDied?.Invoke();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.AstroDeath != null)
        {
            Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.AstroDeath, transform.position);
        }

        Debug.Log(gameObject.name + " ha sido eliminado.");
        Destroy(gameObject);
    }

}
