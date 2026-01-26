using UnityEngine;

public class HoverOutline : MonoBehaviour
{
    public Material outlineMat;  // Drag material outline ke sini
    private Material defaultMat; // Material asli disimpan otomatis
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        defaultMat = sr.material; // Simpan material awal saat game mulai
    }

    // Dipanggil otomatis sama Unity kalau ada mouse masuk
    void OnMouseEnter()
    {
        sr.material = outlineMat;
    }

    // Dipanggil otomatis kalau mouse keluar
    void OnMouseExit()
    {
        sr.material = defaultMat;
    }
}