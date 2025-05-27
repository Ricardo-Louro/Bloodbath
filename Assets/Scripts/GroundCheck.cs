using Unity.Netcode;
using UnityEngine;

public class GroundCheck : MonoBehaviour
{
    [SerializeField] private LayerMask layerMask;

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
            if(((layerMask & (1 << other.gameObject.layer)) != 0))
            {
                playerMovement.grounded = true;
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (networkObject.IsLocalPlayer)
        {
            if (((layerMask & (1 << other.gameObject.layer)) != 0))
            {
                playerMovement.grounded = false;
            }
        }
    }
}