using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class AsteroidHitbox : MonoBehaviour
{
    [Tooltip("Factor de la hitbox respecto al sprite. <1 = mas pequena que el asteroide.")]
    public float shapeScale = 0.8f;

    void Start()
    {
        Apply();
    }

    public void Apply()
    {
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr == null || sr.sprite == null) return;

        Sprite sprite = sr.sprite;
        int shapeCount = sprite.GetPhysicsShapeCount();
        if (shapeCount <= 0) return;

        List<List<Vector2>> paths = new List<List<Vector2>>();
        List<Vector2> buffer = new List<Vector2>();
        float maxAbs = 0f;

        for (int i = 0; i < shapeCount; i++)
        {
            buffer.Clear();
            sprite.GetPhysicsShape(i, buffer);
            if (buffer.Count < 3) continue;

            List<Vector2> path = new List<Vector2>(buffer);
            for (int p = 0; p < path.Count; p++)
            {
                float m = Mathf.Max(Mathf.Abs(path[p].x), Mathf.Abs(path[p].y));
                if (m > maxAbs) maxAbs = m;
            }
            paths.Add(path);
        }

        if (paths.Count == 0) return;

        float spriteRadius = 0.5f * Mathf.Max(sprite.bounds.size.x, sprite.bounds.size.y);
        float correction = 1f;
        if (maxAbs > spriteRadius * 4f && sprite.pixelsPerUnit > 0f)
            correction = 1f / sprite.pixelsPerUnit;

        Collider2D circle = GetComponent<CircleCollider2D>();
        if (circle != null) Destroy(circle);

        PolygonCollider2D poly = GetComponent<PolygonCollider2D>();
        if (poly == null) poly = gameObject.AddComponent<PolygonCollider2D>();

        poly.pathCount = paths.Count;
        for (int i = 0; i < paths.Count; i++)
        {
            List<Vector2> path = paths[i];
            for (int p = 0; p < path.Count; p++)
                path[p] = path[p] * correction * shapeScale;
            poly.SetPath(i, path);
        }
    }
}
