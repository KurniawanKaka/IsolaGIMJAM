using UnityEngine;
using System.Collections;

public class Getarr : MonoBehaviour
{
    [Header("Head Bobbing (Breathing)")]
    public bool enableBobbing = true;
    public float bobFrequency = 1.5f; // Kecepatan nafas
    public float bobAmplitude = 0.02f; // Seberapa naik turun kepalanya (jangan terlalu besar)

    [Header("Lift Shake")]
    public float defaultShakeDuration = 1.0f;
    public float defaultShakeMagnitude = 0.05f;

    private Vector3 initialPos;
    private float timer = 0f;
    private float currentShakeDuration = 0f;
    private float currentShakeMagnitude = 0f;

    void Start()
    {
        // Simpan posisi awal kamera relatif terhadap parent-nya
        initialPos = transform.localPosition;
    }

    void Update()
    {
        Vector3 targetPos = initialPos;

        // 1. LOGIKA HEAD BOBBING (Bernafas)
        if (enableBobbing)
        {
            // Gunakan Sinus Wave untuk gerakan naik turun halus
            float yBob = Mathf.Sin(Time.time * bobFrequency) * bobAmplitude;
            targetPos.y += yBob;
        }

        // 2. LOGIKA SHAKE (Getaran Lift)
        if (currentShakeDuration > 0)
        {
            // Buat getaran acak
            Vector3 shakeOffset = Random.insideUnitSphere * currentShakeMagnitude;

            // Kunci Z axis agar kamera tidak maju mundur aneh, cuma getar atas-bawah-kiri-kanan
            shakeOffset.z = 0;

            targetPos += shakeOffset;

            // Kurangi durasi getaran (Decay)
            currentShakeDuration -= Time.deltaTime;
        }

        // Terapkan posisi
        transform.localPosition = targetPos;
    }

    // --- PANGGIL FUNGSI INI DARI SCRIPT LAIN ---
    public void TriggerShake(float duration, float magnitude)
    {
        currentShakeDuration = duration;
        currentShakeMagnitude = magnitude;
    }

    // Shortcut untuk shake standar lift
    public void TriggerLiftRumble()
    {
        TriggerShake(defaultShakeDuration, defaultShakeMagnitude);
    }
}