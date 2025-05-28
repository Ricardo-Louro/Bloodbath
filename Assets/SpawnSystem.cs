using UnityEngine;

public class SpawnSystem : MonoBehaviour
{
    [SerializeField] private Transform[] spawnPoints;

    public void MoveToSpawnPoint(GameObject player)
    {
        player.transform.position = spawnPoints[Random.Range(0, spawnPoints.Length)].position;
    }
}