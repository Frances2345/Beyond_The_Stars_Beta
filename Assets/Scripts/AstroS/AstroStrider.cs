using UnityEngine;
using System;

public class AstroStrider : MonoBehaviour, IDamageable, IDefendable
{
    public float maxHealth = 400f;
    private float currentHealth;
    public int scoreValue = 250;

    public float maxShield = 250f;
    private float currentShield;
    public float regenCooldownTime = 7f;
    private float regenTimer = 0f;

    public float MaxShield => maxShield;
    public float CurrentShield => currentShield;
    public bool IsShieldActive => currentShield > 0f;

    public event Action OnShieldDepleted;

    public bool IsAlive => currentHealth > 0;
    public event Action OnDied;

    public GameObject bulletPrefab;
    private GameObject jugador;
    private Rigidbody2D rb;

    public float moveSpeed = 2f;
    public float bulletSpeed = 8f;
    public float fireRate = 1f;
    public float attackRange = 130f;
    public float WaitTime = 2f;

    private bool isWaiting = true;
    private float initialTimer = 0f;
    private bool isSoundActive = false;

    // ----------------------------
    // BOOLEANOS PARA ANIMACIONES
    public bool IsShooting = false;
    public bool IsCharging = false;
    // ----------------------------

    // ----------------------------
    // NUEVO COMPORTAMIENTO: ORBITAR, CARGAR Y DISPARAR
    // Radio orbital alrededor del jugador (distancia mín/máx)
    public float orbitRadiusMin = 80f;
    public float orbitRadiusMax = 90f;
    // Tiempo quieto cargando el disparo
    public float chargeDuration = 1.5f;
    // Pausa tras disparar antes de reposicionarse
    public float postShotPause = 0.75f;
    // Tolerancia para considerar que llegó al punto de reposición
    public float reachDistance = 0.5f;
    // Tiempo máximo moviéndose antes de quedarse quieto y cargar
    public float moveTime = 3.5f;
    // ----------------------------

    // ----------------------------
    // ESQUIVA DE ASTEROIDES
    // ----------------------------
    public float avoidanceDistance = 12f;
    public float avoidanceRadius = 5f;
    // ----------------------------

    // ----------------------------
    // MÁQUINA DE ESTADOS
    // ----------------------------
    private enum StriderMode { Repositioning, Charging, Paused }
    private StriderMode currentMode = StriderMode.Repositioning;

    // Punto objetivo fijo (congelado al elegirlo) alrededor del jugador
    private Vector2 repositionTarget;
    private float chargeTimer = 0f;
    private float pauseTimer = 0f;
    private float repositionTimer = 0f;
    // ----------------------------

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        currentHealth = maxHealth;
        currentShield = maxShield;

        jugador = GameObject.FindGameObjectWithTag("Player");
        if (jugador == null)
        {
            Debug.LogWarning("No se encontró el jugador de tag 'Player'");
            enabled = false;
        }
    }

    public void TakeDamage(float amount, bool isCritical)
    {
        float finalDamage = amount;
        if (isCritical)
        {
            finalDamage *= 2f;
            Debug.Log("GOLPE CRITICO");
        }
        TakeDamage(finalDamage);
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive) return;

        regenTimer = 0f;
        float remainingDamage = amount;

        if (IsShieldActive)
        {
            if (currentShield >= remainingDamage)
            {
                currentShield -= remainingDamage;
                remainingDamage = 0f;
                Debug.Log(gameObject.name + " Escudo absorbió " + amount + " de daño. Escudo restante: " + currentShield + ".");
            }
            else
            {
                remainingDamage -= currentShield;
                currentShield = 0f;
                OnShieldDepleted?.Invoke();
            }
        }

        if (remainingDamage > 0f)
        {
            currentHealth -= remainingDamage;
            Debug.Log(gameObject.name + " recibió " + remainingDamage + " de daño directo. Vida restante " + currentHealth + ".");
        }

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    void Update()
    {
        if (jugador == null || !IsAlive)
        {
            return;
        }

        if (!IsShieldActive)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenCooldownTime)
            {
                currentShield = maxShield; 
                regenTimer = 0f;
                Debug.Log("¡Escudo de " + gameObject.name + " restaurado por completo!");
            }

        }

        if (isWaiting)
        {
            InitialWait();
            return;
        }

        float distanceToPlayer = Vector2.Distance(transform.position, jugador.transform.position);

        if (distanceToPlayer < attackRange && !isSoundActive)
        {
            isSoundActive = true;
            PlayAstroSound();
        }
        else if (distanceToPlayer >= attackRange && isSoundActive)
        {
            isSoundActive = false;
        }

        // Fuera de alcance: acercarse directamente al jugador
        if (distanceToPlayer > attackRange)
        {
            MoveTowardPlayer();
            return;
        }

        // ----------------------------
        // MÁQUINA DE ESTADOS
        // ----------------------------
        switch (currentMode)
        {
            case StriderMode.Repositioning:
                HandleRepositioning();
                break;

            case StriderMode.Charging:
                HandleCharging();
                break;

            case StriderMode.Paused:
                HandlePaused();
                break;
        }
    }
    private void PlayAstroSound()
    {
        if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.StriderSound != null)
        {
            Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.StriderSound, transform.position);
        }
    }

    private void InitialWait()
    {
        initialTimer += Time.deltaTime;

        if (initialTimer >= WaitTime)
        {
            isWaiting = false;
            StartRepositioning();
        }
    }

    // ----------------------------
    // MODO 1: MOVERSE ALREDEDOR DEL JUGADOR
    // ----------------------------
    private void HandleRepositioning()
    {
        IsCharging = false;
        IsShooting = false;

        repositionTimer += Time.deltaTime;

        // Tiempo de movimiento cumplido: quedarse quieto y cargar el disparo
        if (repositionTimer >= moveTime)
        {
            rb.linearVelocity = Vector2.zero;
            chargeTimer = 0f;
            currentMode = StriderMode.Charging;
            return;
        }

        Vector2 toPlayer = (Vector2)jugador.transform.position - (Vector2)transform.position;
        float distanceToPlayer = toPlayer.magnitude;

        // No acercarse más del radio mínimo: empujarse hacia afuera
        if (distanceToPlayer < orbitRadiusMin)
        {
            Vector2 awayDir = -toPlayer.normalized;
            rb.linearVelocity = SteerVelocity(awayDir) * moveSpeed;
            return;
        }

        Vector2 target = repositionTarget;

        // El punto objetivo no puede quedar dentro del anillo según el jugador actual
        Vector2 playerToTarget = target - (Vector2)jugador.transform.position;
        if (playerToTarget.magnitude < orbitRadiusMin)
        {
            target = (Vector2)jugador.transform.position + playerToTarget.normalized * orbitRadiusMin;
        }

        Vector2 toTarget = target - (Vector2)transform.position;

        // Llegó al punto: quedarse quieto y cargar el disparo
        if (toTarget.magnitude <= reachDistance)
        {
            rb.linearVelocity = Vector2.zero;
            chargeTimer = 0f;
            currentMode = StriderMode.Charging;
            return;
        }

        rb.linearVelocity = SteerVelocity(toTarget.normalized) * moveSpeed;
    }

    // Punto aleatorio (dirección + radio) fijado alrededor del jugador en ese instante
    private void PickNewOrbitPosition()
    {
        float angle = UnityEngine.Random.Range(0f, Mathf.PI * 2f);
        float radius = UnityEngine.Random.Range(orbitRadiusMin, orbitRadiusMax);
        repositionTarget = (Vector2)jugador.transform.position + new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * radius;
    }

    private void StartRepositioning()
    {
        repositionTimer = 0f;
        PickNewOrbitPosition();
        currentMode = StriderMode.Repositioning;
    }

    // Evita asteroides que estén en la trayectoria desviando la dirección
    private Vector2 SteerVelocity(Vector2 moveDir)
    {
        if (moveDir.sqrMagnitude < 0.001f) return moveDir;

        float step = avoidanceRadius;

        // Muestrea puntos por delante en la dirección de movimiento
        for (float d = avoidanceRadius; d <= avoidanceDistance; d += step)
        {
            Vector2 point = (Vector2)transform.position + moveDir * d;

            Collider2D[] hits = Physics2D.OverlapCircleAll(point, avoidanceRadius);
            foreach (Collider2D hit in hits)
            {
                if (hit == null || !hit.CompareTag("Asteroid")) continue;

                // Asteroide delante: virar en perpendicular y alejándose
                Vector2 toAsteroid = (Vector2)hit.transform.position - (Vector2)transform.position;
                if (toAsteroid.sqrMagnitude < 0.001f) continue;

                Vector2 tangent = new Vector2(-toAsteroid.y, toAsteroid.x).normalized;
                float side = Vector2.Dot(tangent, moveDir) >= 0f ? 1f : -1f;

                moveDir = (moveDir * 0.4f + tangent * side * 1.2f).normalized;
                break;
            }
        }

        return moveDir;
    }

    // ----------------------------
    // MODO 2: QUIETO CARGANDO EL DISPARO
    // ----------------------------
    private void HandleCharging()
    {
        rb.linearVelocity = Vector2.zero;
        IsCharging = true;
        IsShooting = false;

        chargeTimer += Time.deltaTime;

        if (chargeTimer >= chargeDuration)
        {
            PerformShoot();
            chargeTimer = 0f;
            pauseTimer = 0f;
            currentMode = StriderMode.Paused;
        }
    }

    // ----------------------------
    // MODO 3: BREVE PAUSA TRAS DISPARAR
    // ----------------------------
    private void HandlePaused()
    {
        rb.linearVelocity = Vector2.zero;
        IsCharging = false;
        IsShooting = false;

        pauseTimer += Time.deltaTime;

        if (pauseTimer >= postShotPause)
        {
            // Reposicionarse en un punto nuevo y aleatorio
            StartRepositioning();
        }
    }

    // Acercarse directamente al jugador si está fuera de alcance
    private void MoveTowardPlayer()
    {
        IsCharging = false;
        IsShooting = false;

        Vector3 direction = (jugador.transform.position - transform.position).normalized;
        rb.linearVelocity = SteerVelocity(direction) * moveSpeed;
        currentMode = StriderMode.Repositioning;
    }

    private void PerformShoot()
    {
        // ----------------------------
        // SEÑAL: EMPIEZA DISPARO
        // ----------------------------
        IsShooting = true;

        // Usa el clip AstrorShoot
        if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.StriderShoot != null)
        {
            Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.StriderShoot, transform.position);
        }

        if (bulletPrefab == null)
        {
            // apagar la señal si no dispara
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

        // ----------------------------
        // SEÑAL: TERMINA DISPARO
        // ----------------------------
        IsShooting = false;
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, orbitRadiusMin);
        Gizmos.DrawWireSphere(transform.position, orbitRadiusMax);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere((Vector3)repositionTarget, 0.5f);
    }

    public void Die()
    {
        currentHealth = 0;
        OnDied?.Invoke();

        if (ScoreManager.Instance != null)
        {
            ScoreManager.Instance.AddScore(scoreValue);
        }

        if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.StriderDeath != null)
        {
            Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.StriderDeath, transform.position);
        }

        Destroy(gameObject);
    }
}
