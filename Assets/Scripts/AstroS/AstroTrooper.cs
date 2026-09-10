using UnityEngine;
using System;

public class AstroTrooper : MonoBehaviour, IDamageable
{
    // ----------------------------
    public Animator TrooperAnimator;





    //----------------------------

    // VIDA Y PUNTUACIÓN
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
    public float fireRate = 1.5f;
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
    public float dashSpeed = 14f;
    public float dashDuration = 0.45f;
    public float dashWindup = 0.35f;
    public float dashCooldown = 0.7f;

    private bool isDashing = false;
    private bool canDash = true;

    // ----------------------------
    // DISTANCIA Y KITING
    // ----------------------------
    public float preferredDistance = 4f;
    public float preferredMargin = 0.5f;
    public float approachSpeedPerUnit = 1.5f;
    public float maxApproachSpeed = 8f;
    public float minApproachSpeed = 2f;
    public float fleeSpeedPerUnit = 3f;
    public float minFleeSpeed = 2f;

    // ----------------------------
    // IA - MODO DE COMBATE
    // ----------------------------
    public float shootTime = 6f;
    private float shootTimer = 0f;

    public int maxCharges = 3;
    private int dashCount = 5;

    // Rango para intentar embestir: si el jugador se aleja y sale de aquí,
    // el trooper abandona la embestida y vuelve a disparar
    public float chargeGiveUpRange = 5f;
    public float chargeAdvanceSpeed = 6f;

    // ----------------------------
    // RETROCESO
    // ----------------------------
    public float retreatSpeed = 16f;
    public float retreatDuration = 0.5f;
    public float retreatCooldown = 1f;
    private bool isRetreating = false;

    // ----------------------------
    // ESTADO
    // ----------------------------
    private enum CombatMode { Shooting, Charging, Retreating }
    private CombatMode currentMode = CombatMode.Shooting;

    // ----------------------------






    void Start()
    {
        TrooperAnimator = GetComponent<Animator>();

        //------------------------------

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
        // MÁQUINA DE ESTADOS
        // ----------------------------
        switch (currentMode)
        {
            case CombatMode.Shooting:
                HandleShootingMode(distanceToPlayer);
                break;

            case CombatMode.Charging:
                HandleChargingMode(distanceToPlayer);
                break;

            case CombatMode.Retreating:
                if (!isRetreating)
                    StartCoroutine(DoRetreat());
                return;
        }
    }

    // ----------------------------
    // MODO 1: DISPARAR A DISTANCIA
    // ----------------------------
    private void HandleShootingMode(float distanceToPlayer)
    {
        shootTimer += Time.deltaTime;

        // Mantener distancia preferida
        MoveToPreferredDistance(distanceToPlayer);

        // Disparar
        if (fireTimer >= fireRate && distanceToPlayer <= attackRange)
        {
            PerformShoot();
            fireTimer = 0f;
        }

        // Después de disparar un rato, decide embestir
        // Solo si el jugador sigue dentro del rango para embestir
        if (shootTimer >= shootTime && distanceToPlayer <= chargeGiveUpRange)
        {
            shootTimer = 0f;
            dashCount = 0;
            currentMode = CombatMode.Charging;
        }
    }

    // ----------------------------
    // MODO 2: EMBESTIR (CHARGING)
    // ----------------------------
    private void HandleChargingMode(float distanceToPlayer)
    {
        // Si ya agotó las embestidas, retroceder
        if (dashCount >= maxCharges)
        {
            currentMode = CombatMode.Retreating;
            return;
        }

        // Durante el dash no hace nada más
        if (isDashing)
            return;

        // El jugador se alejó fuera del rango para embestir -> abandonar y volver a disparar
        if (distanceToPlayer > chargeGiveUpRange)
        {
            AbortCharge();
            return;
        }

        // Embistir si el jugador está en rango
        if (canDash && distanceToPlayer <= dashRange)
        {
            StartCoroutine(DoDash());
            return;
        }

        // Aún no está dentro del rango de dash -> se acerca rápido para intentar embestir
        if (distanceToPlayer > dashRange)
        {
            rb.linearVelocity = (jugador.transform.position - transform.position).normalized * chargeAdvanceSpeed;
        }
        else
        {
            // Dentro del rango pero en cooldown del dash -> esperar quieto
            rb.linearVelocity = Vector2.zero;
        }
    }

    // ----------------------------
    // ABANDONAR LA EMBESTIDA
    // ----------------------------
    private void AbortCharge()
    {
        dashCount = 0;
        shootTimer = 0f;
        isDashing = false;
        canDash = true;
        rb.linearVelocity = Vector2.zero;
        currentMode = CombatMode.Shooting;
    }

    // ----------------------------
    // MANTENER DISTANCIA PREFERIDA
    // ----------------------------
    private void MoveToPreferredDistance(float distanceToPlayer)
    {
        Vector3 dirAway = (transform.position - jugador.transform.position).normalized;
        Vector3 dirToPlayer = (jugador.transform.position - transform.position).normalized;

        float diff = distanceToPlayer - preferredDistance;

        if (diff > preferredMargin)
        {
            // Muy lejos -> acercarse, MÁS RÁPIDO cuanto más lejos esté el jugador
            float speed = GetApproachSpeed(distanceToPlayer);
            rb.linearVelocity = dirToPlayer * speed;
        }
        else if (diff < -preferredMargin)
        {
            // Muy cerca -> huir, MÁS RÁPIDO cuanto más cerca esté el jugador
            float speed = Mathf.Clamp(-diff * fleeSpeedPerUnit, minFleeSpeed, retreatSpeed);
            rb.linearVelocity = dirAway * speed;
        }
        else
        {
            // En distancia ideal -> quedarse quieto y disparar
            rb.linearVelocity = Vector2.zero;
        }
    }

    // Velocidad de aproximación proporcional a la distancia del jugador
    private float GetApproachSpeed(float distanceToPlayer)
    {
        float diff = distanceToPlayer - preferredDistance;
        return Mathf.Clamp(diff * approachSpeedPerUnit, minApproachSpeed, maxApproachSpeed);
    }

    // ANMACION DEL DASH
    // ----------------------------
    public void FixedUpdate()
    {
        if (TrooperAnimator != null)
        {
            TrooperAnimator.SetBool("IsCharging", isDashing);
            TrooperAnimator.SetBool("IsShooting", IsShooting);
        }
    }

    //  --------------------------   el de las recargas: 


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

        // FASE 1: viento / preparación (animación de embestida, sin moverse)
        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(dashWindup);

        // Si el jugador se alejó durante el viento -> abandonar la embestida
        if (Vector2.Distance(transform.position, jugador.transform.position) > chargeGiveUpRange)
        {
            rb.linearVelocity = Vector2.zero;
            AbortCharge();
            yield break;
        }

        // FASE 2: dash hacia un punto MÁS ALLÁ del jugador (lo atraviesa)
        Vector2 trooperPos = transform.position;
        Vector2 playerPos = jugador.transform.position;

        Vector2 toPlayer = playerPos - trooperPos;
        Vector2 dashDir = toPlayer.normalized;

        // Caso degenerado: si el jugador está justo encima, usar dirección por defecto
        if (dashDir.sqrMagnitude < 0.001f)
        {
            dashDir = Vector2.up;
        }

        // La dirección queda fija: atraviesa al jugador y pasa de largo
        rb.linearVelocity = dashDir * dashSpeed;

        yield return new WaitForSeconds(dashDuration);

        rb.linearVelocity = Vector2.zero;

        isDashing = false;

        dashCount++;

        if (dashCount >= maxCharges)
        {
            currentMode = CombatMode.Retreating;
            yield break;
        }

        yield return new WaitForSeconds(dashCooldown);

        canDash = true;
    }

    // ----------------------------
    // RETREAT (COROUTINE)
    // ----------------------------
    private System.Collections.IEnumerator DoRetreat()
    {
        isRetreating = true;
        canDash = false;

        Vector2 retreatDir = (transform.position - jugador.transform.position).normalized;

        rb.linearVelocity = retreatDir * retreatSpeed;

        yield return new WaitForSeconds(retreatDuration);

        rb.linearVelocity = Vector2.zero;

        yield return new WaitForSeconds(retreatCooldown);

        // Reset para volver al modo de disparo
        dashCount = 0;
        shootTimer = 0f;
        canDash = true;
        isRetreating = false;
        currentMode = CombatMode.Shooting;
    }


    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, attackRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, chargeGiveUpRange);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, preferredDistance);

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

/*
 * 
 * ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣀⣀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡤⠤⠤⠤⠴⠶⠶⠒⠚⠋⠉⠉⠉⠉⣷⢀⣀⡤⠤⠶⠶⠒⠛⢶⡄⠀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣠⡀⠀⣿⠉⠀⠀⠀⠀⠀⠀⠀⠀⢿⡀⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠰⣇⣾⠀⠀⠀⠀⣴⡄⢠⣿⣄⡀⣰⠏⠙⠛⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣧⠀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣀⠤⠶⠚⠉⠉⠙⢦⣄⣀⣀⡟⠙⠋⠁⠈⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⡀⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠇⠀⠀⠀⠀⠀⠀⠀⠈⠉⠁⠀⠀⠀⠀⠀   ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣇⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀      ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠘⣇⠀⠀⠀⠀⠀⠀⠀⠀           ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⡀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀              ⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡇
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢻⡀⠀⠀⠀⠀⠀⠀⠀⠀APRUEBENOS PROFE :D⠀⠀⠀⠀⠸⡇
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣿⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀    XF  :"v ⠀⠀⠀⠀⠀⠀⠀⠀⣧
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢰⠇⣿⠀⠀⠀⠀⠀⠀             ⠀⠀⠀ ⠀⠀⠀ ⠀⠀⠀⠀⠀⡿
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡿⠀⢿⡀⠀⠀                         ⠀⠀⠀⠀⡇
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠇⠀⢸⡇⠀⠀⠀              ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⡇
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡟⠀⠀⠘⡇⠀⠀⠀⠀  ⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀   ⠀⠀⠀⠀⠀⢸⠇
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢸⠇⠀⠀⠀⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⣼⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⡾⠀⠀⠀⠀⢸⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⡠⠞⠁⠀
⠀⠀⠀⠀⠀⠀⠀⠀⠀⣰⠾⣦⠀⢸⡇⠀⠀⠀⠀⢸⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣠⡤⠔⠚⠉⠀⠀⠀⠀
⠀⠀⣴⢦⣄⠀⠀⢀⣰⠏⠀⠘⣧⣿⠀⠀⠀⠀⢠⢾⡇⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⢀⣀⣀⡤⠴⠶⠒⠋⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⡏⠀⠈⠛⠋⠉⢀⣴⣿⣟⢿⡏⠀⠀⢀⡴⠋⠀⣧⠀⠀⢀⣀⣠⣤⣤⠤⠴⠒⠚⠛⠉⠉⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⣧⢀⣴⣶⣶⡄⢾⣿⣿⡿⣸⠃⠀⢠⠞⠁⠀⠀⠈⠉⠉⠉⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⢿⣿⣿⣿⣿⣿⠘⢿⣭⡵⠋⠀⣰⠋⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠈⠳⣬⣿⣭⠯⠖⠚⠁⠀⢀⡞⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⢠⠇⠀⠀⠀⠀⠀⠀⣿⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⣰⠏⠀⣀⠀⠀⠀⠀⠀⢸⡄⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⡼⢃⡴⠚⡿⠀⠀⠀⣤⠀⠈⣷⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⢀⣾⠗⠋⠀⢠⡏⠀⠀⣸⠋⢷⡀⢹⡆⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠋⠁⣄⠀⢠⡿⡇⠀⢰⡏⠀⠀⠻⣮⣧⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠙⠛⠋⠀⡇⢠⡟⠀⠀⠀⠀⠈⠛⠁⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⣧⡟⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀⠀
⠀⠀⠀⠀⠀⠀⡿⠁⠀
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 * 
 */