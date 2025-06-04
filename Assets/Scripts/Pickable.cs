using Unity.Netcode;
using UnityEngine;

public abstract class Pickable : NetworkBehaviour
{
    private NetworkManager networkManager;
    private MeshRenderer meshRenderer;
    private Collider objCollider;

    private NetworkVariable<bool> active = new NetworkVariable<bool>();
    private float pickupTime;
    [SerializeField] private float spawnCooldown; 

    private void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        
        meshRenderer = GetComponent<MeshRenderer>();
        objCollider = GetComponent<Collider>();

        active.Value = true;

        active.OnValueChanged += ToggleStatus;
    }

    private void Update()
    {
        if (networkManager.IsHost)
        {
            if (!active.Value && Time.time - pickupTime >= spawnCooldown)
            {
                active.Value = true;
            }
        }
    }

    private void ToggleStatus(bool previousValue, bool newValue)
    {
        meshRenderer.enabled = active.Value;
        GetComponent<Collider>().enabled = active.Value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (networkManager.IsHost)
        {
            Pickup(other);
        }
    }

    protected abstract void Pickup(Collider collision);
}
