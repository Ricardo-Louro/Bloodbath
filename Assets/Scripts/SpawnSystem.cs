using UnityEngine;
using Unity.Netcode;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    [ClientRpc]
    public void RequestMoveToSpawnPointClientRpc(GameObject player)
    {
        player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }
}