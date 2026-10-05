using UnityEngine;

public class SmoothCamFollow : MonoBehaviour
{
    [SerializeField] private Vector3 offset;
    [SerializeField] private float damping;

    public Transform target;

    private Vector3 vel = Vector3.zero;


    private void FixedUpdate()
    {
        Vector3 tarPos = target.position + offset;

        tarPos.z = transform.position.z;

        transform.position = Vector3.SmoothDamp(transform.position, transform.position, ref vel, damping);
    }

}
