using UnityEngine;
using UnityEngine.UI;
using TMPro; // Pastikan pakai TextMeshPro

public class DebugNPCButton : MonoBehaviour
{
    [Header("UI References")]

    public Image rambutImg;
    public Image bajuImg;
    public Image celanaImg;
    public Image kepalaImg;
    public Image sepatuImg;
    public TextMeshProUGUI infoText; // Drag Text yang ada di anak button kesini
    public Image buttonImage; // Drag Image button kesini (untuk ganti warna kalau benar/salah)

    private bool isTarget;
    private SetUpState manager;

    public void SetupButton(NPCStyleData data, bool _isTarget, SetUpState _manager)
    {
        isTarget = _isTarget;
        manager = _manager;
    }

    // Tampilkan info teks
    public void SetupButton(NPCStyleData data, bool _isTarget, object manager)
    {
        // 1. Simpan Data Logika
        isTarget = _isTarget;

        // 2. TAMPILKAN GAMBAR (RENDER)
        // Ambil sprite dari SO (data.rambut.visual) lalu pasang ke UI Image
        if (data.rambut != null) rambutImg.sprite = data.rambut.visual;
        if (data.baju != null) bajuImg.sprite = data.baju.visual;
        if (data.celana != null) celanaImg.sprite = data.celana.visual;

        if (data.sepatu != null) sepatuImg.sprite = data.sepatu.visual;

        // Debug visual: Kasih tanda bintang kecil kalau target (Opsional, untuk cheat developer)
        // if (isTarget) infoText.text += " (*)"; 
    }

}