
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody rb;
    //public float movingSpeed = 500f;
    public float jumpForce = 500f;
    public float maxSpeed = 2000f;
    public float acceleration = 50f;

    [SerializeField]private Vector3 currentSpeed; //for the players curruent speed

    // Start is called before the first frame update
    void Start()
    {
        //rb.useGravity = false;
        //rb.AddForce(0,200,500);
        currentSpeed= Vector3.zero;
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rb.AddForce (0,0,forwardForce * Time.deltaTime);
        bool isMoving = false;

        if ( Input.GetKey(KeyCode.RightArrow))
        {
            //rb.AddForce(movingSpeed * Time.deltaTime, 0, 0);
            Accelerate(Vector3.right);
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            //rb.AddForce(-movingSpeed * Time.deltaTime, 0, 0);
            Accelerate(Vector3.left);
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            //rb.AddForce(0, 0, movingSpeed * Time.deltaTime);
            Accelerate(Vector3.forward);
            isMoving = true;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            //rb.AddForce(0, 0, -movingSpeed * Time.deltaTime);
            Accelerate(Vector3.back);
            isMoving = true;
        }

        // Stop movement when no keys are pressed
        if (!isMoving)
        {
            ResetVelocity();
        }

        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(0, jumpForce * Time.deltaTime, 0);
        }
    }


    public void Accelerate (Vector3 direction)
    {
        currentSpeed += direction * acceleration * Time.deltaTime;
        currentSpeed = Vector3.ClampMagnitude(currentSpeed, maxSpeed);
        rb.AddForce(currentSpeed *  Time.deltaTime);
    }

    public void ResetVelocity()
    {
        currentSpeed = Vector3.zero;
    }
}
    