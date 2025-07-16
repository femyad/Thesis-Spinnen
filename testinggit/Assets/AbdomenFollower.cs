using UnityEngine;

public class AbdomenFollower : MonoBehaviour
{
    public Transform prosoma; // spiderroot in this case
    public float rotationFollowSpeed = 5f; // Lower = slower following

    private Quaternion targetLocalRotation;

    void Start()
    {
        targetLocalRotation = transform.localRotation;
    }

    void Update()
    {
        targetLocalRotation = Quaternion.Slerp(
            targetLocalRotation,
            prosoma.localRotation,
            Time.deltaTime * rotationFollowSpeed
        );

        transform.localRotation = targetLocalRotation;
    }
}
