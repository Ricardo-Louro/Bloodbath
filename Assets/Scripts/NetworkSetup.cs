//Import necessary namespaces
using System.Threading.Tasks;
using System;
using TMPro;
using Unity.Netcode.Transports.UTP;
using Unity.Netcode;
using Unity.Services.Authentication;
using Unity.Services.Core;
using Unity.Services.Relay.Models;
using UnityEngine.SceneManagement;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class NetworkSetup : MonoBehaviour
{
    //Internal class to store relay connection data
    public class RelayHostData
    {
        public string JoinCode;
        public string IPv4Address;
        public ushort Port;
        public Guid AllocationID;
        public byte[] AllocationIDBytes;
        public byte[] ConnectionData;
        public byte[] HostConnectionData;
        public byte[] Key;
    }

    //Set references to UI elements
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] TextMeshProUGUI textJoinCode;
    [SerializeField] TMP_InputField inputJoinCode;

    //Set transition to the Gameplay scene
    [SerializeField] private string sceneToLoad;

    //Set reference to the chosen Transport
    UnityTransport transport;
    //Bool which indicates if we are using the Relay Servers or not
    bool isRelay = false;
    //
    RelayHostData relayData;

    //Indicate the amount of players necessary to start the match
    int maxPlayers = 2;
    //Indicate whether the current client is the Host
    private bool isHost = false;

    //Variable which will store the relevant Join Code
    private string joinCode;

    void Awake()
    {
        //Obtain the value for the chosen Transport
        transport = GetComponent<UnityTransport>();

        //If the transport is using relay
        if (transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
        {
            //Set the isRelay value to true
            isRelay = true;
        }

        //If we are not using relay and there is a valid reference to the join code display
        if (!isRelay && textJoinCode != null)
        {
            //Disable the join code display
            textJoinCode.gameObject.SetActive(false);
        }
    }

    //Public class (to use with buttons) which allows one to start hosting a game
    public void HostGame()
    {
        //Indicate that this is the Host
        isHost = true;
        //Start the coroutine to host the game
        StartCoroutine(StartAsHostCR());
    }

    //Public class (to use with the input field) which allows one to join the game with the provided Join Code
    public void JoinGame()
    {
        //Indicate that this is not the host
        isHost = false;
        //Extract the provided join code
        joinCode = inputJoinCode.text.Trim();
        //Start the corountine to join a game
        StartCoroutine(StartAsClientCR());
    }

    //Method which handles starting a game as Host
    IEnumerator StartAsHostCR()
    {
        //Allow the network manager to handle scene management
        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = true;
        //Do not allow the network manager to automatically spawn a player prefab (we want to handle this manually)
        NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide = false;

        //Obtain reference to the NetworkManager
        var networkManager = GetComponent<NetworkManager>();
        //Enable the NetworkManager
        networkManager.enabled = true;
        //Enable the Transport
        transport.enabled = true;

        //Register the our OnClientConnected and OnClientedDisconnected to the relevant NetworkManager events
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        //Yield return null to be able to use this a coroutine
        yield return null;

        //If using relay
        if (isRelay)
        {
            //Login into the relay server
            var loginTask = Login();
            //Wait until this process has been completed
            yield return new WaitUntil(() => loginTask.IsCompleted);
            //If this process encounters an error, break out of the method
            if (loginTask.Exception != null) yield break;

            //Create the allocation (temporary reservation on Unity's Relay Servers that allows players to connect to each other without exposing their IP adresses)
            var allocationTask = CreateAllocationAsync(maxPlayers);
            //Wait until this process has been completed
            yield return new WaitUntil(() => allocationTask.IsCompleted);
            //If this process encounters an error, break out of the method
            if (allocationTask.Exception != null) yield break;

            //Store the allocation result
            Allocation allocation = allocationTask.Result;

            //Create new instance of RelayHostData to store information in
            relayData = new RelayHostData();
            //Iterate through every available Relay Server
            foreach (var endpoint in allocation.ServerEndpoints)
            {
                //Store the relevant IPv4 Adress 
                relayData.IPv4Address = endpoint.Host;
                //Store the relevant Port
                relayData.Port = (ushort)endpoint.Port;
                //Break out of this loop (we only need one valid Relay Server)
                break;
            }

            //Store all the other relevant obtained information
            relayData.AllocationID = allocation.AllocationId;
            relayData.AllocationIDBytes = allocation.AllocationIdBytes;
            relayData.ConnectionData = allocation.ConnectionData;
            relayData.Key = allocation.Key;

            //Try and obtain a valid Join Code to our current session
            var joinCodeTask = GetJoinCodeAsync(relayData.AllocationID);
            //Wait until this process has completed
            yield return new WaitUntil(() => joinCodeTask.IsCompleted);

            //Store the result of this process
            relayData.JoinCode = joinCodeTask.Result;

            //If there is a valid reference to the TMPro component
            if (textJoinCode != null)
            {
                //Change the text to display the join code
                textJoinCode.text = $"Join Code:\n{relayData.JoinCode}";
            }

            //Configure the transport with the available Relay Server's data 
            transport.SetRelayServerData(relayData.IPv4Address, relayData.Port, relayData.AllocationIDBytes,
                                         relayData.Key, relayData.ConnectionData);
        }

        //Try to start hosting and if unsucessful...
        if (!networkManager.StartHost())
        {
            //Inform that there was an error
            Debug.LogError("Host start failed");
        }
    }

    //Method which handles starting a game as Client
    IEnumerator StartAsClientCR()
    {
        //Allow the NetworkManager to handle scene management
        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = true;
        //Do not allow the NetworkManager to automatically spawn the player prefab (we want to do this manually)
        NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide = false;

        //Obtain reference to the NetworkManager
        var networkManager = GetComponent<NetworkManager>();
        //Enable the NetworkManager component
        networkManager.enabled = true;
        //Enable the Transport component
        transport.enabled = true;

        //Yield return null so this can be called as a coroutine
        yield return null;

        //If we are using relay
        if (isRelay)
        {
            //Login into the relay server
            var loginTask = Login();
            //Wait until this process has been completed
            yield return new WaitUntil(() => loginTask.IsCompleted);

            //Create the allocation (temporary reservation on Unity's Relay Servers that allows players to connect to each other without exposing their IP adresses)
            var joinAllocationTask = JoinAllocationAsync(joinCode);
            //Wait until this process has been completed
            yield return new WaitUntil(() => joinAllocationTask.IsCompleted);

            //Store the allocation result
            var allocation = joinAllocationTask.Result;

            //Create new instance of RelayHostData to store information in
            relayData = new RelayHostData();

            //Iterate through every available Relay Server
            foreach (var endpoint in allocation.ServerEndpoints)
            {
                //Store the relevant IPv4 Adress 
                relayData.IPv4Address = endpoint.Host;
                //Store the relevant Port
                relayData.Port = (ushort)endpoint.Port;
                //Break out of this loop (we only need one valid Relay Server)
                break;
            }

            //Store all the other relevant obtained information
            relayData.AllocationID = allocation.AllocationId;
            relayData.AllocationIDBytes = allocation.AllocationIdBytes;
            relayData.ConnectionData = allocation.ConnectionData;
            relayData.HostConnectionData = allocation.HostConnectionData;
            relayData.Key = allocation.Key;

            //Configure the transport with the available Relay Server's data 
            transport.SetRelayServerData(relayData.IPv4Address, relayData.Port,
                                         relayData.AllocationIDBytes, relayData.Key,
                                         relayData.ConnectionData, relayData.HostConnectionData);
        }

        //Try to start the client and if sucessful...
        if (networkManager.StartClient())
        {
            //Inform of the success
            Debug.Log("Client started");
        }
        //If unsuccessful...
        else
        {
            //Inform of the failure
            Debug.LogError("Failed to start client");
        }
    }

    //This method handles the necessary operations once a client has connected to the host
    private void OnClientConnected(ulong clientId)
    {
        //If this is the Host (so that the operations only happen once and by the authoritative server)
        if (isHost)
        {
            //Inform that a client has connected and which client it was
            Debug.Log($"Client {clientId} connected");

            //Check if we have reached the desired number of players to start the match (maxPlayers is set to 2 for Host and another Client)
            if (NetworkManager.Singleton.ConnectedClients.Count >= maxPlayers)
            {
                //Inform that the new game scene will be loaded
                Debug.Log("Loading game scene...");
                //Register the OnSceneLoadComplete method to the relevant event
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadComplete;
                //Have the NetworkManager load the Gameplay scene
                NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            }
        }
    }

    //This method handles spawning the players once the Gameplay scene has been transitioned into
    private void OnSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        //Inform that the scene has been loaded and this method will begin
        Debug.Log("launching OnSceneLoadComplete" + NetworkManager.Singleton.IsHost + NetworkManager.Singleton.IsServer);
        //If this is running on anything other than the host, end the method immediately
        if (!NetworkManager.Singleton.IsHost) return;

        //Inform that the players will be spawned
        Debug.Log("Scene load complete. Spawning players...");

        //Iterate through every connected client (the Host is a client too)
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            //Spawn a player prefab and assign it to the corresponding client
            SpawnPlayer(client.ClientId);
            //Inform that this has been successfully performed
            Debug.Log("Spawned player " + client.ClientId);
        }

        //Unsubscribe from the event after use so that it does not occur again if we transition to another scene (ie: back to the Main Menu)
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadComplete;
    }

    //This method informs that a client has been disconnected
    private void OnClientDisconnected(ulong clientId)
    {
        //Inform that a client could be disconnected
        Debug.Log($"Client {clientId} disconnected");
        //For a full project with more development time, a warning that informs the other players via UI could be put here as well as a way to boot back to the Main Menu
    }

    //This method handles the Login task
    private async Task<bool> Login()
    {
        //Attempt to perform the login process
        try
        {
            //Wait for the Unity Gaming Services (UGS) to be launched
            await UnityServices.InitializeAsync();
            //If we are not signed in yet
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                //Wait until we sign in anonymously
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            //Return the true value and exit the method
            return true;
        }
        //If the login process ends in an error
        catch (Exception e)
        {
            //Inform which error it was
            Debug.LogError("Login error: " + e);
            //Exit out of the method
            throw;
        }
    }

    //This method will create the allocation (temporary reservation on Unity's Relay Servers that allows players to connect to each other without exposing their IP adresses)
    private async Task<Allocation> CreateAllocationAsync(int maxPlayers)
    {
        //Attempt to create the allocation
        try
        {
            //Wait until the allocation is created for the desired number of players (in this build, this would be 2)
            return await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(maxPlayers);
        }
        //If it encounters an error
        catch (Exception e)
        {
            //Inform which error it was
            Debug.LogError("Allocation creation failed: " + e);
            //Exit out of the method
            throw;
        }
    }

    //This method will join the created allocation
    private async Task<JoinAllocation> JoinAllocationAsync(string joinCode)
    {
        //Attempt to join the allocation
        try
        {
            //Wait until it successfully joins the allocation with the corresponding Join Code
            return await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        //If it encounters an error
        catch (Exception e)
        {
            //Inform which error it was
            Debug.LogError("Join allocation failed: " + e);
            //Exit out of the method
            throw;
        }
    }
    
    //This method will attempt to obtain a Join Code that corresponds to the connect allocation
    private async Task<string> GetJoinCodeAsync(Guid allocationID)
    {
        //Attempt to obtain the Join Code
        try
        {
            //Wait until it successfully obtains the Join Code corresponding to the current allocation
            return await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocationID);
        }
        //If it encounters an error
        catch (Exception e)
        {
            //Inform which error it was
            Debug.LogError("Join code retrieval failed: " + e);
            //Exit out of the method
            throw;
        }
    }

    //This method will spawn a player prefab and link it with a corresponding client
    private void SpawnPlayer(ulong clientId)
    {
        //If this is not the host (ensures this runs only on the authoritative server and also only once)
        if (!NetworkManager.Singleton.IsHost)
        {
            //Exit out of the method without doing anything
            return;
        }

        //Obtain a (0,0,0) vector for position
        Vector3 spawnPos = Vector3.zero;
        //Obtain a (0,0,0,0) Quaternion for rotation
        Quaternion spawnRot = Quaternion.identity;

        //Instantiate a player prefab with the provided position and rotation (it immediately gets changed in the player's 
        var playerInstance = Instantiate(playerPrefab, spawnPos, spawnRot);
        //Obtain a reference to the Network Object component connected to the player prefab
        var networkObject = playerInstance.GetComponent<NetworkObject>();
        //Have the corresponding client take ownership to that prefab
        networkObject.SpawnAsPlayerObject(clientId, true);
    }
}