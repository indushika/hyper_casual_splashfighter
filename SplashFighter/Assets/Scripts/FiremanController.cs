using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FiremanController : MonoBehaviour
{
    public TimController timController; 

    public void MoveToTruck()
    {
        timController.MoveToFireTruck(); 
    } 

    public void PlayHeart()
    {
        timController.PlayHeart(); 
    }
}
