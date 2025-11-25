using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BackgroundParallax : MonoBehaviour
{
    private float StarPos;
    private float lenght;
    public GameObject cam;

    public float parallaxEffect;


    void Start()
    {
        StarPos = transform.position.x;
        lenght = GetComponent<SpriteRenderer>().bounds.size.x;
    }

    void FixedUpdate()
    {
        float distance = cam.transform.position.x * parallaxEffect;
        float movement = cam.transform.position.x * (1 - parallaxEffect);

        transform.position = new Vector3(StarPos + distance, transform.position.y,transform.position.z);


        if(movement > StarPos + lenght)
        {
            StarPos += lenght;
        }
        else if (movement < StarPos - lenght)
        {
            StarPos -= lenght;
        }
    }
}
