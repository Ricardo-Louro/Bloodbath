using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    private NetworkManager networkManager;
    private NetworkVariable<float> timer = new NetworkVariable<float>(300f);

    private Dictionary<HealthSystem, int> scoreDictionary = new Dictionary<HealthSystem, int>();

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

    private List<HealthSystem> GetWinners()
    {
        List<HealthSystem> currentWinners = new List<HealthSystem>();
        foreach(HealthSystem healthSystem in scoreDictionary.Keys)
        {
            if (currentWinners.Count != 0)
            {
                if (scoreDictionary[healthSystem] > scoreDictionary[currentWinners[0]])
                {
                    currentWinners.Clear();
                    currentWinners.Add(healthSystem);
                }
                else if(scoreDictionary[healthSystem] == scoreDictionary[currentWinners[0]])
                {
                    currentWinners.Add(healthSystem);
                }
            }
            else
            {
                currentWinners.Add(healthSystem);
            }
        }
        return currentWinners;
    }

    public void Score(HealthSystem healthSystem)
    {
        scoreDictionary[healthSystem]++;
    }

    public void AddToDictionary(HealthSystem healthSystem)
    {
        scoreDictionary.Add(healthSystem, 0);
    }

    private void EndGame()
    {
        List<HealthSystem> winners = GetWinners();

        if(winners.Count == 1)
        {
            //Single Winner
        }
        else
        {
            //Draw between players
        }
    }
}