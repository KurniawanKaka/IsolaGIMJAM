using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teskan : MonoBehaviour
{
    [Header("Settings")]
    public float shakeIntensity = 0.2f;
    public float shakeTime = 0.1f;

    void Update()
    {
        // Mendeteksi tombol Space
        if (Input.GetKeyDown(KeyCode.Space))
        {
            TriggerShake();
        }
    }

    public void TriggerShake()
    {
        // Simpan posisi awal lokal
        Vector3 originalPos = transform.localPosition;

        LeanTween.moveLocal(gameObject, originalPos + (Random.insideUnitSphere * shakeIntensity), shakeTime)
            .setEaseShake() // Ease khusus untuk getaran
            .setOnComplete(() =>
            {
                // Pastikan posisi kembali ke (0,0,0) setelah selesai
                transform.localPosition = originalPos;
            });
    }
}
