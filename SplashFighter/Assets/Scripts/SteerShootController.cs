using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SteerShootController : MonoBehaviour
{
    public ParticleSystem WaterBeam;

    ParticleSystem.MainModule main;

    private Vector3 starPosition;
    private bool istouch;
    public float speed;
    private float xRotation;
    private float dragDistance;
    private bool isRight;
    Transform waterBeamTransform; 

    void Start()
    {
        dragDistance = (15 / 100) * (Screen.width);
        waterBeamTransform = WaterBeam.transform; 
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            starPosition = Input.mousePosition;
            istouch = true;
            WaterBeam.Play();
        }
        if (Input.GetMouseButtonUp(0))
        {
            WaterBeam.Stop();
            istouch = false;
        }

        if (istouch && (Mathf.Abs(starPosition.x - Input.mousePosition.x) > dragDistance))
        {
            float angle = transform.rotation.eulerAngles.y - 360;
            if (Input.mousePosition.x > starPosition.x)
            {
                //Debug.Log(transform.rotation.y );
                //if (transform.rotation.y < 0.5f || transform.rotation.y < -0.5f)
                //{
                //    yRotation = transform.rotation.y + (1 * Time.deltaTime * speed);
                //    transform.rotation = new Quaternion(transform.rotation.x, yRotation, transform.rotation.z, transform.rotation.w);
                //}

                transform.rotation = Quaternion.Euler(0, 90, 0);


            }
            else
            {
                //Debug.Log(transform.rotation.y);

                //if (transform.rotation.y > -0.5f || transform.rotation.y > 0.5f)
                //{
                //    yRotation = transform.rotation.y - (1 * Time.deltaTime * speed);
                //    transform.rotation = new Quaternion(transform.rotation.x, yRotation, transform.rotation.z, transform.rotation.w);
                //}
                transform.rotation = Quaternion.Euler(0, -90, 0);

            }
        }

        //if (istouch && (Mathf.Abs(starPosition.y - Input.mousePosition.y) > dragDistance))
        //{
        //    //Debug.Log(transform.rotation.x);

        //    if (Input.mousePosition.y > starPosition.y)
        //    {
        //        if (transform.rotation.x < 0.5f || transform.rotation.x < -0.5f)
        //        {
        //            xRotation = transform.rotation.x + (1 * Time.deltaTime * speed);
        //            transform.rotation = new Quaternion(xRotation, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        //        }
        //        //transform.rotation = Quaternion.Euler(-60, 0, 0);

        //    }
        //    else
        //    {
        //        if (transform.rotation.x > -0.5f || transform.rotation.x > 0.5f)
        //        {
        //            xRotation = transform.rotation.x - (1 * Time.deltaTime * speed);
        //            transform.rotation = new Quaternion(xRotation, transform.rotation.y, transform.rotation.z, transform.rotation.w);
        //        }
        //        //waterBeamTransform.localRotation = Quaternion.Euler(60, 0, 0);

        //    }
        //}


    }

}
