using Unity.Netcode;
using UnityEngine;

public class HPBubble : MonoBehaviour
{
    private NetworkManager networkManager;
    private MeshRenderer meshRenderer;
    private SphereCollider sphereCollider;

    private bool active;
    private float pickupTime = 0;
    [SerializeField] private float spawnCooldown; 


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        
        meshRenderer = GetComponent<MeshRenderer>();
        sphereCollider = GetComponent<SphereCollider>();

        active = true;
    }

    private void Update()
    {
        if(networkManager.IsHost || networkManager.IsServer)
        {
            CheckForSpawn();
        }
    }

    private void CheckForSpawn()
    {
        if(!active && Time.time - pickupTime >= spawnCooldown)
        {
            meshRenderer.enabled = true;
            sphereCollider.enabled = true;
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.GetComponent<Player>() != null)
        {
            meshRenderer.enabled = false;
            sphereCollider.enabled = false;

            active = false;
            pickupTime = Time.time;
        }
    }
}
