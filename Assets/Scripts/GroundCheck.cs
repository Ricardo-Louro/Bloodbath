using Unity.Netcode;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    private NetworkObject networkObject;
    private PlayerMovement playerMovement;

    private void Start()
    {
        networkObject = GetComponentInParent<NetworkObject>();

        if (networkObject.IsLocalPlayer)
        {
            playerMovement = GetComponentInParent<PlayerMovement>();
        }
    }

    private void OnTriggerStay(Collider other)
    {
        if(networkObject.IsLocalPlayer)
        {
            playerMovement.grounded = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (networkObject.IsLocalPlayer)
        {
            playerMovement.grounded = false;
        }
    }
}