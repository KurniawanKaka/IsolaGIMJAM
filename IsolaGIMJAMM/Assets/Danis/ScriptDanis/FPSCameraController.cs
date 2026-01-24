using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    public float mouseSensitivity = 200f;
    public float horizontalLimit = 60f;
    public float verticalLimit = 80f;
    public bool isLocked = false; // Kunci Freeze

    private float xRotation = 0f;
    private float yRotation = 0f;
    private float startY;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        startY = transform.localEulerAngles.y;
    }

    void Update()
    {
        if (isLocked) return; // Kalau dilock, mouse ga fungsi

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);

        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -horizontalLimit, horizontalLimit);

        transform.localRotation = Quaternion.Euler(xRotation, startY + yRotation, 0f);
    }
}