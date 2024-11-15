using UnityEngine;
using Nakama;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Text;
using System.Diagnostics.CodeAnalysis;

public class NakamaManager : MonoBehaviour
{
    /*
    TODO: 
    3. setup a flow to register inputs/direction in unity. 

    */

    private IClient client = null;
    private ISocket socket = null; 
    private ISession session = null;
    private string authToken; 
    const string personaTag = "_test2persona";

    public string PersonaTag
    {
        get {return personaTag;}
    }

    const string playerNickName = "default-9"; //"Shantanu_test_player";
    public string PlayerNickName
    {
        get {return playerNickName; }
    }

    
    public delegate void MovementValidationEvent(bool IsValid, float PostionX, float PositionY);

    // Declare the event
    public event MovementValidationEvent OnEventTriggered;


    /*----------------------------------------------*/
    public class CardinalEvent
    {
        [Serializable]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public struct Message
        {
            public string message;
        }
    }

    public class CardinalReceipt
    {
        [Serializable]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public struct TxHash
        {
            public string txHash;
        }

        [Serializable]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public struct Errors
        {
            public string errors;
        }

        // The exact structure of this Result field will depend on the message you defined in your game.
        [Serializable]
        [SuppressMessage("ReSharper", "InconsistentNaming")]
        public struct Result
        {
            
            public bool isValid;
            public float LocationX;
            public float LocationY;
        }
        public Result? result;
    }

    /*-------------------------------------------------------------*/

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    async void Start()
    {
        client = new Nakama.Client("http", "127.0.0.1", 7350, "defaultkey");
        Debug.Log(client);
        session = await client.AuthenticateDeviceAsync(SystemInfo.deviceUniqueIdentifier);
        authToken = session.AuthToken;
        socket = client.NewSocket();
        bool appearOnline = true;
        int connectionTimeout = 30;
        await socket.ConnectAsync(session, appearOnline, connectionTimeout);
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

    }

    private void OnDestroy()
    {
        if(client != null && session != null)
        {
            client.SessionLogoutAsync(session);
        }
    }
    
    public async Task<Dictionary<string, object>> UpdatePlayerMovementState(float velocity, int direction, float locationX, float locationY)
    {
        if(session == null || session.IsExpired)
        {
            // more loud failure here.
            return new Dictionary<string, object> { { "isvalid", true } };;
        }

        try
        {
            // TODO: cache and resuse this dict. 
            var payload = new Dictionary<string, object>
            {
                { "persona", personaTag},
                { "Target", playerNickName },
                { "Velocity", velocity },
                { "Direction", direction },
                { "LocationX", locationX },
                { "LocationY", locationY }
            };

            Debug.Log(DictionaryToJson(payload));
            var response = await client.RpcAsync(session, "tx/game/movement-player", DictionaryToJson(payload));
            if (!string.IsNullOrEmpty(response.Payload))
            {
                // Deserialize the JSON payload into a Dictionary
                var responsePayload = JsonUtility.FromJson<Dictionary<string, object>>(response.Payload);
                Debug.Log("Response Payload: " + response.Payload);
            }
        }
        catch(ApiResponseException ex)
        {
            Debug.LogFormat("Error: {0}", ex.Message);
        }
        return new Dictionary<string, object> { { "isvalid", true } };
    }
    
    public async Task<Dictionary<string, object>> UpdatePlayerMovementState(Dictionary<string, object> payload )
    {

        if(session == null) 
        {
            return new Dictionary<string, object> { { "isvalid", true } };
        }
        
        try
        {
            payload["persona"] = personaTag;
            payload["TargetNickname"] = playerNickName;
            Debug.Log(DictionaryToJson(payload));
            var response = await client.RpcAsync(session, "tx/game/movement-player", DictionaryToJson(payload));
            Debug.Log(response);
        }
        catch(ApiResponseException ex)
        {
            Debug.LogFormat("Error: {0}", ex.Message);
        }
        return new Dictionary<string, object> { { "isvalid", true } };
    }

    // Update is called once per frame
    void LateUpdate()
    {
        CheckNotifications();
    }


    public void CheckNotifications()
    {
        if(socket == null)
        {
            return;
        }
        socket.ReceivedNotification += notification =>
        {
            switch (notification.Subject)
            {
                case "receipt":
                    {
                        Debug.LogWarning($"Message Subject '{notification.Subject}' content '{notification.Content}'");
                        var cardinalReceipt = JsonUtility.FromJson<CardinalReceipt>(notification.Content);
                        if (cardinalReceipt.result != null)
                        {
                            OnEventTriggered?.Invoke(cardinalReceipt.result.Value.isValid,
                                                    cardinalReceipt.result.Value.LocationX,
                                                    cardinalReceipt.result.Value.LocationY);
                        }
                        break;
                    }
                case "event":
                    {
                        Debug.LogWarning($" Event Subject '{notification.Subject}' content '{notification.Content}'");
                        var cardinalEvent = JsonUtility.FromJson<CardinalEvent>(notification.Content);
                        break;
                    }
            }

        };


       

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
