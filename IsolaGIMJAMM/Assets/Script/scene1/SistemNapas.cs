using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Events;
using System.Data.Common; // Tambahan untuk Event

public class SistemNapas : MonoBehaviour
{

    public AudioManager am;
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
    public float maxStress = 100;
    public float kenaikanPerOrang;
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

    private AudioClip currentNafasClip;

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

        // if (maxStress > maxStress / 3)
        // {
        //     am.PlaySFX(am.Nafasmin);
        // }
        // if (maxStress > maxStress / 3)
        // {
        //     am.PlaySFX(am.Nafasmin);
        // }
        // if (maxStress > maxStress / 3)
        // {
        //     am.PlaySFX(am.Nafasmin);
        // }
        // CEK KONDISI GAMEOVER / BLACKOUT
        if (stressSaatIni >= maxStress)
        {
            TriggerBlackout();
        }
        HandleAudioNafas();
        // 2. APLIKASIKAN EFEK VISUAL
        ApplySmoothedVisuals();
    }

    void TriggerBlackout()
    {
        isBlackout = true;
        Debug.Log("PEMAIN BLACKOUT! GAMEOVER.");

        if (am != null && currentNafasClip != null)
        {
            am.StopLoopingSFX(currentNafasClip);
        }

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
    public void SetMaxStress(float newValue)
    {

        Debug.Log($"<color=green>[SistemNapas]</color> Limit Stress diubah: {maxStress} -> {newValue}");
        maxStress = newValue;


    }
    void HandleAudioNafas()
    {
        if (am == null) return;

        // 1. Hitung Persentase Stress (0.0 sampai 1.0)
        float ratio = stressSaatIni / maxStress;

        // 2. Tentukan Target Audio berdasarkan Persentase
        AudioClip targetClip = null;

        if (ratio <= 0.3f) // Di bawah 30% (Aman)
        {
            targetClip = am.Nafasmin;
        }
        else if (ratio <= 0.6f) // 30% - 60% (Menengah)
        {
            targetClip = am.Nafasmid;
        }
        else // Di atas 60% (Panik)
        {
            targetClip = am.Nafasmax;
        }

        // 3. LOGIKA GANTI SUARA (PENTING!)
        // Kita hanya menyuruh AudioManager kerja JIKA target audionya berubah.
        // Contoh: Dari 'NafasMin' berubah jadi 'NafasMid'.

        if (currentNafasClip != targetClip)
        {
            // Matikan suara yang lama (jika ada)
            if (currentNafasClip != null)
            {
                am.StopLoopingSFX(currentNafasClip);
            }

            // Mainkan suara yang baru (Looping)
            am.PlayLoopingSFX(targetClip);

            // Simpan status sekarang
            currentNafasClip = targetClip;
        }
    }
}