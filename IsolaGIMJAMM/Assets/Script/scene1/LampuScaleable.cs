using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampuScaleable : MonoBehaviour
{
    [Header("Komponen")]
    public Light sumberCahaya;
    public Renderer modelLampu; // Mesh bohlam/neon (Opsional)

    [Header("Setting Kecerahan (Scalable)")]
    [Tooltip("Kecerahan normal saat sedang stabil")]
    public float kecerahanMaksimal = 2.0f;

    [Tooltip("PENTING: Jangan set 0 biar gak mati total. Set misal 0.5")]
    public float kecerahanMinimal = 0.5f;

    [Header("Setting Pola Kedip")]
    [Tooltip("Seberapa cepat voltase naik turun (1 = lambat, 10 = cepat/rusak)")]
    public float kecepatanKedip = 5.0f;

    [Tooltip("Seberapa sering dia error? (0 = stabil, 1 = sangat acak)")]
    public float chaosLevel = 0.8f;

    // Internal vars
    private float seed;
    private Material matLampu;
    private Color warnaDasarEmission;

    void Start()
    {
        if (sumberCahaya == null) sumberCahaya = GetComponent<Light>();

        // Setup material untuk efek glowing (Emission)
        if (modelLampu != null)
        {
            matLampu = modelLampu.material;
            // Kita ambil warna emission awal sebagai referensi
            if (matLampu.HasProperty("_EmissionColor"))
            {
                warnaDasarEmission = matLampu.GetColor("_EmissionColor");
            }
        }

        // Acak seed biar kalau ada 2 lampu polanya gak kembar
        seed = Random.Range(0f, 100f);
    }

    void Update()
    {
        // 1. Rumus Perlin Noise untuk fluktuasi voltase
        // Ini menghasilkan angka halus naik turun antara 0.0 sampai 1.0
        float noise = Mathf.PerlinNoise(seed, Time.time * kecepatanKedip);

        // 2. Modifikasi noise biar lebih 'tajam/rusak' (sesuai Chaos Level)
        // Jika chaos tinggi, perpindahan terang ke redup lebih kasar
        if (noise > chaosLevel) noise = 1f; // Tetap terang
        else noise = noise * 0.5f; // Drop voltase (redup)

        // 3. Konversi noise ke Kecerahan (Scaling)
        // Mathf.Lerp memastikan nilainya TIDAK PERNAH di bawah kecerahanMinimal
        float intensitasAkhir = Mathf.Lerp(kecerahanMinimal, kecerahanMaksimal, noise);

        // 4. Terapkan ke Lampu
        sumberCahaya.intensity = intensitasAkhir;

        // 5. Terapkan ke Material (Biar benda 3D-nya ikut redup)
        if (matLampu != null)
        {
            // Gelapkan warna emission berdasarkan intensitas
            float rasioRedup = intensitasAkhir / kecerahanMaksimal;
            Color warnaAkhir = warnaDasarEmission * rasioRedup;
            matLampu.SetColor("_EmissionColor", warnaAkhir);
        }
    }
}