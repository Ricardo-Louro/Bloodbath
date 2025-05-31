using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class HealthSystem : MonoBehaviour
{
    private NetworkManager networkManager;
    private NetworkObject networkObject;
    private SpawnSystem spawnSystem;
    private MatchManager matchManager;

    public NetworkVariable<bool> alive { get; private set; } = new NetworkVariable<bool>();
    public NetworkVariable<int> currentHealth { get; private set; } = new NetworkVariable<int>();
    public int maxHealth { get; private set; } = 100;
    private int maxOverhealth = 200;

    [SerializeField] private GameObject[] models;
    private Collider playerCollider;

    private void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        networkObject = GetComponent<NetworkObject>();
        spawnSystem = FindFirstObjectByType<SpawnSystem>();
        matchManager = FindFirstObjectByType<MatchManager>();

        matchManager.AddToDictionary(this);

        alive.Value = true;
        currentHealth.Value = 75;


        currentHealth.OnValueChanged += CheckForDeath;

        alive.OnValueChanged += ToggleDeathComponents;

        playerCollider = GetComponent<Collider>();
    }

    private void CheckForDeath(int previousValue, int currentValue)
    {
        if (currentHealth.Value <= 0)
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
        if(!networkObject.IsLocalPlayer)
        {
            foreach(GameObject model in models)
            {
                model.SetActive(alive.Value);
            }
            GetComponent<Collider>().enabled = alive.Value;
            matchManager.Score(this);
        }

        if(networkManager.IsServer && !alive.Value)
        {
            //UPDATE SCORE
            //GIB EXPLOSION
            StartCoroutine(QueueRespawn());
        }
    }

    private IEnumerator QueueRespawn()
    {
        yield return new WaitForSeconds(3);

        alive.Value = true;
        currentHealth.Value = maxHealth;

        spawnSystem.MoveToSpawnPoint(this.gameObject);
    }
}