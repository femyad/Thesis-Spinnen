using UnityEngine;

public class FollowCamera2 : MonoBehaviour
{
    [Header("Target")]
    public Transform target;                          // The character to follow
    public float lookHeight = 1.5f;                   // Where on the target to look (eyes/chest)

    [Header("Position")]
    public Vector3 localOffset = new Vector3(0, 5, -10); // Offset in the target's local space
    public float positionSmooth = 10f;                // Higher = snappier

    [Header("Rotation")]
    public bool rotateWithTarget = true;              // Follow the target's yaw like a 3rd-person cam
    public float rotationSmooth = 12f;                // Higher = snappier rotation

    void LateUpdate()
    {
        if (!target) return;

        // 1) Make the offset relative to the target’s current rotation (so it turns with them).
        Vector3 rotatedOffset = target.rotation * localOffset;
        Vector3 desiredPos = target.position + rotatedOffset;

        // Smooth position (exponential smoothing so it’s frame-rate independent).
        float posLerp = 1f - Mathf.Exp(-positionSmooth * Time.deltaTime);
        transform.position = Vector3.Lerp(transform.position, desiredPos, posLerp);

        // 2) Look at the target (slightly above the pivot).
        Vector3 lookPoint = target.position + Vector3.up * lookHeight;
        Quaternion desiredLook = Quaternion.LookRotation(lookPoint - transform.position, Vector3.up);

        if (rotateWithTarget)
        {
            // Smooth rotation toward the look direction so the camera turns as the character turns.
            float rotLerp = 1f - Mathf.Exp(-rotationSmooth * Time.deltaTime);
            transform.rotation = Quaternion.Slerp(transform.rotation, desiredLook, rotLerp);
        }
        else
        {
            // Snap look-at if you don’t want rotational smoothing.
            transform.rotation = desiredLook;
        }
    }
}
