using System.Collections.Generic;
using UnityEngine;

public class GameColorManager : MonoBehaviour
{
    public static GameColorManager Instance;



    // --- TAMBAHAN BARU ---
    [Header("Runtime Data")]
    public int currentRoundTargetColorIndex;

    public int currentRoundBodyPartIndex;

    [Header("Setting Warna Dunia")]
    public Color lockedColor = new Color(0.3f, 0.3f, 0.3f, 1f); // Abu-abu gelap (Monokrom)

    [System.Serializable]
    public class ColorInfo
    {
        public string name;      // Misal: "Merah"
        public Color actualColor; // Warna Aslinya (Pilih di Inspector)
        public bool isUnlocked;   // Status: Sudah ketemu belum?
    }

    // Hitung berapa warna yang SUDAH terbuka
    public int GetUnlockedColorsCount()
    {
        int count = 0;
        foreach (var color in palette)
        {
            if (color.isUnlocked) count++;
        }
        return count;
    }

    // List 10 Warna kamu
    public List<ColorInfo> palette = new List<ColorInfo>();

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // --- FUNGSI UTAMA YANG DIPANGGIL NPC ---
    // NPC bertanya: "Saya punya Index Warna 0 (Merah), saya harus tampil warna apa?"
    public Color GetColorStatus(int index)
    {
        // Safety check biar gak error
        if (index < 0 || index >= palette.Count) return Color.white;

        // Cek Status
        if (palette[index].isUnlocked)
        {
            return palette[index].actualColor; // Kalau udah unlock, balikin Merah
        }
        else
        {
            return lockedColor; // Kalau belum, balikin Abu-abu
        }
    }

    // Fungsi Debug/Cheat untuk Unlock (Nanti dipanggil kamera)
    public void UnlockColor(int index)
    {
        if (index >= 0 && index < palette.Count)
        {
            palette[index].isUnlocked = true;
            Debug.Log($"Warna {palette[index].name} TERBUKA!");

            // Refresh semua NPC biar langsung berubah visualnya
            NPCVisualControl.RefreshAllNPCs();
            // --- TAMBAHAN BARU: UPDATE UI ---
            // Cari UI di scene dan suruh update
            ColorControlUI ui = FindObjectOfType<ColorControlUI>();
            if (ui != null) ui.UpdateDisplay();
        }


    }

    // ------------------------------------------------------------------
    // FUNGSI BARU: MENCARI WARNA YANG BELUM DITEMUKAN SAJA
    // ------------------------------------------------------------------
    public int GetRandomLockedColorIndex()
    {
        // 1. Buat daftar penampungan sementara
        List<int> lockedIndices = new List<int>();

        // 2. Cek satu per satu dari 10 warna
        for (int i = 0; i < palette.Count; i++)
        {
            // Jika statusnya BELUM UNLOCKED (Masih False)
            if (!palette[i].isUnlocked)
            {
                // Masukkan nomor indexnya ke daftar
                lockedIndices.Add(i);
            }
        }

        // 3. Cek apakah daftar kosong? (Artinya semua warna sudah ketemu/Tamat)
        if (lockedIndices.Count == 0)
        {
            return -1; // Kode khusus yang artinya "GAME TAMAT / MENANG"
        }

        // 4. Pilih satu secara acak dari daftar yang tersisa
        int randomIndex = UnityEngine.Random.Range(0, lockedIndices.Count);
        return lockedIndices[randomIndex];
    }

    // Fungsi untuk mengecek sisa warna (Opsional, buat UI nanti)
    public int GetRemainingColorsCount()
    {
        int count = 0;
        foreach (var color in palette)
        {
            if (!color.isUnlocked) count++;
        }
        return count;
    }
}