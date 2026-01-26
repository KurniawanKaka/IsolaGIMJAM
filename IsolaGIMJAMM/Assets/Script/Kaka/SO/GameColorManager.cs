using System.Collections.Generic;
using UnityEngine;

public class GameColorManager : MonoBehaviour
{
    public static GameColorManager Instance;

    [Header("Palet Warna Global")]
    public Color lockedColor = Color.gray; // Warna dunia saat belum ditemukan (Monokrom)

    [Tooltip("Masukkan 10 warna game disini (Merah, Biru, Hijau, dll)")]
    public List<ColorData> palette = new List<ColorData>();

    [System.Serializable]
    public class ColorData
    {
        public string name;      // Misal: "Merah Menyala"
        public Color colorRGB;   // Warna Asli
        public bool isUnlocked;  // Apakah sudah difoto player?
    }

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // Fungsi Utama: Minta Warna
    // Jika Index 0 (Merah) sudah unlock -> Balikin Merah
    // Jika belum -> Balikin Abu-abu (lockedColor)
    public Color GetColorByIndex(int index)
    {
        // Validasi agar tidak error jika index ngawur
        if (index < 0 || index >= palette.Count) return lockedColor;

        if (palette[index].isUnlocked)
        {
            return palette[index].colorRGB;
        }
        else
        {
            return lockedColor;
        }
    }

    // Fungsi saat Player berhasil memotret
    public void UnlockColor(int index)
    {
        if (index >= 0 && index < palette.Count)
        {
            if (!palette[index].isUnlocked)
            {
                palette[index].isUnlocked = true;
                Debug.Log($"WARNA DITEMUKAN: {palette[index].name}");

                // Refresh semua NPC agar yang pakai warna ini langsung berubah
                // NPCVisualControl.RefreshAllNPCs();
            }
        }
    }
}