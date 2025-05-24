using UnityEngine;
using Unity.Netcode;

public class Projectile : NetworkBehaviour
{
    private NetworkManager networkManager;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        TestServerRpc(1, 2f);
    }

    [ServerRpc]
    void TestServerRpc(int param1, float param2)
    {
        //BROADCASTED INTO THE SERVER
        //DOES NOT HAPPEN IMMEDIATELY, IT IS SENT TO THE SERVER AND WILL HAPPEN

        float currentTime = NetworkManager.Singleton.ServerTime.TimeAsFloat; //use this instead of Time.time
    }
}