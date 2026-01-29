using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.Rendering;

public class PhotoMechanic : MonoBehaviour
{

    public static event Action OnCekrek;

    [Header("Game Managers")]
    public DebugGameManager gm; // Drag GameManager kesini untuk kurangi nyawa

    [Header("Cameras")]
    public Camera fpsCam;       // Kamera Utama
    public Camera overlayCam;   // Kamera UI/Overlay (Opsional)
    public FPSCameraController camController;

    public AudioManager am;

    [Header("Visuals")]
    public ObjectSway swayScript;
    public GameObject viewfinderObj;
    public Image flashPanel;

    [Header("Immersion (Shake & Bob)")]
    public bool enableHeadBob = true;
    public float bobFrequency = 2.0f;  // Kecepatan nafas
    public float bobAmplitude = 0.005f; // Seberapa naik turun (kecil saja)

    [Header("Settings")]
    public float maxDistance = 100f; // Jarak maksimal foto

    // Koordinat Posisi & Scale (Sesuai kode aslimu)
    public Vector3 idlePos = new Vector3(0.333f, -0.184f, 0.4f);
    public Vector3 aimPos = new Vector3(0.115f, -0.116f, 0.4f);
    public Vector3 idleScale = new Vector3(0.172881f, 0.172881f, 0.172881f);
    public Vector3 aimScale = new Vector3(0.61393f, 0.61393f, 0.61393f);

    [Header("Animation Settings")]
    public float normalFOV = 60f;
    public float zoomFOV = 30f;
    public float animDuration = 0.35f;

    [Header("External Sync")]
    [HideInInspector] public float yOffset = 0f;
    public bool canAim = false;

    private bool isAiming;
    private bool inPhotoSequence = false; // Pengganti hasTakenPhoto agar bisa foto berkali-kali
    private Vector3 currentBasePos;
    private float shakeStrength = 0f; // Kekuatan getaran saat ini (dikontrol LeanTween)
    private Vector3 currentShakeOffset = Vector3.zero;
    private float shakeTimer = 0f;
    private float shakeMagnitude = 0f;
    private Vector3 shakeOffset = Vector3.zero;

    void Start()
    {
        // Inisialisasi awal
        if (viewfinderObj != null) viewfinderObj.SetActive(false);
        if (flashPanel != null) flashPanel.color = Color.clear;

        currentBasePos = transform.localPosition = idlePos;
        transform.localScale = idleScale;

        if (overlayCam != null) overlayCam.fieldOfView = normalFOV;

        // Auto-assign kamera jika lupa drag
        if (fpsCam == null) fpsCam = Camera.main;
    }

    void Update()
    {
        // Jika sedang proses jepret, matikan input sementara
        if (inPhotoSequence) return;
        // 1. UPDATE SHAKE TIMER
        if (shakeTimer > 0)
        {
            // Buat getaran acak
            shakeOffset = UnityEngine.Random.insideUnitSphere * shakeMagnitude;
            shakeOffset.z = 0; // Jangan getar maju mundur, pusing nanti
            shakeTimer -= Time.deltaTime;
        }
        else
        {
            shakeOffset = Vector3.zero;
        }
        // Logika Bidik
        if (canAim)
        {
            if (Input.GetMouseButtonDown(1)) RunAim(true); // Klik Kanan Tahan
            if (Input.GetMouseButtonUp(1)) RunAim(false);  // Lepas Klik Kanan

            // Input Motret (Klik Kiri): Hanya jika sedang bidik
            if (isAiming && Input.GetMouseButtonDown(0))
            {
                am.PlaySFX(am.shutter);
                StartCoroutine(SequenceFoto());
            }

        }
        else
        {
            if (isAiming) RunAim(false);
        }
    }

    void LateUpdate()
    {
        // 1. HITUNG POSISI SHAKE
        // Jika shakeStrength > 0, kita acak posisinya. Jika 0, offsetnya 0.
        if (shakeStrength > 0.001f)
        {
            // Random.insideUnitSphere bikin getaran ke segala arah
            Vector3 randomPoint = UnityEngine.Random.insideUnitSphere * shakeStrength;
            randomPoint.z = 0; // Kunci Z biar gak maju mundur (pusing)

            // Gunakan Lerp agar pergerakan acaknya tidak terlalu kasar (Optional)
            currentShakeOffset = Vector3.Lerp(currentShakeOffset, randomPoint, Time.deltaTime * 10f);
        }
        else
        {
            currentShakeOffset = Vector3.zero;
        }

        // 2. HITUNG BOBBING (NAFAS)
        Vector3 bobOffset = Vector3.zero;
        if (enableHeadBob && !isAiming)
        {
            bobOffset.y = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
        }

        // 3. GABUNGKAN SEMUA POSISI
        // currentBasePos (Aim/Idle) + yOffset (Nunduk) + Shake + Bobbing
        Vector3 finalPos = currentBasePos + (Vector3.up * yOffset) + currentShakeOffset + bobOffset;

        // Terapkan
        transform.localPosition = finalPos;
    }


    public void TriggerShake(float duration, float magnitude)
    {
        // Hentikan tween shake sebelumnya (jika ada) biar gak numpuk
        LeanTween.cancel(gameObject);

        // LOGIKA:
        // Kita tidak menggerakkan objeknya, tapi kita menggerakkan nilai 'shakeStrength'
        // Dari 0 -> Magnitude (Fade In) -> 0 (Fade Out)

        // 1. Fade In (Masuk perlahan) - Durasi 10% dari total waktu
        float fadeInTime = duration * 0.1f;
        float fadeOutTime = duration * 0.9f;

        // Tween Naik (0 ke Magnitude)
        LeanTween.value(gameObject, 0f, magnitude, fadeInTime)
            .setEaseOutQuad()
            .setOnUpdate((float val) => { shakeStrength = val; })
            .setOnComplete(() =>
            {
                // 2. Setelah Fade In selesai, langsung Fade Out (Magnitude ke 0)
                LeanTween.value(gameObject, magnitude, 0f, fadeOutTime)
                    .setEaseInQuad() // Ease In biar pas habisnya halus
                    .setOnUpdate((float val) => { shakeStrength = val; });
            });
    }

    // Opsi: Jika ingin getaran terus menerus (loop) untuk Lift yang sedang jalan lama
    public void StartConstantShake(float magnitude)
    {
        LeanTween.cancel(gameObject);
        // Fade in ke magnitude tertentu
        LeanTween.value(gameObject, shakeStrength, magnitude, 0.5f)
            .setOnUpdate((float val) => { shakeStrength = val; });
    }

    // Panggil ini saat Lift Berhenti
    public void StopShake()
    {
        LeanTween.cancel(gameObject);
        // Fade out ke 0
        LeanTween.value(gameObject, shakeStrength, 0f, 0.5f)
            .setOnUpdate((float val) => { shakeStrength = val; });
    }
    public bool IsAiming() => isAiming;

    public void RunAim(bool enter)
    {
        if (isAiming == enter) return;
        isAiming = enter;
        LeanTween.cancel(gameObject);

        // Feedback visual awal
        if (!enter && viewfinderObj != null) viewfinderObj.SetActive(false);

        if (swayScript != null)
        {
            swayScript.canSway = !enter;
            swayScript.gameObject.SetActive(true);
        }

        // 1. Animasi POSISI
        LeanTween.value(gameObject, currentBasePos, enter ? aimPos : idlePos, animDuration)
            .setEaseInOutQuad()
            .setOnUpdateVector3((Vector3 val) => { currentBasePos = val; });

        // 2. Animasi SCALE
        LeanTween.scale(gameObject, enter ? aimScale : idleScale, animDuration)
            .setEaseInOutQuad();

        // 3. Animasi FOV
        LeanTween.value(gameObject, fpsCam.fieldOfView, enter ? zoomFOV : normalFOV, animDuration)
            .setEaseInOutQuad()
            .setOnUpdate((float val) =>
            {
                fpsCam.fieldOfView = val;
                if (overlayCam != null) overlayCam.fieldOfView = val;

                // Threshold progres (0-1) untuk menyalakan UI Viewfinder
                float t = enter ?
                    Mathf.InverseLerp(normalFOV, zoomFOV, val) :
                    Mathf.InverseLerp(zoomFOV, normalFOV, val);

                if (enter && t > 0.7f && viewfinderObj != null && !viewfinderObj.activeSelf)
                {
                    viewfinderObj.SetActive(true);
                    if (swayScript != null) swayScript.gameObject.SetActive(false);
                }
            });
    }

    // --- LOGIKA UTAMA JEPRET ---
    IEnumerator SequenceFoto()
    {

        inPhotoSequence = true; // Kunci input
        Time.timeScale = 0.1f;
        // Kunci pergerakan mouse kamera
        if (camController != null) camController.isLocked = true;

        // 1. Efek Flash (Putih ke Transparan)
        if (flashPanel != null)
        {
            flashPanel.color = Color.white;
            LeanTween.value(gameObject, 1f, 0f, 0.5f).setIgnoreTimeScale(true)
                .setOnUpdate((float val) =>
                {
                    flashPanel.color = new Color(1, 1, 1, val);
                });
        }

        // 2. JALANKAN LOGIKA RAYCAST (DETEKSI TARGET)
        CheckForTarget();

        // 3. Freeze Dramatis (Jeda sebentar setelah foto)
        yield return new WaitForSeconds(0.1f);


        Time.timeScale = 1f;
        Debug.Log($"[SENDER] Saya PhotoMechanic dengan ID: {this.GetInstanceID()}. Mengirim Event sekarang.");
        OnCekrek?.Invoke();

        // Buka kunci kamera
        if (camController != null) camController.isLocked = false;

        inPhotoSequence = false; // Buka input lagi (Bisa foto lagi)

        // Opsional: Jika ingin kamera turun otomatis setelah foto, uncomment baris ini:

        RunAim(false);
        Debug.Log("PhotoMechanic: Cekrek! Event dikirim.");

    }

    // --- FUNGSI BARU: PENGGANTI LOGIKA LAMA ---
    void CheckForTarget()
    {
        // Tembak Raycast persis dari tengah layar kamera (Titik Fokus Viewfinder)
        Ray ray = fpsCam.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Debug Visual di Scene view (Garis Merah)
        Debug.DrawRay(ray.origin, ray.direction * maxDistance, Color.red, 2f);

        if (Physics.Raycast(ray, out hit, maxDistance))
        {
            // Cek apakah kena HITBOX BAGIAN TUBUH?
            NPCPartHitbox hitPart = hit.collider.GetComponent<NPCPartHitbox>();
            Time.timeScale = 0.1f;

            if (hitPart != null)
            {
                // Ambil Logic dari PlayerInteraction sebelumnya
                int targetColorIndex = GameColorManager.Instance.currentRoundTargetColorIndex;
                int clickedColorIndex = hitPart.GetMyColorIndex();

                Debug.Log($"JEPRET! Bagian: {hitPart.name} | Warna: {clickedColorIndex} | Target: {targetColorIndex}");

                if (clickedColorIndex == targetColorIndex)
                {
                    Debug.Log("FOTO SUKSES! Warna Benar.");
                    GameColorManager.Instance.UnlockColor(targetColorIndex);


                    // Efek suara sukses bisa ditaruh disini
                }
                else
                {
                    Debug.Log("FOTO GAGAL! Warna Salah.");

                    // Kurangi Nyawa

                    if (gm != null) gm.nyawa--;

                    // Efek suara gagal bisa ditaruh disini
                }
            }
            else
            {
                Debug.Log("Meleset: Tidak mengenai bagian tubuh NPC (Kena " + hit.collider.name + ")");
            }
        }
        else
        {
            Debug.Log("Meleset: Tidak mengenai apapun.");

        }
    }
    public void SetCanAim(bool state)
    {
        canAim = state;
    }

}