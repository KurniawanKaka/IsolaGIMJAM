using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class GameDifficultyManager : MonoBehaviour
{
    public static GameDifficultyManager Instance;

    [Header("Player Stats")]
    public int maxNyawa = 5;
    public int nyawa;

    [Header("UI References")]
    public BatteryUI batteryUI; // Drag script BatteryUI tadi kesini

    void Start()
    {
        // Reset nyawa saat mulai
        nyawa = maxNyawa;

        // Update tampilan awal (Penuh)
        if (batteryUI != null) batteryUI.UpdateBattery(nyawa);
    }

    public void KurangiNyawa(int jumlah = 1)
    {
        nyawa -= jumlah;

        // Cegah minus
        if (nyawa < 0) nyawa = 0;

        Debug.Log($"Nyawa berkurang! Sisa: {nyawa}");

        // Update UI Baterai
        if (batteryUI != null) batteryUI.UpdateBattery(nyawa);

    }

    [Header("Referensi")]
    public GameColorManager colorManager; // Drag script GameColorManager kesini

    [Header("Stats Tracking")]
    public int currentRound = 0; // Menghitung sudah berapa kali masuk SetUpState

    [Header("Stress Settings")]
    public float baseMaxStress = 100;
    public float stressDecreasePerRound = 5f;
    public float minLimitStress = 10f; // Batas minimal biar gak mustahil

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Panggil fungsi ini setiap kali masuk SetUpState
    public void IncrementRound()
    {
        currentRound++;
    }

    // =========================================================
    // 1. LOGIKA MAX STRESS (Makin lama main, makin gampang stress)
    // =========================================================
    public float GetCurrentMaxStress()
    {
        // Rumus: 50 - (Ronde * 5)
        float calculatedStress = baseMaxStress - (currentRound * stressDecreasePerRound);

        // Clamp biar tidak di bawah batas minimal (misal 20)
        return Mathf.Max(calculatedStress, minLimitStress);
    }

    // =========================================================
    // 2. LOGIKA JUMLAH NPC (Berdasarkan Warna yang Didapat)
    // =========================================================
    public int GetNPCCount()
    {
        int colorsFound = 0;

        // 1. CEK REFERENCE (Penting!)
        if (colorManager != null)
        {
            colorsFound = colorManager.GetUnlockedColorsCount();
        }
        else
        {
            Debug.LogError("GameDifficultyManager: Slot 'Color Manager' belum diisi di Inspector!");
        }

        // 2. LOGIKA PROGRESIF
        if (colorsFound < 3)
        {
            // FASE AWAL: Cuma 2 orang (Mudah)
            return 2;
        }
        else if (colorsFound >= 3 && colorsFound <= 5)
        {
            // FASE MENENGAH: 3 atau 4 orang
            // Random.Range(3, 5) -> Akan keluar angka 3 atau 4.
            return Random.Range(3, 5);
        }
        else
        {
            // FASE SULIT: 4, 5, atau 6 orang
            // Random.Range(4, 7) -> Akan keluar angka 4, 5, atau 6.
            return 4;
        }
    }

    // =========================================================
    // 3. LOGIKA TIMER (Berdasarkan Warna yang Didapat)
    // =========================================================
    public float GetTimePerNPC()
    {
        int colorsFound = 0;
        if (colorManager != null) colorsFound = colorManager.GetUnlockedColorsCount();

        if (colorsFound < 3)
        {
            return 6.0f; // Lambat (Mudah)
        }
        else if (colorsFound >= 3 && colorsFound <= 4)
        {
            return 4.0f; // Sedang
        }
        else
        {
            // Makin banyak warna, makin cepat mikirnya
            return 2.5f; // Cepat (Susah)
        }
    }

    // =========================================================
    // 4. IDE TAMBAHAN: TINGKAT KEMIRIPAN (Distraction Similarity)
    // =========================================================
    public float GetDistractionSimilarity()
    {
        // 0 = Beda banget (Gampang dibedakan)
        // 1 = Mirip banget (Susah dibedakan)

        // Semakin tinggi ronde, NPC pengecoh makin mirip target
        float similarity = 0.2f + (currentRound * 0.05f);
        return Mathf.Clamp(similarity, 0.2f, 0.8f);
    }
}