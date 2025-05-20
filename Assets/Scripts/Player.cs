using UnityEngine;

public class Player : MonoBehaviour
{
    private Rigidbody rb;
    [SerializeField] private float moveSpeed;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        rb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void Update()
    {
    }

    private void FixedUpdate()
    {
        Vector3 vel = transform.forward * Input.GetAxis("Vertical")
                      + transform.right * Input.GetAxis("Horizontal");

        vel = vel.normalized * moveSpeed;

        rb.linearVelocity = vel;
    }
}