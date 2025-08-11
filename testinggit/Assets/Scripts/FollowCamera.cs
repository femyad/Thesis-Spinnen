using UnityEngine;

public class FollowCamera : MonoBehaviour
{
    public Transform target;        // The spider to follow
    public Vector3 offset = new Vector3(0, 5, -10);  // Offset from the target
    public float smoothSpeed = 0.125f;               // Smooth factor

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed);
        transform.position = smoothedPosition;

        transform.LookAt(target);
    }
}
