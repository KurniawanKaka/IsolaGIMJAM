using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    [Header("Sensitivity & Limits")]
    public float mouseSensitivity = 200f;
    public float verticalLimit = 50f;   // Batas nunduk (X)
    public float horizontalLimit = 60f; // Batas nengok (Y)
    public bool isLocked = false;

    [Header("References")]
    public Transform playerBody; // Drag objek 'npc1 (1)' ke sini

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;

        // Reset posisi kamera agar tepat di tengah poros badan saat mulai
        transform.localPosition = new Vector3(0, transform.localPosition.y, 0);
    }

    void Update()
    {
        if (isLocked) return;

        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

        // 1. Hitung rotasi
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);

        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -horizontalLimit, horizontalLimit);

        // 2. Terapkan rotasi ke kamera (Look Rotation)
        // Kita gunakan rotasi relatif terhadap start (identity)
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // 3. Terapkan rotasi ke badan (Body Follow)
        // Body hanya ikut rotasi Y agar kaki tetap sinkron di bawah mata
        if (playerBody != null)
        {
            playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        }
    }
}