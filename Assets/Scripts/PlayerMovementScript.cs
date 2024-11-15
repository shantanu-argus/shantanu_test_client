using UnityEngine;
using System.Collections.Generic;
public class PlayerMovementScript : MonoBehaviour
{
    public float velocity = 5f;
    private Vector3 movement;
    private Dictionary<int, Vector2> directions = new Dictionary<int, Vector2>
    {
        { 0, new Vector2(1, 0) },   // right
        { 1, new Vector2(-1, 0) },  // left
        { 2, new Vector2(0, -1) },  // up
        { 3, new Vector2(0, 1) }    // down
    };

    private int direction = -1;
    
    private float intervalToUpdateBackend = 2.0f;
    private float timerToUpdateBackend = 0.0f;
    private NakamaManager nakamaManager = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject nakamaManagerGameObject = GameObject.Find("NakamaManager");

        if(nakamaManagerGameObject != null)
        {
            nakamaManager = nakamaManagerGameObject.GetComponent<NakamaManager>();
            nakamaManager.OnEventTriggered+= HandleMovementIsInvalid;
        }
        else
        {
            Debug.LogError("Couldn't find NamakaManager game object.");
        }

    }

    async void Update()
    {
        // Reset movement
        movement = Vector3.zero;

        // Get input from WASD keys
        if (Input.GetKey(KeyCode.W))
        {
            direction = 2;
            movement.y = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            direction = 3;
            movement.y = -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            direction = 0;
            movement.x = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            direction = 1;
            movement.x = -1;
        }

        // Normalize movement to prevent faster diagonal movement and scale by speed
        movement = movement.normalized * velocity * Time.deltaTime;

        // Move the player
        transform.Translate(movement);


        if(timerToUpdateBackend <= 0 && nakamaManager != null)
        {
            timerToUpdateBackend = intervalToUpdateBackend;
          
            var response = await nakamaManager.UpdatePlayerMovementState(velocity, 
                                                                        direction,
                                                                        transform.position.x,
                                                                        transform.position.y );
            Debug.Log(response);
        }
        else
        {
            timerToUpdateBackend-= timerToUpdateBackend;
        }
    }

    private void HandleMovementIsInvalid(bool isValid, float positionX, float PositionY)
    {
        if(!isValid)
        {
            Debug.LogError("Server requesting reset of the position.");
           transform.Translate(new Vector3(positionX, PositionY,0));
        }
    }
}
