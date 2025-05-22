using Unity.Netcode;
using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float moveSpeed;

    private NetworkObject networkObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();

        if (networkObject.IsLocalPlayer)
        {
            AssignCamera();
        }
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        if(networkObject.IsLocalPlayer)
        {
            Vector3 vel = transform.forward * Input.GetAxis("Vertical")
                          + transform.right * Input.GetAxis("Horizontal");

            vel = vel.normalized * moveSpeed;

            rb.linearVelocity = vel;
        }
    }

    private void AssignCamera()
    {
        Camera.main.GetComponent<CameraController>().player = this;
    }

    public void Rotate(float yRot)
    {
        Vector3 rotation = Vector3.zero;
        rotation.y = yRot;
        transform.rotation = Quaternion.Euler(rotation);
    }
}