using UnityEngine;
using Unity.Netcode;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    [ClientRpc]
    public void RequestMoveToSpawnPointClientRpc(GameObject player)
    {
        Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        player.transform.position = randomSpawn.position;
    }
}