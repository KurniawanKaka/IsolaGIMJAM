using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class SistemNapas : MonoBehaviour
{
    [Header("Referensi Utama")]
    public Transform kameraGoyang;
    public Volume postProcessVolume;
    public Slider uiStressBar;

    [Header("Simulasi Gameplay")]
    [Tooltip("Ubah angka ini di Inspector untuk ngetes")]
    public int jumlahOrang = 1;

    [Header("Setting Stress")]
    public float stressSaatIni = 0f;
    public float maxStress = 100f;
    public float kenaikanPerOrang = 5f;
    public float kekuatanScroll = 15f;

    [Header("Setting Visual (SMOOTHING)")]
    // --- [BARU] Variable untuk mengatur kehalusan transisi ---
    [Tooltip("Semakin kecil angkanya, semakin lambat/halus efeknya berubah (Recommended: 2 - 5)")]
    public float kecepatanVisualSmooth = 3f;

    [Header("Setting Visual Panik")]
    public float powerGuncangan = 2f;
    public float speedGuncangan = 2f;

    [Space(10)] // Memberi jarak di inspector biar rapi
    [Range(0f, 1f)] public float maxGelapVignette = 0.6f;

    // --- [BARU] Tambahan efek biar gak flat ---
    [Range(0f, 1f)] public float maxChromaticAberration = 1f; // Efek pusing/lensa rusak
    [Range(-100f, 0f)] public float maxDesaturation = -60f;   // Efek pucat (minus)

    // Variabel privat
    private float seedX, seedY;

    // --- [BARU] Variabel penampung nilai halus ---
    private float visualStressRatio = 0f;

    // Penampung Component Post Process
    private Vignette vignetteEffect;
    private ChromaticAberration chromaticEffect; // [BARU]
    private ColorAdjustments colorEffect;        // [BARU]

    void Start()
    {
        seedX = Random.value * 100f;
        seedY = Random.value * 100f;

        // --- MENCARI EFEK DI DALAM VOLUME ---
        if (postProcessVolume != null)
        {
            // Cari Vignette
            postProcessVolume.profile.TryGet(out vignetteEffect);

            // --- [BARU] Cari efek tambahan ---
            postProcessVolume.profile.TryGet(out chromaticEffect);
            postProcessVolume.profile.TryGet(out colorEffect);

            if (vignetteEffect == null) Debug.LogWarning("Vignette belum ada di Global Volume!");
        }
        else
        {
            Debug.LogError("Objek Global Volume belum dimasukkan ke script!");
        }
    }

    void Update()
    {
        // --- 1. LOGIKA NAIK TURUN STRESS ---
        float kenaikanTotal = jumlahOrang * kenaikanPerOrang;
        stressSaatIni += kenaikanTotal * Time.deltaTime;

        float scrollInput = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollInput) > 0)
        {
            stressSaatIni -= kekuatanScroll * Mathf.Abs(scrollInput);
        }
        stressSaatIni = Mathf.Clamp(stressSaatIni, 0, maxStress);

        if (uiStressBar != null) uiStressBar.value = stressSaatIni;

        // --- 2. APLIKASIKAN EFEK VISUAL (YANG SUDAH DI-SMOOTH) ---
        ApplySmoothedVisuals();
    }

    void ApplySmoothedVisuals()
    {
        // Hitung target stress yang asli (0.0 sampai 1.0)
        float targetRatio = stressSaatIni / maxStress;

        // --- [BARU] RUMUS SMOOTHING ---
        // Kita tidak langsung pakai targetRatio.
        // Kita gerakkan visualStressRatio mendekati targetRatio pelan-pelan.
        // Ini kuncinya agar tidak "kaget" saat stress naik/turun mendadak.
        visualStressRatio = Mathf.Lerp(visualStressRatio, targetRatio, Time.deltaTime * kecepatanVisualSmooth);

        // A. EFEK GOYANG KAMERA (Pakai visualStressRatio)
        if (visualStressRatio > 0.01f)
        //{
        //    seedX += speedGuncangan * Time.deltaTime;
        //    seedY += speedGuncangan * Time.deltaTime;

        //    // Power goyangan juga di-lerp biar start-nya halus
        //    float currentPower = Mathf.Lerp(0, powerGuncangan, visualStressRatio);

        //    float x = (Mathf.PerlinNoise(seedX, 0) - 0.5f) * 2 * currentPower;
        //    float y = (Mathf.PerlinNoise(0, seedY) - 0.5f) * 2 * currentPower;
        //    kameraGoyang.localRotation = Quaternion.Euler(x, y, 0);
        //}
        //else
        //{
        //    kameraGoyang.localRotation = Quaternion.identity;
        //}

        // B. EFEK POST PROCESSING (Pakai visualStressRatio)

        // 1. Vignette (Gelap)
        if (vignetteEffect != null)
            vignetteEffect.intensity.value = visualStressRatio * maxGelapVignette;

        // 2. Chromatic Aberration (Pusing/Lensa Pisah Warna)
        if (chromaticEffect != null)
            chromaticEffect.intensity.value = visualStressRatio * maxChromaticAberration;

        // 3. Color Adjustments (Pucat)
        if (colorEffect != null)
            colorEffect.saturation.value = visualStressRatio * maxDesaturation;
    }
}