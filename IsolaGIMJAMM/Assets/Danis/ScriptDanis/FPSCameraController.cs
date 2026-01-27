using UnityEngine;

public class FPSCameraController : MonoBehaviour
{
    [Header("Sensitivity & Limits")]
    // KARENA Time.deltaTime DIHAPUS, TURUNKAN SENSITIVITY DI INSPECTOR!
    // Coba mulai dari angka kecil misal 1.0 atau 2.0
    public float mouseSensitivity = 2.0f;

    public float verticalLimit = 50f;   // Batas nunduk (X)
    public float horizontalLimit = 60f; // Batas nengok (Y)
    public bool isLocked = false;

    [Header("References")]
    public Transform playerBody;
    public ItemLookDownSwitcher switcher;

    private float xRotation = 0f;
    private float yRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // Inisialisasi rotasi awal agar tidak snapping
        xRotation = transform.localEulerAngles.x;
        if (xRotation > 180) xRotation -= 360;

        // Jika playerBody ada, yRotation ikut body. Jika tidak, ikut kamera.
        if (playerBody != null)
            yRotation = playerBody.localEulerAngles.y;
        else
            yRotation = transform.localEulerAngles.y;
    }

    // GANTI KE LATEUPDATE
    // Agar rotasi diproses SETELAH semua pergerakan fisik selesai, 
    // dan BERSAMAAN dengan script PhotoMechanic.
    void LateUpdate()
    {
        if (isLocked) return;

        // 1. HAPUS Time.deltaTime
        // Input mouse sudah bersifat delta (perubahan posisi), tidak perlu dikali waktu.
        float mouseX = Input.GetAxisRaw("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxisRaw("Mouse Y") * mouseSensitivity;

        // 2. HITUNG ROTASI VERTIKAL (Nunduk/Dongak)
        xRotation -= mouseY;
        xRotation = Mathf.Clamp(xRotation, -verticalLimit, verticalLimit);

        // 3. HITUNG ROTASI HORIZONTAL (Tengok Kanan/Kiri)
        yRotation += mouseX;
        yRotation = Mathf.Clamp(yRotation, -horizontalLimit, horizontalLimit);

        // 4. TERAPKAN ROTASI
        // Kamera: Ikut X (Nunduk) dan Y (Tengok)
        transform.localRotation = Quaternion.Euler(xRotation, yRotation, 0f);

        // Body Player: Ikut Y (Tengok) saja, X-nya 0 agar badan tidak ikut nunduk/terbang
        if (playerBody != null)
        {
            // Kita set body player rotasi Y-nya sama dengan kamera
            playerBody.localRotation = Quaternion.Euler(0f, yRotation, 0f);
        }

        // 5. KABARI SWITCHER
        if (switcher != null)
        {
            switcher.CheckPitch(xRotation);
        }
    }
}