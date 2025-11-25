using UnityEngine;

public class CinemachineNoRetrocesoX : MonoBehaviour
{
    private float maxX;

    void Start()
    {
        maxX = transform.position.x;
    }

    void LateUpdate()
    {
        Vector3 pos = transform.position;

        if (pos.x > maxX)
        {
            maxX = pos.x;
        }
        else
        {
            transform.position = new Vector3(
                maxX,
                pos.y,
                pos.z
            );
        }
    }
}
