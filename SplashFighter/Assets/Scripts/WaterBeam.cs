using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WaterBeam : MonoBehaviour
{
    public float forceParam;
    int count = 0;
    public int waterThreshold; 

    private void OnTriggerStay(Collider other)
    {

        if (other.tag == "Tims")
        {
            count++;
            if (count > waterThreshold && !other.GetComponent<TimController>().isSaved)
            {
                //if (other.GetComponent<TimController>().isMoving)
                //{
                //    other.GetComponent<Animator>().enabled = false;
                //}
                other.GetComponent<TimController>().FlameVFX.Stop();
                other.GetComponent<TimController>().SmokeVFX.Play();
                other.GetComponent<TimController>().flames.SetActive(false);

                other.GetComponent<TimController>().isSaved = true; 
                other.GetComponent<TimController>().isMoving = false; 

                other.GetComponent<TimController>().animator.Play("0");

                //cheer animation 
                //go to the fire truck 
                other.GetComponent<Obi.ObiCollider>().thickness = 0.0f; 

            }




            //other.GetComponent<Rigidbody>().AddForce(Vector3.up * forceParam, ForceMode.Impulse);
            //other.transform.position += new Vector3(1.0f, 0, 0)*Time.deltaTime * forceParam; 

        }

        if (other.tag == "Prop")
        {
            count++;

            if (count > 50 && !other.GetComponent<TimController>().isSaved)
            {
                other.GetComponent<TimController>().FlameVFX.Stop();
                other.GetComponent<TimController>().SmokeVFX.Play();

                other.GetComponent<TimController>().isSaved = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        count = 0; 
    }

}
