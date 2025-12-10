using UnityEngine;

public class FollowMouse : MonoBehaviour
{
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 direction = mousePos - transform.position;

        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // Compensamos porque el sprite mira hacia ARRIBA (90�)
        transform.rotation = Quaternion.Euler(0, 0, angle - 90f);
    }
}