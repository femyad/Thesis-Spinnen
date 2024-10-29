using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateMovement : MonoBehaviour
{
    public float horizontalInput;
    public float verticalInput;
    public float turnSpeed = 10;
    public float moveSpeed = 10;
    public float jumpForce = 5;
    public float gravity = 9.8f;
    public float acceleration = 2;

    private bool isJumping = false;
    private float verticalVelocity = 0;
    private float currentSpeed = 0;
    

    // Start is called before the first frame update
    void Start()
    {
        currentSpeed = 0;
    }

    // Update is called once per frame
    void Update()
    {
        //keyboard values
        horizontalInput = Input.GetAxis("Horizontal");
        verticalInput = Input.GetAxis("Vertical");


         // Accelerate or decelerate to target speed based on vertical input
        if (verticalInput > 0)
        {
            // Accelerate forward
            currentSpeed += acceleration * Time.deltaTime;
        }
        else if (verticalInput < 0)
        {
            // Decelerate when moving backward or stopping
            currentSpeed -= acceleration * Time.deltaTime;
        }
        else
        {
            // Gradually bring speed back to zero if no input
            //Lerp = linear interpolation.  Interpolates between two values over a certain time, based on a percentage (0 to 1).
            currentSpeed = Mathf.Lerp(currentSpeed, 0, acceleration * Time.deltaTime);
        }

        // Clamp currentSpeed within -moveSpeed to moveSpeed
        //Clamp restricts a value to a given range. If the value is below the minimum, it returns the minimum; if above the maximum, it returns the maximum.
        currentSpeed = Mathf.Clamp(currentSpeed, -moveSpeed, moveSpeed);

        // Move the object with the accelerated currentSpeed
        transform.Translate(Vector3.forward * Time.deltaTime * currentSpeed);

        //euler rond z-as rotatie
        transform.Rotate(Vector3.up * horizontalInput * turnSpeed * Time.deltaTime);

        // Jump logic
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            isJumping = true;
            verticalVelocity = jumpForce; 
        }

        // Apply vertical movement for jumping or falling
        if (isJumping)
        {
            verticalVelocity -= gravity * Time.deltaTime; // Simulate gravity 
            transform.Translate(Vector3.up * verticalVelocity * Time.deltaTime);

            // Check if the object has landed (assume y = 0 is the ground)
            if (transform.position.y <= 0)
            {
                transform.position = new Vector3(transform.position.x, 0, transform.position.z); // Reset to ground level
                isJumping = false;
                verticalVelocity = 0;
            }
        }

    }
}
