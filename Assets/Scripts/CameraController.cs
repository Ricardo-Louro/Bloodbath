using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField] private float heightOffset;
    [SerializeField] private float mouseSens;
    public PlayerMovement player;

    private Vector3 rotation;

    private void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    // Update is called once per frame
    private void Update()
    {
        MoveCamera();
        RotateCamera();
        player.Rotate(rotation.y);
    }

    private void MoveCamera()
    {
        if(player != null)
        {
            Vector3 pos = player.transform.position;
            pos.y += heightOffset;
            transform.position = pos;
        }
    }

    private void RotateCamera()
    {
        rotation.x -= Input.GetAxis("Mouse Y") * mouseSens;
        rotation.y += Input.GetAxis("Mouse X") * mouseSens;

        rotation.x = Mathf.Clamp(rotation.x, -90, 90);

        transform.rotation = Quaternion.Euler(rotation);
    }
}