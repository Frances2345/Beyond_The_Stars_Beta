using UnityEngine;

[RequireComponent(typeof(Player1))]
public class PlayerShield : MonoBehaviour
{
    [Header("Vida del escudo")]
    public float maxHealth = 3000f;
    private float currentHealth;

    [Header("Regeneracion (cuando no bloqueas)")]
    public bool regenerate = true;
    public float regenDelay = 3f;
    public float regenRate = 400f;

    [Header("Tecla de bloqueo")]
    public KeyCode blockKey = KeyCode.F;

    [Header("Visual (rectangulo celeste)")]
    public float width = 10f;
    public float height = 1.8f;
    public Color color = new Color(0.4f, 0.75f, 1f, 0.35f);
    public float forwardOffset = 0f;
    public float verticalOffset = 2.5f;
    public int sortingOrder = 5;

    public bool IsBlocking => isBlocking && currentHealth > 0f;
    public bool IsBroken => currentHealth <= 0f;
    public float CurrentHealth => currentHealth;
    public float MaxHealth => maxHealth;

    private bool isBlocking;
    private float regenTimer;
    private GameObject visual;
    private SpriteRenderer sr;
    private Player1 player;
    private Camera cam;

    void Awake()
    {
        player = GetComponent<Player1>();
        currentHealth = maxHealth;
        cam = Camera.main;
        BuildVisual();
        visual.SetActive(false);
    }

    void BuildVisual()
    {
        visual = new GameObject("ShieldVisual");
        visual.transform.SetParent(transform, false);

        sr = visual.AddComponent<SpriteRenderer>();
        sr.sprite = CreateBoxSprite();
        sr.color = color;
        sr.sortingOrder = sortingOrder;

        BoxCollider2D box = visual.AddComponent<BoxCollider2D>();
        box.isTrigger = true;
        box.size = Vector2.one;

        Vector3 lossy = transform.lossyScale;
        float sx = Mathf.Approximately(lossy.x, 0f) ? width : width / lossy.x;
        float sy = Mathf.Approximately(lossy.y, 0f) ? height : height / lossy.y;
        visual.transform.localScale = new Vector3(sx, sy, 1f);
        visual.transform.localPosition = Vector3.zero;
        visual.transform.localRotation = Quaternion.identity;
    }

    Sprite CreateBoxSprite()
    {
        Texture2D tex = new Texture2D(1, 1, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Point;
        tex.wrapMode = TextureWrapMode.Clamp;
        tex.SetPixel(0, 0, Color.white);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0f, 0f, 1f, 1f), new Vector2(0.5f, 0.5f), 1f);
    }

    void Update()
    {
        if (cam == null) cam = Camera.main;

        bool held = Input.GetKey(blockKey);
        bool canBlock = held && !IsBroken && player != null && player.IsAlive;

        isBlocking = canBlock;

        if (visual != null && visual.activeSelf != canBlock)
            visual.SetActive(canBlock);

        if (canBlock)
        {
            regenTimer = 0f;
            AimAtMouse();
        }
        else if (regenerate && currentHealth < maxHealth)
        {
            regenTimer += Time.deltaTime;
            if (regenTimer >= regenDelay)
                currentHealth = Mathf.Min(maxHealth, currentHealth + regenRate * Time.deltaTime);
        }
    }

    void AimAtMouse()
    {
        if (visual == null || cam == null) return;

        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        Vector2 dir = (Vector2)mouseWorld - (Vector2)transform.position;
        if (dir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;
        visual.transform.rotation = Quaternion.Euler(0f, 0f, angle - 90f);
        visual.transform.position = (Vector2)transform.position + dir.normalized * forwardOffset + Vector2.up * verticalOffset;
    }

    public void OnBulletHit(BulletEnemy bullet)
    {
        if (!IsBlocking || bullet == null) return;

        if (bullet.breakShield)
        {
            BreakInstantly();
            if (bullet.applyKnockbackOnShield && player != null)
            {
                Vector2 dir = bullet.GetTravelDirection(transform.position);
                player.ApplyKnockback(dir, bullet.knockbackForce);
            }
            return;
        }

        TakeShieldDamage(bullet.shieldDamage);
    }

    public void TakeShieldDamage(float amount)
    {
        if (amount <= 0f || IsBroken) return;

        currentHealth -= amount;
        if (currentHealth <= 0f)
            BreakInstantly();
    }

    public void BreakInstantly()
    {
        currentHealth = 0f;
        isBlocking = false;
        regenTimer = 0f;
        if (visual != null) visual.SetActive(false);
    }
}
