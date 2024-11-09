using UnityEngine;
using Nakama; 

public class NakamaManager : MonoBehaviour
{
    /*
    TODO: 
    1. setup a persona claim flow.
    2. setup controller script in unity. 
    3. setup a flow to register inputs/direction in unity. 

    */

    private IClient client;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        client = new Nakama.Client("http", "127.0.0.1", 7350, "defaultkey");
        Debug.Log(client);
        var session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier);
        Debug.Log(session.AuthToken);
        Debug.Log(session.IsExpired);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
