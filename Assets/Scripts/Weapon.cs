//Importing necessary namespaces
using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    //Set references to important network components
    private NetworkManager networkManager;
    private NetworkObject networkObject;

    //Set reference to the LineRenderer component to use for the railgun shots
    [SerializeField] private LineRenderer lineRenderer;

    //Set reference to the uiWeapon so I can use the firingPosition for the LineRenderer
    private UIWeapon uiWeapon;

    //Set reference to the uiWeapon firing position
    [SerializeField] private Transform firingPosition;

    //Set LayerMask so that the firing raycast ignores Triggers (potential pickables wouldn't register as valid shot targets)
    [SerializeField] private LayerMask layerMask;

    //Set variable for the weapon's damage output
    [SerializeField] private int damage;
    //Set variable to define how long the weapon takes between each shot
    [SerializeField] private float fireCooldown;
    //Set variable which will be used to calculate if the weapon is still on cooldown
    private float lastTimeShot = 0;

    //Set reference to the audio clip for firing the gun
    [SerializeField] private AudioClip audioClip;
    //Set reference to the AudioSource component
    private AudioSource audioSource;

    //Set reference to the player's HealthSystem so that they cannot fire the weapon while dead
    private HealthSystem healthSystem;



    private void Start()
    {
        //Set values to the relevant components 
        networkManager = FindFirstObjectByType<NetworkManager>();
        networkObject = GetComponentInParent<NetworkObject>();
        uiWeapon = FindFirstObjectByType<UIWeapon>();
        audioSource = GetComponent<AudioSource>();
        healthSystem = GetComponentInParent<HealthSystem>();
    }

    private void Update()
    {
        //If this is the local player and they are currently alive
        if(networkObject.IsLocalPlayer && healthSystem.alive.Value)
        {
            //If they left mouse button is pressed and the weapon is not in cooldown
            if(Input.GetKeyDown(KeyCode.Mouse0) && Time.time - lastTimeShot >= fireCooldown)
            {
                //Update the last time the weapon was fired (will enter cooldown)
                lastTimeShot = Time.time;
                
                //If the local player is the host
                if(networkManager.IsHost)
                {
                    //Fire the weapon as the server
                    ShootInServer();
                }
                //If the local player in a non-host client
                else
                {
                    //Fire the weapon as a client
                    ShootInClient();
                }
            }
        }
    }

    //This method handles firing the weapon in a client
    private void ShootInClient()
    {
        //Set variable to receive the information related to the hit
        RaycastHit hit;

        //Utilize a raycast to see if the shot hits something
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, layerMask))
        {
            //If it does, set a line renderer from the firing position to the point where the hit occured.
            DrawLineRenderer(uiWeapon.firingTransform.position, hit.point);
        }
        //If the shot hit nothing (ie: fires toward the Skybox)
        else
        {
            //Set a line renderer from the UI firing position to a point very far away in the direction that the shot went to (the point is far away enough to give the impression of the weapon's infinite range). This is the 1st Person View of the shot.
            DrawLineRenderer(uiWeapon.firingTransform.position, uiWeapon.firingTransform.position + Camera.main.transform.forward * 50);
        }

        //Inform the server to calculate the shooting, perform the noise and draw the line from the third person perspective (needs to come from the gun model rather than the ui one)
        CheckForHitServerRpc(Camera.main.transform.position, Camera.main.transform.forward);
    }

    //This method handles firing the weapon in a server
    private void ShootInServer()
    {
        //Set variable to receive the information related to the health system of the target
        HealthSystem healthSystem;
        //Receive information regarding everything the raycast hits in an unordered array
        RaycastHit[] raycastHits = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, Mathf.Infinity, layerMask);
        //Order the array of hits by closest to longest distance from the origin of the shot. Retains the same reference (arrays are reference types)
        OrderRaycastHitsByClosest(raycastHits, Camera.main.transform.position);

        //Iterate through every hit in the array of hits
        foreach(RaycastHit hit in raycastHits)
        {
            //If the hit is not the player who fired
            if(hit.collider.gameObject != transform.parent.gameObject)
            {
                //Obtain reference to the hit object's HealthSystem
                healthSystem = hit.collider.GetComponent<HealthSystem>();
            
                //If it has a HealthSystem (meaning it's a Player)
                if(healthSystem != null)
                {
                    //Have the hit player lose health equal to the weapon's damage
                    healthSystem.LoseHealth(damage);
                }

                //Set the line renderer from the UI Weapon's firing position up to the point where the hit occured. This is the 1st Person View of the shot.
                DrawLineRenderer(uiWeapon.firingTransform.position, hit.point);
                //Inform the non-host clients to draw the line from the 3rd Person Point of View.
                DrawLineInClientRpc(hit.point);
                //Exit out of the loop as we only want the first valid hit
                break;
            }
        }

        //If nothing got hit (ie: firing at the Skybox)
        if (raycastHits.Length == 0)
        {
            //Set the line renderer from the UI Weapon's firing position to a point in the correct direction much further away. This handles the 1st Person View of the shot.
            DrawLineRenderer(uiWeapon.firingTransform.position, uiWeapon.transform.position + Camera.main.transform.forward * 50);
            //Inform the non-host clients to set the line renderer up to a point very far away in the shot direction. This handles the 3rd Person View of the shot.
            DrawLineInClientRpc(uiWeapon.transform.position + Camera.main.transform.forward * 50);
        }
    }

    //This only occurs in the Server
    [ServerRpc]
    //This method informs the server to check if something hit and to draw the line renderer.
    private void CheckForHitServerRpc(Vector3 origin, Vector3 direction)
    {
        //Set the reference to the HealthSystem
        HealthSystem healthSystem;
        //Obtain an unordored array of all the hits from the shot raycast
        RaycastHit[] raycastHits = Physics.RaycastAll(origin, direction, Mathf.Infinity, layerMask);
        //Order all the hits from closest to furthest from the shot origin maintaining the same reference.
        OrderRaycastHitsByClosest(raycastHits, origin);

        //Iterate through all the hits
        foreach(RaycastHit hit in raycastHits)
        {
            //If the hit is not the player who shot
            if(hit.collider.gameObject != transform.parent.gameObject)
            {
                //Obtain the reference to the HealthSystem of the hit object
                healthSystem = hit.collider.GetComponent<HealthSystem>();
            
                //If the hit object contains a HealthSystem (meaning it's a Player)
                if(healthSystem != null)
                {
                    //Reduce the hit player's health by the weapon's damage
                    healthSystem.LoseHealth(damage);
                }

                //Set the line renderer from the firing position to the point where the hit occurred. This handles the 3rd Person View of the shot. 
                DrawLineRenderer(firingPosition.position, hit.point);
                //Break out of the loop as we only need the first hit
                break;
            }
        }

        //If the raycast hit nothing (ie: shot at the Skybox)
        if (raycastHits.Length == 0)
        {
            //Set line renderer from the firing position to a point very far away in the shot direction. This handles the 3rd Person View of the shot.
            DrawLineRenderer(firingPosition.position, direction * 50);
        }
    }

    //This only occurs in the Non-Host Clients
    [ClientRpc]
    //This method informs the non-host clients to draw the line renderer
    private void DrawLineInClientRpc(Vector3 hitpoint)
    {
        //If it is a non-host client
        if(!networkManager.IsHost)
        {
            //This method sets the line renderer from the firing position to the provided position. This handles the 3rd Person View of the shot.
            DrawLineRenderer(firingPosition.position, hitpoint);
        }
    }
    
    //This method sets the line renderer to the two provided points, plays the audio clip of the shot and starts the coroutine of clearing the line renderer.
    private void DrawLineRenderer(Vector3 origin, Vector3 end)
    {
        //Play the audio clip of the shot
        audioSource.PlayOneShot(audioClip);
        //Set the line renderer position to 2 points.
        lineRenderer.positionCount = 2;
        //Set the line renderer's points to the two desired positions
        lineRenderer.SetPositions(new Vector3[]{ origin, end });
        //Call the coroutine to clear the line renderer after a few seconds (equal to the shot cooldown time)
        StartCoroutine(ClearLineRenderer());
    }

    //This method orders the provided array using a Selection Sort algorythm. This edits the reference type array so after calling this method, using the same reference as before calling the method.
    private RaycastHit[] OrderRaycastHitsByClosest(RaycastHit[] raycastHits, Vector3 originPosition)
    {
        //Selection sort algorythm 
        for (int hitIndex = 0; hitIndex < raycastHits.Length; hitIndex++)
        {
            int minIndex = hitIndex;

            for (int selectionIndex = hitIndex + 1; selectionIndex < raycastHits.Length; selectionIndex++)
            {
                if ((raycastHits[selectionIndex].point - originPosition).magnitude <
                    (raycastHits[minIndex].point - originPosition).magnitude)
                {
                    minIndex = selectionIndex;
                }
            }
            
            if(minIndex != hitIndex)
            {
                RaycastHit tempHit = raycastHits[hitIndex];
                raycastHits[hitIndex] = raycastHits[minIndex];
                raycastHits[minIndex] = tempHit;
            }
        }
        //Returns the raycast array (not necessary in the current case because it alters the reference type but decided to include in case it was ever needed)
        return raycastHits;
    }

    //This coroutine clears the line renderer after a short delay equal to the gun cooldown
    private IEnumerator ClearLineRenderer()
    {
        //Wait for the gun cooldown duration
        yield return new WaitForSeconds(fireCooldown);
        //Set the line renderer points to 0 (effectively clearing the line renderer)
        lineRenderer.positionCount = 0;
    }
}