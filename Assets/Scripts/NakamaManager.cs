using UnityEngine;
using Nakama;
using System.Collections.Generic;
using System.Text;

public class NakamaManager : MonoBehaviour
{
    /*
    TODO: 
    1. setup a persona claim flow.
    3. setup a flow to register inputs/direction in unity. 

    */

    private IClient client;
    const string personaTag = "shantanu-test-persona";
    string authToken; 
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        client = new Nakama.Client("http", "127.0.0.1", 7350, "defaultkey");
        Debug.Log(client);
        var session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier);
        authToken = session.AuthToken;

        try
        {
            var payload = new Dictionary<string, string> {{ "personaTag", personaTag }};
            Debug.Log(DictionaryToJson(payload));
            var response = await client.RpcAsync(session, "nakama/claim-persona", DictionaryToJson(payload));
            Debug.Log(response);
        }
        catch (ApiResponseException ex)
        {
            Debug.LogFormat("Error: {0}", ex.Message);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    string DictionaryToJson(Dictionary<string, string> dictionary)
    {
        StringBuilder jsonBuilder = new StringBuilder();
        jsonBuilder.Append("{");

        int count = 0;
        foreach (var kvp in dictionary)
        {
            jsonBuilder.Append($"\"{kvp.Key}\": \"{kvp.Value}\"");
            count++;

            // Add comma if it's not the last element
            if (count < dictionary.Count)
            {
                jsonBuilder.Append(", ");
            }
        }

        jsonBuilder.Append("}");
        return jsonBuilder.ToString();
    }
}
