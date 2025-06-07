using System.Runtime.InteropServices;
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

    [SerializeField] private GameObject playerPrefab;
    [SerializeField] TextMeshProUGUI textJoinCode;
    [SerializeField] TMP_InputField inputJoinCode;

    [SerializeField] private string sceneToLoad = "GameScene";

    UnityTransport transport;
    bool isRelay = false;
    RelayHostData relayData;

    int maxPlayers = 1;
    private bool isHost = false;

    private string joinCode;

    void Awake()
    {
        transport = GetComponent<UnityTransport>();
        if (transport.Protocol == UnityTransport.ProtocolType.RelayUnityTransport)
            isRelay = true;

        if (!isRelay && textJoinCode != null)
            textJoinCode.gameObject.SetActive(false);
    }

    // === PUBLIC ENTRY POINTS FOR UI ===
    public void HostGame()
    {
        isHost = true;
        StartCoroutine(StartAsHostCR());
    }

    public void JoinGame()
    {
        isHost = false;
        joinCode = inputJoinCode.text.Trim();
        StartCoroutine(StartAsClientCR());
    }

    // === SERVER LOGIC ===
    IEnumerator StartAsHostCR()
    {
        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = true;
        NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide = false;

        SetWindowTitle("Bloodbath (host mode)");

        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        transport.enabled = true;

        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        yield return null;

        if (isRelay)
        {
            var loginTask = Login();
            yield return new WaitUntil(() => loginTask.IsCompleted);

            if (loginTask.Exception != null) yield break;

            var allocationTask = CreateAllocationAsync(maxPlayers);
            yield return new WaitUntil(() => allocationTask.IsCompleted);

            if (allocationTask.Exception != null) yield break;

            Allocation allocation = allocationTask.Result;

            relayData = new RelayHostData();
            foreach (var endpoint in allocation.ServerEndpoints)
            {
                relayData.IPv4Address = endpoint.Host;
                relayData.Port = (ushort)endpoint.Port;
                break;
            }

            relayData.AllocationID = allocation.AllocationId;
            relayData.AllocationIDBytes = allocation.AllocationIdBytes;
            relayData.ConnectionData = allocation.ConnectionData;
            relayData.Key = allocation.Key;

            var joinCodeTask = GetJoinCodeAsync(relayData.AllocationID);
            yield return new WaitUntil(() => joinCodeTask.IsCompleted);

            relayData.JoinCode = joinCodeTask.Result;

            if (textJoinCode != null)
            {
                textJoinCode.text = $"Join Code:\n{relayData.JoinCode}";
                textJoinCode.gameObject.SetActive(true);
            }

            transport.SetRelayServerData(relayData.IPv4Address, relayData.Port, relayData.AllocationIDBytes,
                                         relayData.Key, relayData.ConnectionData);
        }

        if (!networkManager.StartHost())
        {
            Debug.LogError("Host start failed");
        }
    }

    // === CLIENT LOGIC ===
    IEnumerator StartAsClientCR()
    {
        NetworkManager.Singleton.NetworkConfig.EnableSceneManagement = true;
        NetworkManager.Singleton.NetworkConfig.AutoSpawnPlayerPrefabClientSide = false;

        SetWindowTitle("Bloodbath (client mode)");

        var networkManager = GetComponent<NetworkManager>();
        networkManager.enabled = true;
        transport.enabled = true;

        yield return null;

        if (isRelay)
        {
            var loginTask = Login();
            yield return new WaitUntil(() => loginTask.IsCompleted);

            var joinAllocationTask = JoinAllocationAsync(joinCode);
            yield return new WaitUntil(() => joinAllocationTask.IsCompleted);

            relayData = new RelayHostData();
            var allocation = joinAllocationTask.Result;

            foreach (var endpoint in allocation.ServerEndpoints)
            {
                relayData.IPv4Address = endpoint.Host;
                relayData.Port = (ushort)endpoint.Port;
                break;
            }

            relayData.AllocationID = allocation.AllocationId;
            relayData.AllocationIDBytes = allocation.AllocationIdBytes;
            relayData.ConnectionData = allocation.ConnectionData;
            relayData.HostConnectionData = allocation.HostConnectionData;
            relayData.Key = allocation.Key;

            transport.SetRelayServerData(relayData.IPv4Address, relayData.Port,
                                         relayData.AllocationIDBytes, relayData.Key,
                                         relayData.ConnectionData, relayData.HostConnectionData);
        }

        if (networkManager.StartClient())
        {
            Debug.Log("Client started");
        }
        else
        {
            Debug.LogError("Failed to start client");
        }
    }

    // === CONNECTION EVENTS ===
    private void OnClientConnected(ulong clientId)
    {
        if (isHost)
        {
            Debug.Log($"Client {clientId} connected");

            // Only transition when at least 2 players (1 host + 1 client)
            if (NetworkManager.Singleton.ConnectedClients.Count >= 2)
            {
                Debug.Log("Loading game scene...");
                NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoadComplete;
                NetworkManager.Singleton.SceneManager.LoadScene(sceneToLoad, LoadSceneMode.Single);
            }
        }
    }

    private void OnSceneLoadComplete(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    {
        Debug.Log("launching OnSceneLoadComplete" + NetworkManager.Singleton.IsHost + NetworkManager.Singleton.IsServer);
        if(!NetworkManager.Singleton.IsHost) return;

        Debug.Log("Scene load complete. Spawning players...");

        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SpawnPlayer(client.ClientId);
            Debug.Log("Spawned player " + client.ClientId);
        }

        // Unsubscribe after use
        NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoadComplete;
    }

    private void OnClientDisconnected(ulong clientId)
    {
        Debug.Log($"Client {clientId} disconnected");
    }

    // === RELAY SERVICE WRAPPERS ===
    private async Task<bool> Login()
    {
        try
        {
            await UnityServices.InitializeAsync();
            if (!AuthenticationService.Instance.IsSignedIn)
            {
                await AuthenticationService.Instance.SignInAnonymouslyAsync();
            }
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError("Login error: " + e);
            throw;
        }
    }

    private async Task<Allocation> CreateAllocationAsync(int maxPlayers)
    {
        try
        {
            return await Unity.Services.Relay.RelayService.Instance.CreateAllocationAsync(maxPlayers);
        }
        catch (Exception e)
        {
            Debug.LogError("Allocation creation failed: " + e);
            throw;
        }
    }

    private async Task<JoinAllocation> JoinAllocationAsync(string joinCode)
    {
        try
        {
            return await Unity.Services.Relay.RelayService.Instance.JoinAllocationAsync(joinCode);
        }
        catch (Exception e)
        {
            Debug.LogError("Join allocation failed: " + e);
            throw;
        }
    }

    private async Task<string> GetJoinCodeAsync(Guid allocationID)
    {
        try
        {
            return await Unity.Services.Relay.RelayService.Instance.GetJoinCodeAsync(allocationID);
        }
        catch (Exception e)
        {
            Debug.LogError("Join code retrieval failed: " + e);
            throw;
        }
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!NetworkManager.Singleton.IsHost)
            return;

        Vector3 spawnPos = Vector3.zero;
        Quaternion spawnRot = Quaternion.identity;

        var playerInstance = Instantiate(playerPrefab, spawnPos, spawnRot);
        var networkObject = playerInstance.GetComponent<NetworkObject>();
        networkObject.SpawnAsPlayerObject(clientId, true);  // true = take ownership
    }

#if UNITY_STANDALONE_WIN
    [DllImport("user32.dll", SetLastError = true)]
    static extern bool SetWindowText(IntPtr hWnd, string lpString);
    [DllImport("user32.dll", SetLastError = true)]
    static extern uint GetWindowThreadProcessId(IntPtr hWnd, out uint lpdwProcessId);
    [DllImport("user32.dll")]
    static extern IntPtr EnumWindows(EnumWindowsProc enumProc, IntPtr lParam);
    private delegate bool EnumWindowsProc(IntPtr hWnd, IntPtr lParam);
    private static IntPtr FindWindowByProcessId(uint processId)
    {
        IntPtr windowHandle = IntPtr.Zero;
        EnumWindows((hWnd, lParam) =>
        {
            GetWindowThreadProcessId(hWnd, out uint windowProcessId);
            if (windowProcessId == processId)
            {
                windowHandle = hWnd;
                return false;
            }
            return true;
        }, IntPtr.Zero);
        return windowHandle;
    }
    static void SetWindowTitle(string title)
    {
        /*
#if !UNITY_EDITOR
        uint processId = (uint)Process.GetCurrentProcess().Id;
        IntPtr hWnd = FindWindowByProcessId(processId);
        if (hWnd != IntPtr.Zero)
        {
            SetWindowText(hWnd, title);
        }
#endif
    */
        }
#else
    static void SetWindowTitle(string title) { }
#endif
}