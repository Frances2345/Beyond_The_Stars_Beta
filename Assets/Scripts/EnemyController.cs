using UnityEngine;
using System;
using UnityEngine.Rendering;

public class EnemyController : MonoBehaviour, IDamageable
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

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;

        jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador == null)
        {
            Debug.LogWarning("No se encontró el jugador  de tag 'Player'");
            enabled = false;
        }
    }

    public void TakeDamage(float amount)
    {
        if(!IsAlive)
        {
            return;
        }

        currentHealth -= amount;
        Debug.Log(gameObject.name + "recibio daño. Vida restante " + currentHealth + " .");

        if(currentHealth <= 0f)
        {
            Die();
        }
    }

    void Update()
    {
        if(jugador == null || !IsAlive)
        {
            return;
        }

        fireTimer += Time.deltaTime;

        if(isWaiting)
        {
            InitialWait();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, jugador.transform.position);

        if(distanceToPlayer < attackRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMovement();
        }

        if(fireTimer >= fireRate && distanceToPlayer <= attackRange)
        {
            PerformShoot();
            fireTimer = 0f;
        }
    }

    private void InitialWait()
    {
        initialTimer += Time.deltaTime;

        if(fireTimer >= fireRate)
        {
            PerformShoot();
            fireTimer = 0f;
        }
        
        if(initialTimer >= WaitTime)
        {
            isWaiting = false;
        }
    }

    private void ChasePlayer()
    {
        Vector3 direction = (jugador.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }

    private void PerformShoot()
    {
        if(bulletPrefab == null)
        {
            return;
        }

        Vector3 direction = (jugador.transform.position - transform.position).normalized;
        GameObject bala = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rbBullet = bala.GetComponent<Rigidbody2D>();

        if(rbBullet != null)
        {
            rbBullet.linearVelocity = direction * bulletSpeed;
        }
    }
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }

    public void Die()
    {
        currentHealth = 0;
        OnDied?.Invoke();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        Debug.Log(gameObject.name + "ha sido eliminado.");
        Destroy(gameObject);
    }
}
