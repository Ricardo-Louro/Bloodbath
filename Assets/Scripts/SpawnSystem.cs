//Import necessary namespaces
using UnityEngine;
using Unity.Netcode;

public class SpawnSystem : MonoBehaviour
{
    //Set all the possible spawnPoints in the map
    [SerializeField] private Transform[] spawnPoints;

    //Only runs on the clients as they are authoritative in the transform of the player who corresponds to them. As such, in order for this value to not be immediately overwritten, this change needs to be made by the client itself. All other severs will be ignored.
    [ClientRpc]
    //This method will move the player's position to a random spawn point
    public void RequestMoveToSpawnPointClientRpc(GameObject player)
    {
        //Choose a random spawn point
        Transform randomSpawn = spawnPoints[Random.Range(0, spawnPoints.Length)];
        //Move the player to this spawn point's position
        player.transform.position = randomSpawn.position;
    }
}