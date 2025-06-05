using Unity.Netcode;
using UnityEngine;
using System.Collections;
using Unity.VisualScripting;

public class HealthSystem : NetworkBehaviour
{
    private NetworkManager networkManager;
    private NetworkObject networkObject;
    private SpawnSystem spawnSystem;
    private MatchManager matchManager;

    public NetworkVariable<bool> alive = new NetworkVariable<bool>(true);
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(75);
    public int maxHealth { get; private set; } = 100;
    private int maxOverhealth = 200;

    [SerializeField] private GameObject[] models;
    private Collider playerCollider;
    private UIWeapon uiWeapon;

    private void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        networkObject = GetComponent<NetworkObject>();
        spawnSystem = FindFirstObjectByType<SpawnSystem>();
        matchManager = FindFirstObjectByType<MatchManager>();

        uiWeapon = FindFirstObjectByType<UIWeapon>();

        matchManager.AddToDictionary(this);

        playerCollider = GetComponent<Collider>();

        spawnSystem.RequestMoveToSpawnPointClientRpc(gameObject);
    }

    public override void OnNetworkSpawn()
    {
        currentHealth.OnValueChanged += CheckForDeath;
        alive.OnValueChanged += ToggleDeathComponents;
    }

    public override void OnNetworkDespawn()
    {
        currentHealth.OnValueChanged -= CheckForDeath;
        alive.OnValueChanged -= ToggleDeathComponents;
    }

    private void CheckForDeath(int previousValue, int currentValue)
    {
        if (currentHealth.Value <= 0 && networkManager.IsHost)
        {
            alive.Value = false;
        }
    }

    public void LoseHealth(int value)
    {
        currentHealth.Value = Mathf.Max(currentHealth.Value - value, 0);
    }

    public void GainHealth(int value)
    {
        currentHealth.Value = Mathf.Min(currentHealth.Value + value, maxHealth);
    }

    public void GainOverhealth(int value)
    {
        currentHealth.Value = Mathf.Min(currentHealth.Value + value, maxOverhealth);
    }

    private void ToggleDeathComponents(bool previousValue, bool currentValue)
    {
        if (!networkObject.IsLocalPlayer)
        {
            foreach (GameObject model in models)
            {
                model.SetActive(currentValue);
            }

            GetComponent<Collider>().enabled = currentValue;
        }
        else
        {
            uiWeapon.weaponModel.SetActive(currentValue);
        }

        if (networkManager.IsHost && !currentValue)
        {
           // matchManager.Score(this);
        }

        if (!currentValue)
        {
            StartCoroutine(QueueRespawn());
        }
    }

    private IEnumerator QueueRespawn()
    {
        yield return new WaitForSeconds(1);
        spawnSystem.RequestMoveToSpawnPointClientRpc(gameObject);
        yield return new WaitForSeconds(1);
        RequestRespawnServerRpc();
    }

    [ServerRpc]
    private void RequestRespawnServerRpc()
    {
        currentHealth.Value = 75;
        alive.Value = true;
    }
}