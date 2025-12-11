using UnityEngine;
using System;
using System.Collections;

public class EnemyPatrol : MonoBehaviour, IDamageable
{
    [SerializeField] private float maxHealth = 75f;
    private float currentHealth;
    public int scoreValue = 750;

    public float damageToPlayer = 170f;

    public bool IsAlive => currentHealth > 0;
    public event Action OnDied;

    private Transform player;
    private Rigidbody2D rb;

    public float fallSpeed = 8;
    public float visionRange = 5;

    private bool isFalling = false;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if(Player1.Instance != null)
        {
            player = Player1.Instance.transform;
        }
        else
        {
            Debug.Log("No hay instancia del Jugador");
            enabled = false;
        }

        if(rb != null)
        {
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.zero;
        }
    }

    public void TakeDamage(float amount)
    {
        if(!IsAlive)
        {
            return;
        }

        currentHealth -= amount;

        if(currentHealth <= 0)
        {
            Die();
        }
    }


    void Update()
    {
        if (player == null || !IsAlive || isFalling)
        {
            return;
        }

        float distance = Vector2.Distance(transform.position, player.position);

        if(distance <= visionRange)
        {
            StartFall();
        }
    }

    private void StartFall()
    {
        isFalling = true;

        if (Level2SoundManager.Instance != null && Level2SoundManager.Instance.TaladriumSound != null)
        {
            Level2SoundManager.Instance.PlayClip(Level2SoundManager.Instance.TaladriumSound, transform.position);
        }

        if (rb != null)
        {
            rb.bodyType = RigidbodyType2D.Dynamic;
            rb.gravityScale = 0;
            rb.linearVelocity = Vector2.down * fallSpeed;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(collision.gameObject.CompareTag("Player"))
        {
            IDamageable damageableTarget = collision.gameObject.GetComponent<IDamageable>();
            if (damageableTarget != null)
            {
                damageableTarget.TakeDamage(damageToPlayer);
                Debug.Log("MAS CUIDADOSO LA SIGUIENTE, QUE NO LA PUEDES CONTAR");
            }
            Destroy(gameObject);
        }
        if (collision.gameObject.CompareTag("Limits"))
        {
            Destroy(gameObject);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    public void Die()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        if (Level2SoundManager.Instance != null && Level2SoundManager.Instance.TaladriumDeath != null)
        {
            Level2SoundManager.Instance.PlayClip(Level2SoundManager.Instance.TaladriumDeath, transform.position);
        }

        currentHealth = 0;
        OnDied?.Invoke();
        Destroy(gameObject);
    }
}
