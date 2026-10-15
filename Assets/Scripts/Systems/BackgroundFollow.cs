using UnityEngine;

public class BackgroundFollow : MonoBehaviour
{
    [Tooltip("Objetivo a seguir. Si esta vacio, se usa la camara principal.")]
    public Transform target;

    [Range(0f, 1f)]
    [Tooltip("1 = el fondo sigue exactamente al objetivo (fondo infinito). <1 = paralaje.")]
    public float parallaxX = 1f;

    [Range(0f, 1f)]
    public float parallaxY = 1f;

    public Vector2 offset = Vector2.zero;

    private Vector2 startPos;
    private Vector2 targetStart;
    private float startZ;

    void Start()
    {
        if (target == null && Camera.main != null)
            target = Camera.main.transform;

        startPos = transform.position;
        startZ = transform.position.z;
        if (target != null) targetStart = target.position;
    }

    void LateUpdate()
    {
        if (target == null) return;

        Vector2 delta = (Vector2)target.position - targetStart;
        Vector3 pos = startPos + offset + new Vector2(delta.x * parallaxX, delta.y * parallaxY);
        pos.z = startZ;
        transform.position = pos;
    }
}
