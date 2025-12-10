using UnityEngine;

public class LookAtPlayerSpecialist : MonoBehaviour
{
    public Transform sprite;
    public Transform player;

    void Update()
    {
        if (sprite == null || player == null) return;

        Vector2 dir = player.position - sprite.position;
        float angle = Mathf.Atan2(dir.y, dir.x) * Mathf.Rad2Deg;

        sprite.rotation = Quaternion.Euler(0, 0, angle + 180f);
    }
}
