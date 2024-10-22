
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{

    public Rigidbody rb;
    public float movingSpeed = 500f;
    public float jumpForce = 500f;

    // Start is called before the first frame update
    void Start()
    {
        //rb.useGravity = false;
        //rb.AddForce(0,200,500);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        //rb.AddForce (0,0,forwardForce * Time.deltaTime);

        if ( Input.GetKey(KeyCode.RightArrow))
        {
            rb.AddForce(movingSpeed * Time.deltaTime, 0, 0);
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            rb.AddForce(-movingSpeed * Time.deltaTime, 0, 0);
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            rb.AddForce(0, 0, movingSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            rb.AddForce(0, 0, -movingSpeed * Time.deltaTime);
        }

        if (Input.GetKey(KeyCode.Space))
        {
            rb.AddForce(0, jumpForce * Time.deltaTime, 0);
        }
    }
}
