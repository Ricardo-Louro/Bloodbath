using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : MonoBehaviour
{
    private Rigidbody rb;
    
    [SerializeField] private float moveSpeed;
    [SerializeField] private float jumpSpeed;

    [SerializeField] private MeshRenderer gunRenderer;

    public bool grounded;
    private bool jump;

    private NetworkObject networkObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();

        if (networkObject.IsLocalPlayer)
        {
            AssignCamera();
            AssignLocalPlayer();
        }
    }

    private void Update()
    {
        if(networkObject.IsLocalPlayer)
        {
            if (grounded && Input.GetKeyDown(KeyCode.Space))
            {
                jump = true;
            }
        }
    }

    private void FixedUpdate()
    {
        if(networkObject.IsLocalPlayer)
        {
            Vector3 vel = transform.forward * Input.GetAxis("Vertical")
                          + transform.right * Input.GetAxis("Horizontal");

            vel = vel.normalized * moveSpeed;

            if(jump)
            {
                vel.y = jumpSpeed;
                jump = false;
            }
            else
            {
                vel.y = rb.linearVelocity.y;
            }

            rb.linearVelocity = vel;
        }
        else
        {
            rb.linearVelocity = Vector3.zero;
        }
    }

    private void AssignCamera()
    {
        Camera.main.GetComponent<CameraController>().player = this;
    }

    private void AssignLocalPlayer()
    {
        //Disable rendered body (works best for 1st person)
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        gunRenderer.enabled = false;
    }

    public void Rotate(float yRot)
    {
        Vector3 rotation = Vector3.zero;
        rotation.y = yRot;
        transform.rotation = Quaternion.Euler(rotation);
    }
}