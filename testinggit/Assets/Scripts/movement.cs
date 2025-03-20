using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TranslateMovement : MonoBehaviour
{
    public float horizontalInput;
    public float verticalInput;
    public float turnSpeed = 10;
    public float moveSpeed = 10;

    // jumping vars:
    public float acceleration = 2;
    public float jumpForce = 5;
    public float gravity = 9.8f;
    private bool isJumping = false;
    private float jumpStartTime;
    private Vector3 velocity;
    //////////////

    public float raycastDistance = 1f; // Raycast length to detect terrain (jumping)
    public LayerMask terrainLayer; // Layer mask for the terrain (jumping)
    public spiderAllignment allignment;
    
    private float currentSpeed = 0;
    public float GetCurrentSpeed()
{
    return currentSpeed;
}
    private Animator animator;
    

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        allignment = GetComponent<spiderAllignment>();
        currentSpeed = 0;
        Debug.Log(allignment);
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
        animator.SetFloat("moveSpeed", currentSpeed);

        // Move the object with the accelerated currentSpeed
        transform.Translate(Vector3.forward * Time.deltaTime * currentSpeed);

        //euler rond z-as rotatie
        transform.Rotate(Vector3.up * horizontalInput * turnSpeed * Time.deltaTime);

        // Jump logic
        if (Input.GetKeyDown(KeyCode.Space) && !isJumping)
        {
            allignment.enabled = false;
            isJumping = true;
            jumpStartTime = Time.time;
            animator.SetTrigger("jump");
            velocity = new Vector3(0, jumpForce, 0); // Set the jump velocity 
        }

       // Apply vertical movement for jumping or falling
        if (isJumping)
        {
            float elapsedTime = Time.time - jumpStartTime;
            // Debug.Log(elapsedTime);
            velocity.y = jumpForce - (gravity * elapsedTime * 1.5f); //  gravity
            transform.Translate(velocity * Time.deltaTime, Space.World);

            // Check for landing
            if (Physics.Raycast(transform.position + Vector3.up*raycastDistance, Vector3.down, raycastDistance, terrainLayer))
            {
                isJumping = false;
                Debug.Log("landing");
                allignment.enabled = true;
                velocity = Vector3.zero; // Reset velocity on landing
            }
        }


        // Update animator speed parameter 
        float normalizedSpeed = Mathf.Abs(currentSpeed) / moveSpeed;
        animator.SetFloat("moveSpeed", normalizedSpeed);

        //  adjust the animation speed directly
        animator.speed = 1 + (normalizedSpeed * 5.0f); // 
    }
}
