using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftRumble : MonoBehaviour
{
    [Header("Setting Getaran Dasar")]
    public float kekuatanGetar = 0.06f; // Agak kasar
    public float kecepatanGetar = 3f;   // Lambat/Berat

    [Header("Setting Sentakan (Jolt)")]
    public float kekuatanSentakan = 0.15f; // Sentakan keras sesekali
    public float intervalSentakan = 2.0f;  // Seberapa sering ngecek sentakan

    public bool sedangJalan = false;

    private Vector3 posisiAwal;
    private float timerSentakan;
    private float bonusY = 0f; // Variabel untuk efek jatuh

    void Start()
    {
        posisiAwal = transform.localPosition;
    }

    void Update()
    {
        if (sedangJalan)
        {
            // 1. Getaran Dasar (Perlin Noise)
            float x = (Mathf.PerlinNoise(Time.time * kecepatanGetar, 0f) - 0.5f) * kekuatanGetar;
            float y = (Mathf.PerlinNoise(0f, Time.time * kecepatanGetar) - 0.5f) * kekuatanGetar;

            // 2. Logika Sentakan (Kadang-kadang liftnya anjlok ke bawah dikit)
            timerSentakan -= Time.deltaTime;
            if (timerSentakan <= 0)
            {
                // 30% kemungkinan terjadi sentakan setiap interval habis
                if (Random.value < 0.3f)
                {
                    // Beri hentakan ke bawah (negatif Y)
                    bonusY = -kekuatanSentakan;
                }
                timerSentakan = intervalSentakan;
            }

            // Kembalikan posisi sentakan ke 0 pelan-pelan (Spring effect)
            bonusY = Mathf.Lerp(bonusY, 0, Time.deltaTime * 5f);

            // Gabungkan semua posisi
            // Posisi = Awal + GetarKiriKanan + GetarAtasBawah + Sentakan
            transform.localPosition = posisiAwal + new Vector3(x, y + bonusY, 0);
        }
        else
        {
            // Reset posisi pelan-pelan saat berhenti
            transform.localPosition = Vector3.Lerp(transform.localPosition, posisiAwal, Time.deltaTime * 2f);
        }
    }

    public void MulaiGetar()
    {
        sedangJalan = true;
    }

    public void StopGetar()
    {
        sedangJalan = false;
    }
}