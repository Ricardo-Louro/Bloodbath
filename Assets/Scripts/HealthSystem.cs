//Import necessary namespaces
using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class HealthSystem : NetworkBehaviour
{
    //Set reference to the NetworkManager
    private NetworkManager networkManager;
    //Set reference to the NetworkObject
    private NetworkObject networkObject;
    //Set reference to the SpawnSystem
    private SpawnSystem spawnSystem;
    
    //Set reference to the MatchManager. This is commented as this system is not implemented due to time constraints.
    //private MatchManager matchManager;

    //Initialize the set the value to the NetworkVariable alive which informs whether the player is alive or dead
    public NetworkVariable<bool> alive = new NetworkVariable<bool>(true);
    //Initialize the set the value to the NetworkVariable currentHealth which can be lowered or raised and is used to check whether the player needs to die
    public NetworkVariable<int> currentHealth = new NetworkVariable<int>(75);

    //Initialize variable which sets the maximum health which can be recovered by normal healing items
    public int maxHealth { get; private set; } = 100;
    //Initialize variable which sets the maximum health which can be recovered by overheal items (like a potential MegaHealth)
    private int maxOverhealth = 200;

    //Reference to the variable models which need to be toggled on or off depending on the alive status
    [SerializeField] private GameObject[] models;
    //Reference to the player collider which needs to be turned off during death and back on after respawn
    private Collider playerCollider;
    //Reference to the UI Weapon so that we can disable it during death and re-enable during respawn
    private UIWeapon uiWeapon;

    private void Start()
    {
        //Set the necessary references to the variables
        networkManager = FindFirstObjectByType<NetworkManager>();
        networkObject = GetComponent<NetworkObject>();
        spawnSystem = FindFirstObjectByType<SpawnSystem>();
        uiWeapon = FindFirstObjectByType<UIWeapon>();
        playerCollider = GetComponent<Collider>();

        //matchManager = FindFirstObjectByType<MatchManager>();  <--- UNUSED. Not fully implemented due to time restraints.
        //matchManager.AddToDictionary(this);   <--- UNUSED. Not fully implemented due to time restraints.

        //Move the player to a randomized spawn point
        spawnSystem.RequestMoveToSpawnPointClientRpc(gameObject);
    }

    //Gets called when the NetworkObject gets spawned, message handlers are ready to be registered and the network is setup.
    public override void OnNetworkSpawn()
    {
        //Register the CheckForDeath to the event of changing the currentHealth
        currentHealth.OnValueChanged += CheckForDeath;
        //Register the ToggleDeathComponents to the event of changing the alive value
        alive.OnValueChanged += ToggleDeathComponents;
    }

    //Gets called when the NetworkObject gets despawned.Is called both on the server and clients.
    public override void OnNetworkDespawn()
    {
        //Unregister the CheckForDeath to the event of changing the currentHealth
        currentHealth.OnValueChanged -= CheckForDeath;
        //Unregister the ToggleDeathComponents to the event of changing the alive value
        alive.OnValueChanged -= ToggleDeathComponents;
    }

    //Method that allows to check if the Player died and to toggle the alive value 
    private void CheckForDeath(int previousValue, int currentValue)
    {
        //If the player's current health reaches a value below or equal to zero and this is running in the Host (to avoid duplicate triggers of the OnValueChanged event)
        if (currentHealth.Value <= 0 && networkManager.IsHost)
        {
            //Set the alive value to false
            alive.Value = false;
        }
    }

    //Method to be called when needed to remove the player's health by the set amount
    public void LoseHealth(int value)
    {
        //Reduce the player's current health by a specified amount. The end value cannot be lower than zero.
        currentHealth.Value = Mathf.Max(currentHealth.Value - value, 0);
    }

    //Method to be called when needed to gain the player's health by the set amount
    public void GainHealth(int value)
    {
        //Increase the player's current health by a specified amount. The end value cannot exceed the maximum health.
        currentHealth.Value = Mathf.Min(currentHealth.Value + value, maxHealth);
    }
    //Method to be called when needed to gain the player's overhealth by the set amount
    public void GainOverhealth(int value)
    {
        //Increase the player's current healthy by a specified amount. The end value cannot exceed the maximum overhealth.
        currentHealth.Value = Mathf.Min(currentHealth.Value + value, maxOverhealth);
    }

    //This method toggles the selected components depending on the current value of alive. This is called whenever alive changes its current value.
    private void ToggleDeathComponents(bool previousValue, bool currentValue)
    {
        //If this is not the local player (only runs on everyone elses clients as these components are always off to support first person view)
        if (!networkObject.IsLocalPlayer)
        {
            //Iterate through all the models
            foreach (GameObject model in models)
            {
                //Toggle its status (off when dead, on when alive)
                model.SetActive(currentValue);
            }

            //Toggle the colliders' status (off when dead, on when alive)
            GetComponent<Collider>().enabled = currentValue;
        }
        //If this is the local player
        else
        {
            //Toggle the UI Weapon's status (off when dead, on when alive)
            uiWeapon.weaponModel.SetActive(currentValue);
        }

        /*
        if (networkManager.IsHost && !currentValue)
        {
           //INSERT WAY TO HAVE THE OTHER PLAYER SCORE
        }
        */

        //If the player is dead
        if (!currentValue)
        {
            //Queue the player's respawn
            StartCoroutine(QueueRespawn());
        }
    }

    //Coroutine which handles the player's respawn
    private IEnumerator QueueRespawn()
    {
        //Waits for 1 second
        yield return new WaitForSeconds(1);
        //Tells the player's client to move to a random spawn point (only the corresponding client can control the local player's position due to the Network Transform) 
        spawnSystem.RequestMoveToSpawnPointClientRpc(gameObject);
        //Wait an additional second to prevent spaw camping (the player can still move and look around but cannot shoot)
        yield return new WaitForSeconds(1);
        //Respawn the player
        RequestRespawnServerRpc();
    }

    //ServerRpc which respawns the player by giving it health and the alive status
    [ServerRpc]
    private void RequestRespawnServerRpc()
    {
        //Set current health value
        currentHealth.Value = 75;
        //Set the alive status to true
        alive.Value = true;
    }
}