using UnityEngine;

public class PlayerMovementScript : MonoBehaviour
{
    public float moveSpeed = 5f;
    private Vector3 movement;
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    void Update()
    {
        // Reset movement
        movement = Vector3.zero;

        // Get input from WASD keys
        if (Input.GetKey(KeyCode.W))
        {
            movement.y = 1;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            movement.y = -1;
        }

        if (Input.GetKey(KeyCode.D))
        {
            movement.x = 1;
        }
        else if (Input.GetKey(KeyCode.A))
        {
            movement.x = -1;
        }

        // Normalize movement to prevent faster diagonal movement and scale by speed
        movement = movement.normalized * moveSpeed * Time.deltaTime;

        // Move the player
        transform.Translate(movement);
    }
}
