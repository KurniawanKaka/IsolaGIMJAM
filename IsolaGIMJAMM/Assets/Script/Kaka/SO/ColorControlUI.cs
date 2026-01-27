using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorControlUI : MonoBehaviour
{
    [Header("Referensi 10 Kotak Image")]
    // Drag 10 Image yang kamu buat tadi ke list ini (berurutan!)
    public List<Image> colorSlots;

    void Start()
    {
        // Update tampilan saat game baru mulai
        UpdateDisplay();
    }

    public void UpdateDisplay()
    {
        if (GameColorManager.Instance == null) return;

        Debug.Log("UI UPDATE: Memperbarui tampilan warna..."); // <--- CCTV 1

        for (int i = 0; i < colorSlots.Count; i++)
        {
            if (i >= GameColorManager.Instance.palette.Count) break;

            var dataWarna = GameColorManager.Instance.palette[i];

            if (dataWarna.isUnlocked)
            {
                // CCTV 2: Lapor jika menemukan warna terbuka
                Debug.Log($"Slot {i} UNLOCKED! Mengubah warna jadi: {dataWarna.name}");
                colorSlots[i].color = dataWarna.actualColor;
            }
            else
            {
                colorSlots[i].color = Color.white;
            }
        }
    }
}