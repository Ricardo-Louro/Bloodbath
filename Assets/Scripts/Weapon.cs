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



    private void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
        networkObject = GetComponentInParent<NetworkObject>();

        uiWeapon = FindFirstObjectByType<UIWeapon>();
    }

    private void Update()
    {
        if(networkObject.IsLocalPlayer)
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

        //DO THE NOISE

        //INFORM THE SERVER TO CALCULATE THE SHOOTING AND THE THE LINE AND NOISE
        CheckForHitServerRpc(Camera.main.transform.position, Camera.main.transform.forward);
    }

    private void ShootInServer()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out hit, Mathf.Infinity, layerMask))
        {
            DrawLineRenderer(uiWeapon.firingTransform.position, hit.point);
            HealthSystem healthSystem = hit.collider.GetComponent<HealthSystem>();
            if (healthSystem != null)
            {
                healthSystem.LoseHealth(damage);
            }
        }
        DrawLineInClientRpc(hit.point);
    }

    [ServerRpc]
    private void CheckForHitServerRpc(Vector3 origin, Vector3 direction)
    {
        HealthSystem healthSystem;
        RaycastHit hit;
        if(Physics.Raycast(origin, direction, out hit, Mathf.Infinity, layerMask))
        {
            healthSystem = hit.collider.GetComponent<HealthSystem>();
            if(healthSystem != null)
            {
                healthSystem.LoseHealth(damage);
            }
        }
        DrawLineRenderer(firingPosition.position, hit.point);
    }

    [ClientRpc]
    private void DrawLineInClientRpc(Vector3 hitpoint)
    {
        DrawLineRenderer(firingPosition.position, hitpoint);
    }

    private void DrawLineRenderer(Vector3 origin, Vector3 end)
    {
        lineRenderer.positionCount = 2;
        lineRenderer.SetPositions(new Vector3[]{ origin, end });
        StartCoroutine(ClearLineRenderer());
    }

    private IEnumerator ClearLineRenderer()
    {
        yield return new WaitForSeconds(fireCooldown);
        lineRenderer.positionCount = 0;
    }
}