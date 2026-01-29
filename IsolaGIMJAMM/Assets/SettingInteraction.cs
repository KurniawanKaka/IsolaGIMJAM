using UnityEngine;

public class SettingInteraction : MonoBehaviour
{
    [Header("Settings")]
    public Camera mainCam;
    public FPSCameraController camController;
    public float interactionDist = 3f;
    public LayerMask uiLayer;

    // Tambahan referensi Sistem Napas (biar bisa dimatikan saat edit)
    public SistemNapas sistemNapas;

    private bool isHoldingSlider = false;
    private SensitivityPoster currentPoster;

    void Update()
    {
        // 1. SAAT KLIK KIRI DITEKAN (MENCARI SLIDER)
        if (Input.GetMouseButtonDown(0))
        {
            TryGrabSlider();
        }

        // 2. SAAT KLIK KIRI DITAHAN (MENGENDALIKAN SLIDER)
        if (isHoldingSlider)
        {
            // Log ini harusnya muncul TERUS MENERUS selama ditahan
            // Jika tidak muncul, berarti variable isHoldingSlider berubah jadi false
            // Debug.Log("DEBUG: Sedang Menahan Slider..."); 

            // Matikan sistem napas biar scroll gak bentrok
            if (sistemNapas != null) sistemNapas.enabled = false;

            // Kunci kamera
            if (camController != null) camController.isLocked = true;

            // Baca Scroll (Coba dua metode input sekaligus biar pasti)
            float scroll = Input.mouseScrollDelta.y;

            // Jika scroll terdeteksi
            if (Mathf.Abs(scroll) > 0)
            {
                Debug.Log($"DEBUG: Scroll Terdeteksi! Nilai: {scroll}");

                if (currentPoster != null)
                {
                    currentPoster.ScrollSensitivity(scroll);
                }
                else
                {
                    Debug.LogError("ERROR: Current Poster Null! Kehilangan referensi script.");
                }
            }
        }

        // 3. SAAT KLIK KIRI DILEPAS (STOP)
        if (Input.GetMouseButtonUp(0))
        {
            ReleaseSlider();
        }
    }

    void TryGrabSlider()
    {
        Ray ray = new Ray(mainCam.transform.position, mainCam.transform.forward);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, interactionDist, uiLayer))
        {
            // Cek apa yang kena raycast
            Debug.Log($"DEBUG RAYCAST: Kena Objek '{hit.collider.name}' | Tag: '{hit.collider.tag}'");

            if (hit.collider.CompareTag("Sensi"))
            {
                currentPoster = hit.collider.GetComponentInParent<SensitivityPoster>();

                if (currentPoster != null)
                {
                    isHoldingSlider = true;
                    Debug.Log("SUKSES: Slider Ditangkap! Mode Edit AKTIF.");
                }
                else
                {
                    Debug.LogError("ERROR: Tag benar 'SensitivitySlider', tapi Script 'SensitivityPoster' tidak ditemukan di Parent!");
                }
            }
        }
    }

    void ReleaseSlider()
    {
        if (isHoldingSlider)
        {
            Debug.Log("DEBUG: Slider Dilepas. Mode Edit NON-AKTIF.");
            isHoldingSlider = false;
            currentPoster = null;

            if (camController != null) camController.isLocked = false;
            if (sistemNapas != null) sistemNapas.enabled = true; // Nyalakan napas lagi
        }
    }
}