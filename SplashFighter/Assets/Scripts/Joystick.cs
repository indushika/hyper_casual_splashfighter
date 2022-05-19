using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Joystick : MonoBehaviour
{
    public Transform player;
    public Animator PlayerMeshAnimator; 
    public float speed = 5.0f;
    public float rotateSpeed; 
    private bool touchStart = false;
    private Vector3 pointA;
    private Vector3 pointB;
    public bool gameStarted;
    //public GameData gameData;
    
    private float directionMultiplier = -1;


    private bool isTouchBeingHeld;

    public void StartTakingInput()
    {
        gameStarted = true;

        if(Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            pointB = Camera.main.ViewportToScreenPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.transform.position.z));
        }

    }
    public void StopTakingInput()
    {
        gameStarted = false;
    }
    
    private void FixedUpdate()
    {


#if UNITY_EDITOR
        directionMultiplier = 1;
        if (Input.GetMouseButtonDown(0))
        {
            StartTakingInput(); 

            pointA = (new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
            //PlayerMeshAnimator.ResetTrigger("Shove");

            //if(gameStarted)
            //{
            //    PlayerMeshAnimator.ResetTrigger("Shove"); 
            //    PlayerMeshAnimator.SetBool("running", true);

            //}


        }
        if (Input.GetMouseButton(0))
        {
            touchStart = true;
            pointB = (new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.z));
            //if (gameStarted)
            //{
            //    PlayerMeshAnimator.ResetTrigger("Shove");
            //    PlayerMeshAnimator.SetBool("running", true);

            //}

        }
        else
        {
            //PlayerMeshAnimator.ResetTrigger("Shove");

            //if (gameStarted)
            //{
            //    PlayerMeshAnimator.ResetTrigger("Shove");
            //    PlayerMeshAnimator.SetBool("running", false);
            //}
            //player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
            touchStart = false;
        }
        if (Input.GetMouseButtonUp(0))
        {
            StopTakingInput(); 
        }
#endif

        if ((Input.touchCount > 0)&&gameStarted)
        {
            Touch touch = Input.GetTouch(0);
            switch (touch.phase)
            {
                case TouchPhase.Began:
                    touchStart = true;

                    pointB = Camera.main.ViewportToScreenPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.transform.position.z));

                    break;
                case TouchPhase.Moved:
                    pointA = Camera.main.ViewportToScreenPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.transform.position.z));
                    if(gameStarted)
                    {
                        PlayerMeshAnimator.ResetTrigger("Shove");
                        PlayerMeshAnimator.SetBool("running", true);

                    }
                    break;
                case TouchPhase.Stationary:
                    pointA = Camera.main.ViewportToScreenPoint(new Vector3(touch.position.x, touch.position.y, Camera.main.transform.position.z));
                    break;
                case TouchPhase.Ended:
                    touchStart = false;
                    if (gameStarted) player.GetComponent<Rigidbody>().velocity = new Vector3(0, 0, 0);
                    if (gameStarted)
                    {
                        PlayerMeshAnimator.ResetTrigger("Shove");
                        PlayerMeshAnimator.SetBool("running", false);
   

                    }
                    break;
                case TouchPhase.Canceled:
                    break;
                default:
                    break;
            }
        }

       
        if ((touchStart && gameStarted))
        {

            Vector3 offset = pointB - pointA;
            Vector3 direction = Vector3.ClampMagnitude(offset, 0.75f);
            direction.z = -direction.x; 
            direction.x = direction.y;
            direction.y = 0f;
            moveCharacter(direction * directionMultiplier);

           // Debug.Log(GetComponent<Rigidbody>().velocity.magnitude);

        }

    }

    //Dont mind the commented out stuff it's previous iterations we've tried
    void moveCharacter(Vector3 direction)
    {
        //player.Translate(direction * speed * Time.deltaTime);
        //player.GetComponent<Rigidbody>().AddForce(direction * speed*-1,ForceMode.VelocityChange);
        //player.transform.Translate(direction * Time.deltaTime * -speed);

        Vector3 newMoveDirection = new Vector3(direction.x, direction.y, direction.z) * speed;
        transform.Translate(newMoveDirection);

        newMoveDirection = new Vector3(direction.x, direction.y, direction.z) * rotateSpeed;
        //player.transform.rotation = Quaternion.LookRotation(direction *-1);
        player.rotation = Quaternion.LookRotation(newMoveDirection); //gameObject.transform.GetComponent<PlayerController>().playerMesh
    }

    //public void PlayerImpulsed()
    //{
    //    gameStarted = false;
    //    StartCoroutine(WaitSeconds());
    //}

    //IEnumerator WaitSeconds()
    //{
    //    // suspend execution for 5 seconds
    //    yield return new WaitForSeconds(gameData.ImpulseWait);
    //    gameStarted = true;
    //}

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.tag == "Tims")
        {
            if (!collision.gameObject.GetComponent<TimController>().isSaved)
            {
                Debug.Log("OnFire");

            }
        }
    }
}