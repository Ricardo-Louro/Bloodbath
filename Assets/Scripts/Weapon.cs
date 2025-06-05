using System.Collections;
using Unity.Netcode;
using UnityEngine;

public class Weapon : NetworkBehaviour
{
    private NetworkManager networkManager;
    private NetworkObject networkObject;
    [SerializeField] private LineRenderer lineRenderer;

    private UIWeapon uiWeapon;

    [SerializeField] private Transform firingPosition;

    [SerializeField] private LayerMask layerMask;

    [SerializeField] private int damage;
    [SerializeField] private float fireCooldown;
    private float lastTimeShot = 0;

    [SerializeField] private AudioClip audioClip;
    private AudioSource audioSource;

    private HealthSystem healthSystem;



    private void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        networkObject = GetComponentInParent<NetworkObject>();

        uiWeapon = FindFirstObjectByType<UIWeapon>();

        audioSource = GetComponent<AudioSource>();

        healthSystem = GetComponentInParent<HealthSystem>();
    }

    private void Update()
    {
        if(networkObject.IsLocalPlayer && healthSystem.alive.Value)
        {
            if(Input.GetKeyDown(KeyCode.Mouse0) && Time.time - lastTimeShot >= fireCooldown)
            {
                lastTimeShot = Time.time;
                if(networkManager.IsHost)
                {
                    ShootInServer();
                }
                else
                {
                    ShootInClient();
                }
            }
        }
    }

    private void ShootInClient()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, layerMask))
        {
            DrawLineRenderer(uiWeapon.firingTransform.position, hit.point);
        }
        else
        {
            DrawLineRenderer(uiWeapon.firingTransform.position, uiWeapon.firingTransform.position + Camera.main.transform.forward * 50);
        }

        //INFORM THE SERVER TO CALCULATE THE SHOOTING AND THE THE LINE AND NOISE
        CheckForHitServerRpc(Camera.main.transform.position, Camera.main.transform.forward);
    }

    private void ShootInServer()
    {
        HealthSystem healthSystem;
        RaycastHit[] raycastHits = Physics.RaycastAll(Camera.main.transform.position, Camera.main.transform.forward, Mathf.Infinity, layerMask);
        OrderRaycastHitsByClosest(raycastHits, Camera.main.transform.position);

        foreach(RaycastHit hit in raycastHits)
        {
            if(hit.collider.gameObject != transform.parent.gameObject)
            {
                healthSystem = hit.collider.GetComponent<HealthSystem>();
            
                if(healthSystem != null)
                {
                    healthSystem.LoseHealth(damage);
                }

                DrawLineRenderer(uiWeapon.firingTransform.position, hit.point);
                DrawLineInClientRpc(hit.point);
                break;
            }
        }

        if (raycastHits.Length == 0)
        {
            DrawLineRenderer(uiWeapon.firingTransform.position, uiWeapon.transform.position + Camera.main.transform.forward * 50);
            DrawLineInClientRpc(uiWeapon.transform.position + Camera.main.transform.forward * 50);
        }
    }

    [ServerRpc]
    private void CheckForHitServerRpc(Vector3 origin, Vector3 direction)
    {
        HealthSystem healthSystem;
        RaycastHit[] raycastHits = Physics.RaycastAll(origin, direction, Mathf.Infinity, layerMask);
        OrderRaycastHitsByClosest(raycastHits, origin);

        foreach(RaycastHit hit in raycastHits)
        {
            if(hit.collider.gameObject != transform.parent.gameObject)
            {
                healthSystem = hit.collider.GetComponent<HealthSystem>();
            
                if(healthSystem != null)
                {
                    healthSystem.LoseHealth(damage);
                }

                DrawLineRenderer(firingPosition.position, hit.point);
                break;
            }
        }

        if (raycastHits.Length == 0)
        {
            DrawLineRenderer(firingPosition.position, direction * 50);
        }
    }

    [ClientRpc]
    private void DrawLineInClientRpc(Vector3 hitpoint)
    {
        if(!networkManager.IsHost)
        {
            DrawLineRenderer(firingPosition.position, hitpoint);
        }
    }

    private void DrawLineRenderer(Vector3 origin, Vector3 end)
    {
        audioSource.PlayOneShot(audioClip);
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[]{ origin, end });
        StartCoroutine(ClearLineRenderer());
    }

    private RaycastHit[] OrderRaycastHitsByClosest(RaycastHit[] raycastHits, Vector3 originPosition)
    {
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
        return raycastHits;
    }

    private IEnumerator ClearLineRenderer()
    {
        yield return new WaitForSeconds(fireCooldown);
        lineRenderer.positionCount = 0;
    }
}