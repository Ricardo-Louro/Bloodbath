//Importing necessary namespaces
using UnityEngine;
using Unity.Netcode;

public class PlayerMovement : MonoBehaviour
{
    //Set reference to the Rigidbody component
    private Rigidbody rb;
    //Set reference to the NetworkObject component
    private NetworkObject networkObject;
    
    //Set value for the character's movement speed
    [SerializeField] private float moveSpeed;
    //Set value for the character's jump speed
    [SerializeField] private float jumpSpeed;

    //Set reference to the wielded gun's render (not the camera UI one)
    [SerializeField] private MeshRenderer gunRenderer;

    //Set public variable to determine whether the player is in the ground or not (used by the GroundCheck class)
    public bool grounded;
    //Set private variable to know whether a jump has been queued or not
    private bool jump;

    private void Start()
    {
        //Obtain the values for the required references to components
        rb = GetComponent<Rigidbody>();
        networkObject = GetComponent<NetworkObject>();

        //If this player belongs to this specific client
        if (networkObject.IsLocalPlayer)
        {
            //Assign this player to the camera
            AssignCamera();
            //Assign this player as the local one for first person view purposes
            AssignLocalPlayer();
        }
    }

    private void Update()
    {
        //If this player belongs to this specific client
        if(networkObject.IsLocalPlayer)
        {
            //If this player is grounded and the spacebar is pressed
            if (grounded && Input.GetKeyDown(KeyCode.Space))
            {
                //Queue a jump
                jump = true;
            }
        }
    }

    private void FixedUpdate()
    {
        //If this player belongs to this specific client (the client is authoritative in the movement of the player due to NetworkTransform)
        if(networkObject.IsLocalPlayer)
        {
            //Calculate the movement vector based on the player's inputs
            Vector3 vel = transform.forward * Input.GetAxis("Vertical")
                          + transform.right * Input.GetAxis("Horizontal");

            //Normalize this vector and multiply it by the desired movement speed
            vel = vel.normalized * moveSpeed;

            //If a jump is queued
            if(jump)
            {
                //Add vertical velocity equal to the jump speed
                vel.y = jumpSpeed;
                //De-queue the jump
                jump = false;
            }
            //If no jump is queued
            else
            {
                //Set the vel.y velocity to the one set within the rigidbody (allows gravity to work)
                vel.y = rb.linearVelocity.y;
            }

            //Update the rigidbody's velocity by our movement vector
            rb.linearVelocity = vel;
        }
        //If this player is not the local player
        else
        {
            //Set its velocity to zero. I do not believe this line does anything in practice but it might be useful in specific cases where there is collisions involving forces with the player which may cause desynch between clients
            rb.linearVelocity = Vector3.zero;
        }
    }

    //This method accesses the camera and assigns this player as the local one. Used within the CameraController class
    private void AssignCamera()
    {
        //Acess the camera and assign this player as the local one
        Camera.main.GetComponent<CameraController>().player = this;
    }

    //This method assigns this player as the local one for use when it comes to setting up the game's first person view
    private void AssignLocalPlayer()
    {
        //Disable rendered body and gun (prevents these models from obstructing the first person view)
        GetComponentInChildren<SkinnedMeshRenderer>().enabled = false;
        gunRenderer.enabled = false;
    }

    //This method rotates the player around the y axis according to a specific value. Used within the CameraController class
    public void Rotate(float yRot)
    {
        //Set an initial rotation of (0,0,0)
        Vector3 rotation = Vector3.zero;
        //Update the rotation around the y axis to the provided value
        rotation.y = yRot;
        //Convert the vector to a Quaternion and then apply it to the player's rotation
        transform.rotation = Quaternion.Euler(rotation);
    }
}