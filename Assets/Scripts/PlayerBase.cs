using UnityEngine;
using System;
using System.Collections;
using UnityEngine.InputSystem;

public class Player1 : MonoBehaviour, IDamageable
{


    public Animator PlayerAnimator;
    //-----------------------------------------
    public static Player1 Instance { get; private set; }

    public event Action<float> OnHealthChanged;
    public event Action<int, int> OnMonoliumCountChanged;
    public event Action<int, int> OnHealthPackChanged;
    public event Action OnDied;

    public InputSystem_Actions inputs;
    private Vector2 moveInput;

    public int MaxMonolium = 10;
    public int MonoliumCount = 0;

    public int MaxMonoliumValue => MaxMonolium;
    public int MonoliumCountValue => MonoliumCount;

    public float maxHealth = 500f;

    private float _currentHealth;
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

    [SerializeField] private int maxHealthPacks = 3;
    [SerializeField] private float healthRegenAmount = 150f;
    [SerializeField] private float regenCooldown = 20f;
    private int currentHealthPacks;
    public int MaxHealthPacks => maxHealthPacks;

    [Header("Dash")]
    public float dashForce = 25f;
    public float dashDuration = 0.15f;
    public float dashCooldown = 1f;

    private bool isDashing = false;
    private bool isOnCooldown = false;

    [Header("Sprint")]
    public float sprintMultiplier = 1.8f;
    public float maxStamina = 100f;
    public float staminaDrainRate = 20f;
    public float staminaRegenRate = 10f;

    private float currentStamina;
    private bool isSprinting = false;

    public GameObject bulletPrefab;
    public float bulletSpeed = 15;
    private Collider2D playerCollider;

    public float speed = 12f;
    public Rigidbody2D rb;

    private AudioSource movementAudio;
    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }
        Instance = this;

        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        inputs = new InputSystem_Actions();

        movementAudio = GetComponent<AudioSource>();
        if (movementAudio == null)
        {
            Debug.LogError("Player1 necesita un componente AudioSource para el sonido de movimiento.");
        }
        movementAudio.loop = true;

        playerCollider = GetComponent<Collider2D>();
        if (playerCollider == null)
        {
            Debug.Log("Se necesita un collider en el mismo objeto");
        }
    }

    private void Start()
    {
        PlayerAnimator = GetComponent<Animator>();

        //---------------------------------------

        currentHealth = maxHealth;
        currentStamina = maxStamina;
        currentHealthPacks = maxHealthPacks;
        OnHealthPackChanged?.Invoke(currentHealthPacks, maxHealthPacks);
        StartCoroutine(RegenerateHealthPack());
    }

    public void TakeDamage(float amount)
    {
        if (!IsAlive)
        {
            return;
        }

        currentHealth -= amount;
        Debug.Log("Fuiste herido. Vida restante " + currentHealth + " .");

        if (currentHealth <= 0)
        {
            Die();
        }
    }

    private void OnEnable()
    {
        inputs.Enable();
        inputs.Player.Move.started += OnMove;
        inputs.Player.Move.started += OnMoveStart;

        inputs.Player.Move.performed += OnMove;

        inputs.Player.Move.canceled += OnMove;
        inputs.Player.Move.canceled += OnMoveStop;

        inputs.Player.Sprint.started += OnSprintStarted;
        inputs.Player.Sprint.canceled += OnSprintCanceled;

        inputs.Player.Dash.performed += OnDash;

        inputs.Player.Fire.performed += OnFire;
    }

    private void OnMoveStart(InputAction.CallbackContext context)
    {
        //----------------------
        PlayerAnimator.SetBool("IsMoving", true); //<--- animacion
        //-------------------------

        if (movementAudio != null && !movementAudio.isPlaying)
        {
            movementAudio.Play();
        }

    }

    private void OnMoveStop(InputAction.CallbackContext context)
    {
        //----------------------
        PlayerAnimator.SetBool("IsMoving", false);
        //-------------------------
        if (movementAudio != null)
        {
            movementAudio.Stop();
        }
    }

    private void OnMove(InputAction.CallbackContext context)
    {
        moveInput = context.ReadValue<Vector2>();
    }

    private void OnSprintStarted(InputAction.CallbackContext context)
    {
        if (currentStamina > 0f)
        {
            isSprinting = true;
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        isSprinting = false;
    }

    private void OnDash(InputAction.CallbackContext context)
    {
        Debug.LogWarning("Esta dasheando");

        if (!isDashing && !isOnCooldown)
        {
            Vector2 dashDirection = moveInput.normalized;

            if (dashDirection.magnitude == 0)
            {
                Debug.Log("El Dash no esta permitido si el jugador no esta en movimiento");
                return;
            }
            StartCoroutine(PerformDash(dashDirection));
        }
    }

    private IEnumerator PerformDash(Vector2 direction)
    {
        isDashing = true;
        isOnCooldown = true;

        rb.linearVelocity = direction * dashForce;

        yield return new WaitForSeconds(dashDuration);
        isDashing = false;

        yield return new WaitForSeconds(dashCooldown);
        isOnCooldown = false;
    }




    private void FixedUpdate()
    {
        if (!isDashing)
        {
            MovementController();
        }
    }
    public void MovementController()
    {
        if (rb == null)
        {
            return;
        }
        float currentSpeed = isSprinting ? speed * sprintMultiplier : speed;
        Vector2 targetVelocity = moveInput * currentSpeed;
        rb.linearVelocity = targetVelocity;
    }

    private void OnDisable()
    {
        inputs.Player.Move.started -= OnMove;
        inputs.Player.Move.started -= OnMoveStart;

        inputs.Player.Move.performed -= OnMove;

        inputs.Player.Move.canceled -= OnMove;
        inputs.Player.Move.canceled -= OnMoveStop;

        inputs.Player.Sprint.started -= OnSprintStarted;
        inputs.Player.Sprint.canceled -= OnSprintCanceled;

        inputs.Player.Dash.performed -= OnDash;
        inputs.Player.Fire.performed -= OnFire;

        inputs.Disable();

        if (movementAudio != null)
        {
            movementAudio.Stop();
        }
    }

    private void OnFire(InputAction.CallbackContext context)
    {
        Shoot();
    }

    private void Shoot()
    {
        if (bulletPrefab == null)
        {
            return;
        }

        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePos.z = 0f;
        Vector3 direction = (mousePos - transform.position).normalized;

        GameObject bullet = Instantiate(bulletPrefab, transform.position, Quaternion.identity);
        Rigidbody2D rbBullet = bullet.GetComponent<Rigidbody2D>();

        if (rbBullet != null)
        {
            rbBullet.linearVelocity = direction * bulletSpeed;
        }

        Collider2D bulletCollider = bullet.GetComponent<Collider2D>();
        if (bulletCollider != null && playerCollider != null)
        {
            Physics2D.IgnoreCollision(playerCollider, bulletCollider);
        }
    }

    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.E))
        {
            UseHealthPack();
        }

        HandleStamina();
    }

    private void HandleStamina()
    {
        if (isSprinting && moveInput.magnitude > 0f)
        {
            currentStamina -= staminaDrainRate * Time.deltaTime;
            if (currentStamina <= 0f)
            {
                currentStamina = 0f;
                isSprinting = false;
            }
        }
        else
        {
            if (currentStamina < maxStamina)
            {
                currentStamina += staminaRegenRate * Time.deltaTime;
                if (currentStamina > maxStamina)
                {
                    currentStamina = maxStamina;
                }
            }
        }
    }

    public void UseHealthPack()
    {
        if (!IsAlive) return;

        if (currentHealth > 350f)
        {
            Debug.Log("Vida alta. No se necesita Health Pack, ya que la curación se desperdiciaría.");
            return;
        }

        if (currentHealthPacks > 0)
        {
            currentHealthPacks--;

            float newHealth = currentHealth + healthRegenAmount;
            currentHealth = Mathf.Min(newHealth, maxHealth);
            OnHealthPackChanged?.Invoke(currentHealthPacks, maxHealthPacks);

            Debug.Log("Health Pack usado (E). Curación: " + healthRegenAmount + ". Stacks restantes: " + currentHealthPacks);
        }
        else
        {
            Debug.Log("No quedan Health Packs disponibles.");
        }
    }

    private IEnumerator RegenerateHealthPack()
    {
        while (true)
        {
            yield return new WaitForSeconds(regenCooldown);

            if (currentHealthPacks < maxHealthPacks)
            {
                currentHealthPacks++;
                OnHealthPackChanged?.Invoke(currentHealthPacks, maxHealthPacks);
                Debug.Log("Health Pack recargado automáticamente. Total: " + currentHealthPacks);
            }
        }
    }

    public void CollectMonolium(Monolium monoliumObject)
    {
        if (MonoliumCount < MaxMonolium)
        {
            if (Level1SoundManager.Instance != null && Level1SoundManager.Instance.MonoliumCollectClip != null)
            {
                Level1SoundManager.Instance.PlayClip(Level1SoundManager.Instance.MonoliumCollectClip, transform.position);
            }

            MonoliumCount++;
            OnMonoliumCountChanged?.Invoke(MonoliumCount, MaxMonolium);
            Debug.Log("Monolium Recolectado. " + MonoliumCount + " de " + MaxMonolium + " .");

            if (MonoliumCount == MaxMonolium)
            {
                Debug.Log("Recolectaste los Monolium, Ahora sal de este campo de guerra");
                monoliumObject.DestroyWall();
            }

            Destroy(monoliumObject.gameObject);
        }
    }

    public void Die()
    {
        currentHealth = 0;
        OnDied?.Invoke();

        Debug.Log("Has muerto.");
        Destroy(gameObject);
    }

    public Vector2 MoveInput => moveInput;
}