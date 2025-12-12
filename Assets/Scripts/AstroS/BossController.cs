using UnityEngine;
using System;
using System.Collections;




public class BossController : MonoBehaviour, IDamageable
{
    public Animator AstroObliteratorAnimator;

    public static BossController Instance { get; private set; }

    public event Action<float> OnHealthChanged;

    public float maxHealth = 18000f;
    private float _currentHealth;
    public int scoreValue = 5000;

    public float currentHealth
    {
        get => _currentHealth;
        private set
        {
            _currentHealth = value;
            OnHealthChanged?.Invoke(_currentHealth);
        }
    }

    public bool IsAlive => currentHealth > 0;
    public event Action OnDied;

    public GameObject bossBulletPrefab;
    public float bulletSpeed = 10f;

    public float moveSpeed = 7f;

    public int burstBulletCount = 20;
    public float burstDuration = 2f;
    public float cooldownTime = 8f;

    public float visionRange = 25f;
    public float regenAmount = 150f;
    public float regenInterval = 6f;

    private Transform player;
    private Rigidbody2D rb;


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        //-----------------------------
        AstroObliteratorAnimator = GetComponent<Animator>();
        //------------------------------

        rb = GetComponent<Rigidbody2D>();
        currentHealth = maxHealth;

        if (Player1.Instance != null)
        {
            player = Player1.Instance.transform;
        }
        else
        {
            Debug.LogError("No se encontró la instancia del jugador (Player1.Instance).");
            enabled = false;
            return;
        }

        StartCoroutine(RegenerateHealth());
        StartCoroutine(AttackPattern());
    }

    void Update()
    {
        if (player == null || !IsAlive) return;

        float distanceToPlayer = Vector2.Distance(transform.position, player.position);

        if (distanceToPlayer <= visionRange)
        {
            ChasePlayer();
        }
        else
        {
            StopMovement();
        }
    }

    private void ChasePlayer()
    {
        Vector3 direction = (player.transform.position - transform.position).normalized;
        rb.linearVelocity = direction * moveSpeed;
    }

    private void StopMovement()
    {
        rb.linearVelocity = Vector2.zero;
    }



    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;
        currentHealth -= amount;

        if (currentHealth <= 0f) Die();
    }

    private IEnumerator AttackPattern()
    {
        float delayBetweenShots = burstDuration / burstBulletCount;

        while (IsAlive)
        {
            if (player == null) break;

            float distanceToPlayer = Vector2.Distance(transform.position, player.position);

            if (distanceToPlayer > visionRange)
            {
                yield return null;
                continue;
            }

            yield return new WaitForSeconds(cooldownTime - burstDuration);

            for (int i = 0; i < burstBulletCount; i++)
            {
                if (player != null && Vector2.Distance(transform.position, player.position) <= visionRange)
                {
                    PerformShootTowardsPlayer();
                }
                yield return new WaitForSeconds(delayBetweenShots);
            }
        }
    }

    private void PerformShootTowardsPlayer()
    {
        if (bossBulletPrefab == null || player == null)
        {
            return;
        }

        if (BossLevelSoundManager.Instance != null && BossLevelSoundManager.Instance.BossShootClip != null)
        {
            BossLevelSoundManager.Instance.PlayBossSFX(BossLevelSoundManager.Instance.BossShootClip);
        }

        Vector3 direction = (player.position - transform.position).normalized;

        GameObject bala = Instantiate(bossBulletPrefab, transform.position, Quaternion.identity);

        Rigidbody2D rbBullet = bala.GetComponent<Rigidbody2D>();
        Collider2D bossCollider = GetComponent<Collider2D>();
        Collider2D bulletCollider = bala.GetComponent<Collider2D>();

        if (bossCollider != null && bulletCollider != null)
        {
            Physics2D.IgnoreCollision(bossCollider, bulletCollider, true);
        }

        if (rbBullet != null)
        {
            rbBullet.linearVelocity = direction * bulletSpeed;
        }
    }

    private IEnumerator RegenerateHealth()
    {
        while (IsAlive)
        {
            yield return new WaitForSeconds(regenInterval);

            if (currentHealth < maxHealth)
            {
                if (BossLevelSoundManager.Instance != null && BossLevelSoundManager.Instance.BossRoarClip != null)
                {
                    BossLevelSoundManager.Instance.PlayBossSFX(BossLevelSoundManager.Instance.BossRoarClip);
                }
                currentHealth = Mathf.Min(currentHealth + regenAmount, maxHealth);
            }
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, visionRange);
    }

    public void Die()
    {
        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        if (BossLevelSoundManager.Instance != null && BossLevelSoundManager.Instance.BossDeathClip != null)
        {
            BossLevelSoundManager.Instance.PlayBossSFX(BossLevelSoundManager.Instance.BossDeathClip);
            BossLevelSoundManager.Instance.StopMusic();
        }

        currentHealth = 0;
        OnDied?.Invoke();
        Destroy(gameObject);
    }
}