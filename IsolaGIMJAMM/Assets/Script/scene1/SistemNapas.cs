using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events; // Tambahan untuk Event

public class SistemNapas : MonoBehaviour
{
    [Header("Referensi Utama")]
    public Transform kameraGoyang; // Pasang Main Camera disini
    public Volume postProcessVolume;
    public Slider uiStressBar;
    public GameObject uiGameOver; // Panel Game Over (Layar Hitam/Teks)

    [Header("Status Gameplay")]
    public bool isBlackout = false; // Status apakah pemain pingsan

    [Header("Simulasi Gameplay")]
    public int jumlahOrang = 1;

    [Header("Setting Stress")]
    public float stressSaatIni = 0f;
    public float maxStress = 100f;
    public float kenaikanPerOrang = 5f;
    public float kekuatanScroll = 15f;

    [Header("Setting Visual")]
    public float kecepatanVisualSmooth = 3f;
    public float powerGuncangan = 2f;
    public float speedGuncangan = 2f;
    [Range(0f, 1f)] public float maxGelapVignette = 0.6f;
    [Range(0f, 1f)] public float maxChromaticAberration = 1f;
    [Range(-100f, 0f)] public float maxDesaturation = -60f;

    // Private variables
    private float seedX, seedY;
    private float visualStressRatio = 0f;
    private Vignette vignetteEffect;
    private ChromaticAberration chromaticEffect;
    private ColorAdjustments colorEffect;

    void Start()
    {
        seedX = Random.value * 100f;
        seedY = Random.value * 100f;

        if (uiGameOver != null) uiGameOver.SetActive(false);

        if (postProcessVolume != null)
        {
            postProcessVolume.profile.TryGet(out vignetteEffect);
            postProcessVolume.profile.TryGet(out chromaticEffect);
            postProcessVolume.profile.TryGet(out colorEffect);
        }
    }

    void Update()
    {
        // JIKA SUDAH BLACKOUT, STOP PROSES
        if (isBlackout) return;

        // 1. LOGIKA NAIK TURUN STRESS
        float kenaikanTotal = jumlahOrang * kenaikanPerOrang;
        stressSaatIni += kenaikanTotal * Time.deltaTime;

        // Input Scroll untuk bernafas
        float scrollInput = Input.mouseScrollDelta.y;
        if (Mathf.Abs(scrollInput) > 0)
        {
            // Mengurangi stress
            stressSaatIni -= kekuatanScroll * Mathf.Abs(scrollInput);
        }

        // Clamp agar tidak minus
        stressSaatIni = Mathf.Clamp(stressSaatIni, 0, maxStress);

        // Update UI Slider
        if (uiStressBar != null) uiStressBar.value = stressSaatIni;

        // CEK KONDISI GAMEOVER / BLACKOUT
        if (stressSaatIni >= maxStress)
        {
            TriggerBlackout();
        }

        // 2. APLIKASIKAN EFEK VISUAL
        ApplySmoothedVisuals();
    }

    void TriggerBlackout()
    {
        isBlackout = true;
        Debug.Log("PEMAIN BLACKOUT! GAMEOVER.");

        // Munculkan UI Game Over
        if (uiGameOver != null) uiGameOver.SetActive(true);

        // Paksa visual ke kondisi maksimal (gelap total/pusing)
        visualStressRatio = 1f;
        ApplySmoothedVisuals();

        // Unlock kursor agar bisa klik tombol restart
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void ApplySmoothedVisuals()
    {
        float targetRatio = stressSaatIni / maxStress;
        visualStressRatio = Mathf.Lerp(visualStressRatio, targetRatio, Time.deltaTime * kecepatanVisualSmooth);

        // --- B. POST PROCESSING (Update Bagian Ini) ---

        // 1. Vignette (Gelap Pinggir)
        if (vignetteEffect != null)
            vignetteEffect.intensity.value = visualStressRatio * maxGelapVignette;

        // 2. Chromatic Aberration (Pusing)
        if (chromaticEffect != null)
            chromaticEffect.intensity.value = visualStressRatio * maxChromaticAberration;

        // 3. Color Adjustments (Pucat & GELAP TOTAL)
        if (colorEffect != null)
        {
            // Efek Pucat (Saturation turun)
            colorEffect.saturation.value = visualStressRatio * maxDesaturation;

            // --- [LOGIKA BARU] EFEK BLACKOUT (Exposure Turun) ---
            // Jika visualStressRatio mendekati 1 (sangat stress), kita kurangi exposure drastis.
            // Angka -10 biasanya sudah cukup untuk membuat layar hitam total.

            float targetExposure = 0f;

            if (isBlackout)
            {
                // Jika status Blackout TRUE, langsung hitam pekat
                targetExposure = -20f;
            }
            else
            {
                // Jika belum blackout, gelapkan sedikit demi sedikit sesuai stress
                // Rumus: (0 sampai -5) tergantung stress
                targetExposure = Mathf.Lerp(0f, -5f, visualStressRatio);
            }

            // Terapkan ke Post Exposure
            colorEffect.postExposure.value = Mathf.Lerp(colorEffect.postExposure.value, targetExposure, Time.deltaTime * 5f);
        }
    }
}