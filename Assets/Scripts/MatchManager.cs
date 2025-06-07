using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class MatchManager : MonoBehaviour
{
    //Set reference to the NetworkManager
    private NetworkManager networkManager;
    //Initialize NetworkVariable of a timer that should be synched between all players
    private NetworkVariable<float> timer = new NetworkVariable<float>(300f);

    //Initialize Dictionary variable which will contain references to both players and their scores
    private Dictionary<HealthSystem, int> scoreDictionary = new Dictionary<HealthSystem, int>();

    private void Start()
    {
        //Assign the value to the NetworkManager reference
        networkManager = FindFirstObjectByType<NetworkManager>();
    }

    private void Update()
    {
        //This code will only run on the Host
        if (networkManager.IsHost)
        {
            //Tick down the timer
            timer.Value = Mathf.Max(timer.Value - Time.deltaTime, 0);

            //If the timer has reached 0
            if(timer.Value <= 0)
            {
                //End the game
                EndGame();
            }
        }
    }

    //This method checks which player (if any) has won and returns them in a list
    private List<HealthSystem> GetWinners()
    {
        //Initialize the list of the winners
        List<HealthSystem> currentWinners = new List<HealthSystem>();
        //Iterate through every HealthSystem inside the dictionary
        foreach(HealthSystem healthSystem in scoreDictionary.Keys)
        {
            //If there are any players inside the winner's list already...
            if (currentWinners.Count != 0)
            {
                //If the score of the current player (referenced by their HealthSystem) surpasses that of the current winner...
                if (scoreDictionary[healthSystem] > scoreDictionary[currentWinners[0]])
                {
                    //Clear the winner's list
                    currentWinners.Clear();
                    //Insert the current player into the winner's list
                    currentWinners.Add(healthSystem);
                }
                //If the score of the current player is the same as of the current winner...
                else if(scoreDictionary[healthSystem] == scoreDictionary[currentWinners[0]])
                {
                    //Add the current player into the winner's list
                    currentWinners.Add(healthSystem);
                }
            }
            //If there are no players inside the winner's list...
            else
            {
                //Add the current player into the winner's list...
                currentWinners.Add(healthSystem);
            }
        }
        //Return the winner's list
        return currentWinners;
    }

    //This method increases a specific player's score
    public void Score(HealthSystem healthSystem)
    {
        //Increase the provided player's score
        scoreDictionary[healthSystem]++;
    }

    //This method inserts a new player into the dictionary
    public void AddToDictionary(HealthSystem healthSystem)
    {
        //Add new player to the dictionary with a starting score of 0
        scoreDictionary.Add(healthSystem, 0);
    }

    //This method would control the end game to display the correct UI with the winners and their score
    private void EndGame()
    {
        //Check the winners and retrieve the winner's list
        List<HealthSystem> winners = GetWinners();

        //If there is only one winner
        if(winners.Count == 1)
        {
            //Single Winner
        }
        //If there are multiple winners
        else
        {
            //Draw between players
        }
    }
}