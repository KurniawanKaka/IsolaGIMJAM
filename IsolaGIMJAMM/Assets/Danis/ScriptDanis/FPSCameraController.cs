using UnityEngine;
using System.Collections;

public class FPSCameraController : MonoBehaviour
{
    [Header("Sensitivity & Limits")]
    public float mouseSensitivity = 200f;
    public float verticalLimit = 50f;
    public float horizontalLimit = 60f;
    public bool isLocked = false;

    [Header("References")]
    public Transform playerBody;
    public ItemLookDownSwitcher switcher; // Hubungkan ke switcher

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        StartCoroutine(CameraLoop());
    }

    IEnumerator CameraLoop()
    {
        while (true) // Pengganti Update
        {
            if (!isLocked)
            {
                float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity * Time.deltaTime;
                float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity * Time.deltaTime;

                xRotation -= mouseY;
                xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);

                yRotation += mouseX;
                yRotation = Mathf.Clamp(yRotation, -horizontalLimit, horizontalLimit);

                transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

                if (playerBody != null)
                    playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);

                // Kabari switcher kalau sudut berubah
                switcher.CheckPitch(xRotation);
            }
            yield return null; // Tunggu ke frame berikutnya
        }
    }
}