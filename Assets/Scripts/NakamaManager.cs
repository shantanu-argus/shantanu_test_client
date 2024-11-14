using UnityEngine;
using Nakama;
using System.Collections.Generic;
using System.Text;

public class NakamaManager : MonoBehaviour
{
    /*
    TODO: 
    3. setup a flow to register inputs/direction in unity. 

    */

    private IClient client;
    const string personaTag = "_test2persona";
    const string playerNickName = "Shantanu_test_player";
    string authToken; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        client = new Nakama.Client("http", "127.0.0.1", 7350, "defaultkey");
        Debug.Log(client);
        var session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier);
        authToken = session.AuthToken;
        Debug.Log(authToken);

        try
        {
            var payload = new Dictionary<string, object> {{ "personaTag", personaTag }};
            Debug.Log(DictionaryToJson(payload));
            var response = await client.RpcAsync(session, "nakama/claim-persona", DictionaryToJson(payload));
            Debug.Log(response);
        }
        catch (ApiResponseException ex)
        {
            Debug.LogFormat("Error: {0}", ex.Message);
        }

        try
        {   
            Debug.Log("Running test with create Api");
            var payload = new Dictionary<string, object> {
                {"Nickname", playerNickName},
            };
            Debug.Log(DictionaryToJson(payload));
            var response = await client.RpcAsync(session, "tx/game/create-player", DictionaryToJson(payload));
            Debug.Log(response);
        }
        catch (ApiResponseException ex)
        {
            Debug.LogFormat("Error: {0}", ex.Message);
        }

        //testing movement msg call should be moved to a utility function.
        try
        {
            var payload = new Dictionary<string, object>
            {
                { "Target", playerNickName },
                { "Velocity", 5 },
                { "Direction", 0 },
                { "LocationX", 0 },
                { "LocationY", 0 }
            };

            Debug.Log(DictionaryToJson(payload));
            var response = await client.RpcAsync(session, "tx/game/movement-player", DictionaryToJson(payload));
            Debug.Log(response);
        }
        catch (ApiResponseException ex)
        {
            Debug.Log(ex);
            Debug.LogFormat("Error: {0}", ex.Message);
        }


    }

    // Update is called once per frame
    void Update()
    {
        
    }

    string DictionaryToJson(Dictionary<string, object> dictionary)
    {
        var sb = new StringBuilder();
        sb.AppendLine("{");

        foreach (var kvp in dictionary)
        {
            string valueString;

            // Check if the value is a string, and add quotes if it is
            if (kvp.Value is string)
            {
                valueString = $"\"{kvp.Value}\"";
            }
            else
            {
                valueString = kvp.Value.ToString();
            }

            sb.AppendLine($"    \"{kvp.Key}\": {valueString},");
        }

        // Remove the trailing comma
        if (sb.Length > 2)
            sb.Length -= 3; 

        sb.AppendLine("\n}");

        return sb.ToString();
    }
}
