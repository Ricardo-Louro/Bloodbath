using Unity.Netcode;
using UnityEngine;

public class HPBubble : NetworkBehaviour
{
    private NetworkManager networkManager;
    private MeshRenderer meshRenderer;
    private SphereCollider sphereCollider;

    private NetworkVariable<bool> active = new NetworkVariable<bool>();
    private NetworkVariable<float> pickupTime = new NetworkVariable<float>();

    [SerializeField] private float spawnCooldown; 

    private void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        
        meshRenderer = GetComponent<MeshRenderer>();
        sphereCollider = GetComponent<SphereCollider>();

        active.Value = true;

        active.OnValueChanged += ToggleStatus;
    }

    private void Update()
    {
        if (networkManager.IsHost || networkManager.IsServer)
        {
            if (!active.Value && Time.time - pickupTime.Value >= spawnCooldown)
            {
                active.Value = true;
            }
        }
    }

    private void ToggleStatus(bool previousValue, bool newValue)
    {
        meshRenderer.enabled = active.Value;
        sphereCollider.enabled = active.Value;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (networkManager.IsHost || networkManager.IsServer)
        {            
            HealthSystem healthSystem = other.GetComponent<HealthSystem>();
            if(healthSystem != null)
            {
                if (healthSystem.currentHealth.Value < healthSystem.maxHealth)
                {
                    active.Value = false;
                    pickupTime.Value = Time.time;
                }
            }
        }
    }
}
