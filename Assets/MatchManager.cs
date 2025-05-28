using Unity.Netcode;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private NetworkVariable<float> timer = new NetworkVariable<float>(300f);

    private void Update()
    {
        if (NetworkManager.Singleton.IsHost || NetworkManager.Singleton.IsServer)
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