//Import necessary namespaces
using Unity.Netcode;
using UnityEngine;

public abstract class Pickable : NetworkBehaviour
{
    //Set references to the necessary components
    private NetworkManager networkManager;
    private MeshRenderer meshRenderer;
    private Collider objCollider;

    //Initialize new NetworkVariable which will be utilized to synch its status (enabled or disabled) between all clients
    private NetworkVariable<bool> active = new NetworkVariable<bool>();
    //Initialize new variable which will be utilized to keep track of when the pickup was last used so it can handle its own spawn
    private float pickupTime;
    //Initialize variable which defines how many seconds the pickup will be disabled after being utilized
    [SerializeField] private float spawnCooldown; 

    private void Start()
    {
        //Obtain the relevant references
        networkManager = FindFirstObjectByType<NetworkManager>();
        meshRenderer = GetComponent<MeshRenderer>();
        objCollider = GetComponent<Collider>();

        //Set the initial state of the object to true
        active.Value = true;

        //Register the ToggleStatus method to the event of switching its active value
        active.OnValueChanged += ToggleStatus;
    }

    private void Update()
    {
        //If this is running in the Host (prevents this from occuring multiple times)
        if (networkManager.IsHost)
        {
            //If the pickup is not active and it's spawn cooldown has already been fulfilled
            if (!active.Value && Time.time - pickupTime >= spawnCooldown)
            {
                //Set the pickup as active
                active.Value = true;
            }
        }
    }

    //This method toggles the necessary components (model and collider) on and off depending on the status of the pickup
    private void ToggleStatus(bool previousValue, bool newValue)
    {
        //Toggle the model depending on the pickup's status
        meshRenderer.enabled = active.Value;
        //Toggle the collider depending on the pickup's status
        objCollider.enabled = active.Value;
    }

    //This method triggers once the object's collider is touched
    private void OnTriggerEnter(Collider other)
    {
        //If this is running in the host (prevents this from ocurring multiple times)
        if (networkManager.IsHost)
        {
            //Call the pickup method whose effects are dictated in the specific class which inherits from this one
            Pickup(other);
        }
    }

    //This abstract method is overriden by the specific class which inherits this and allows to easily create multiple pickups with specific effects
    protected abstract void Pickup(Collider collision);
}
