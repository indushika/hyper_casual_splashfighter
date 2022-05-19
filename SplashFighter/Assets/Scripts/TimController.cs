using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimController : MonoBehaviour
{
    [SerializeField]
    public ParticleSystem FlameVFX;

    [SerializeField]
    public ParticleSystem SmokeVFX;

    [SerializeField]
    public ParticleSystem HeartVFX;

    public GameObject flames;
    public Animator animator; 

    public bool isSaved = false;

    bool moveToTruck = false;
    [SerializeField]
    float speed = 5;
    public Transform target;

    public bool isMoving;
    public Transform player;

    public List<Transform> movingPoint;
    int currentMovingPnt; 

    private void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player").transform;
        currentMovingPnt = 0;
        if (isMoving)
        {
            animator.Play("0_12");
        }
        
    }
    private void Update()
    {
        if (moveToTruck)
        {
            // Move our position a step closer to the target.
            float step = speed * Time.deltaTime; // calculate distance to move
            transform.position = Vector3.MoveTowards(transform.position, target.position, step);

            // Check if the position of the cube and sphere are approximately equal.
            if (Vector3.Distance(transform.position, target.position) < 0.1f)
            {
                // Swap the position of the cylinder.
                moveToTruck = false;
                transform.LookAt(player);
                animator.Play("2"); 

                //gameObject.SetActive(false);

            }
        }

        if (isMoving)
        {
            if (currentMovingPnt < movingPoint.Count)
            {
                // Move our position a step closer to the target.
                float step = speed * Time.deltaTime; // calculate distance to move
                transform.position = Vector3.MoveTowards(transform.position, movingPoint[currentMovingPnt].position, step);
                transform.LookAt(movingPoint[currentMovingPnt]);

                // Check if the position of the cube and sphere are approximately equal.
                if (Vector3.Distance(transform.position, movingPoint[currentMovingPnt].position) < 1f)
                {
                    // Swap the position of the cylinder.
                    //moveToTruck = false;
                    //animator.Play("2");
                    currentMovingPnt++;
                    //gameObject.SetActive(false);

                }
            }
            else
            {
                currentMovingPnt = 0; 
            }

            
        }
    }

    public void MoveToFireTruck()
    {
        transform.LookAt(target);
        moveToTruck = true; 
    }

    public void PlayHeart()
    {
        HeartVFX.Play();
    }
}
