using Unity.Netcode;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private NetworkManager networkManager;
    private NetworkVariable<float> timer = new NetworkVariable<float>(300f);

    private void Start()
    {
        networkManager = FindFirstObjectByType<NetworkManager>();
    }
    private void Update()
    {
        if (networkManager.IsHost || networkManager.IsServer)
        {
            timer.Value = Mathf.Max(timer.Value - Time.deltaTime, 0);

            if(timer.Value <= 0)
            {
                EndGame();
            }
        }
    }

    private void EndGame()
    {
        //END THE GAME
    }
}