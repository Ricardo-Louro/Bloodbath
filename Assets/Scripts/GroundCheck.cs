using Unity.Netcode;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    //Obtain reference to the NetworkObject to check ownership
    private NetworkObject networkObject;
    //Obtain reference to the playerMovement in order to switch grounded bool
    private PlayerMovement playerMovement;

    private void Start()
    {
        //Assign NetworkObject value
        networkObject = GetComponentInParent<NetworkObject>();

        //If this is the player controlled by the local client
        if (networkObject.IsLocalPlayer)
        {
            //Assign the PlayerMovement value
            playerMovement = GetComponentInParent<PlayerMovement>();
        }
    }

    //If the GroundCheck collider stays in contact with the ground
    private void OnTriggerStay(Collider other)
    {
        //If this is the player controlled by the local client
        if (networkObject.IsLocalPlayer)
        {
            //Set the grounded bool to true
            playerMovement.grounded = true;
        }
    }

    //If the GroundCheck collider stops being in contact with the ground
    private void OnTriggerExit(Collider other)
    {
        //If this is the player controlled by the local client
        if (networkObject.IsLocalPlayer)
        {
            //Set the grounded bool to false
            playerMovement.grounded = false;
        }
    }
}