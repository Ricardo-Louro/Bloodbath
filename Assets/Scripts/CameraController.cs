//Import necessary namespaces
using UnityEngine;

public class CameraController : MonoBehaviour
{
    //Set camera's additional height from the player's position
    [SerializeField] private float heightOffset;
    //Set the mouse sensitivity
    [SerializeField] private float mouseSens;
    //Set public reference to the player
    public PlayerMovement player;

    //Set rotation value to be constantly updated and reutilized
    private Vector3 rotation;

    private void Start()
    {
        //Lock the cursor to the game
        Cursor.lockState = CursorLockMode.Locked;
        //Hide the cursor
        Cursor.visible = false;
    }

    private void Update()
    {
        //Move the camera to the player's position
        MoveCamera();
        //Rotate the camera according to the mouse movement
        RotateCamera();
        //Rotate the player according to the camera's rotation
        player.Rotate(rotation.y);
    }

    private void MoveCamera()
    {
        //If there is a valid reference to a player
        if(player != null)
        {
            //Save the player's position
            Vector3 pos = player.transform.position;
            //Apply the height offset to have it set a eye level
            pos.y += heightOffset;
            //Move the camera to this edited position
            transform.position = pos;
        }
    }

    private void RotateCamera()
    {
        //Modify the vertical rotation based on mouse movement
        rotation.x -= Input.GetAxis("Mouse Y") * mouseSens;
        //Modify the horizontal rotation based on mouse movement
        rotation.y += Input.GetAxis("Mouse X") * mouseSens;

        //Clamp the vertical rotation from -90 to 90 degrees to avoid 360 rotations in this axis
        rotation.x = Mathf.Clamp(rotation.x, -90, 90);

        //Set the camera rotation to this new value
        transform.rotation = Quaternion.Euler(rotation);
    }
}