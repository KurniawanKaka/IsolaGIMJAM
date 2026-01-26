using UnityEngine;

public class NPCVisualControl : MonoBehaviour
{
    [Header("Sprite Renderer References")]
    public SpriteRenderer rambutRenderer;
    public SpriteRenderer kepalaRenderer;
    public SpriteRenderer bajuRenderer;
    public SpriteRenderer celanaRenderer;
    public SpriteRenderer sepatuRenderer;

    private NPCStyleData currentData;

    public void SetupVisuals(NPCStyleData data)
    {
        currentData = data;
        // Default saat spawn: Hadap Depan (IsFront = true)
        SetDirection(true);
    }

    // Saya tambahkan parameter bool agar bisa dipakai buat hadap depan lagi nanti saat keluar
    public void SetDirection(bool isFront)
    {
        if (currentData == null) return;

        if (isFront)
        {
            // TAMPILAN DEPAN (Visual)
            if (currentData.rambut) rambutRenderer.sprite = currentData.rambut.visual;
            if (currentData.kepala) kepalaRenderer.sprite = currentData.kepala.visual;
            if (currentData.baju) bajuRenderer.sprite = currentData.baju.visual;
            if (currentData.celana) celanaRenderer.sprite = currentData.celana.visual;
            if (currentData.sepatu) sepatuRenderer.sprite = currentData.sepatu.visual;
        }
        else
        {
            // TAMPILAN BELAKANG (VisualBack)
            // Pastikan di Scriptable Object "Visual Back" sudah diisi gambarnya
            if (currentData.rambut && currentData.rambut.visualBack) rambutRenderer.sprite = currentData.rambut.visualBack;
            if (currentData.kepala && currentData.kepala.visualBack) kepalaRenderer.sprite = currentData.kepala.visualBack;
            if (currentData.baju && currentData.baju.visualBack) bajuRenderer.sprite = currentData.baju.visualBack;
            if (currentData.celana && currentData.celana.visualBack) celanaRenderer.sprite = currentData.celana.visualBack;
            if (currentData.sepatu && currentData.sepatu.visualBack) sepatuRenderer.sprite = currentData.sepatu.visualBack;
        }
    }

    public void AnimateFlip()
    {
        // 1. Stop semua animasi lain (misal lagi bernafas) biar tidak tabrakan
        LeanTween.cancel(gameObject);


        // 2. Animasi Mengecil (Scale X -> 0)
        LeanTween.scaleX(gameObject, 0f, 0.08f)
            .setEase(LeanTweenType.easeInElastic)
            .setOnComplete(() =>
            {



                LeanTween.scaleX(gameObject, 0.1889712f, 0.5f)
                    .setEase(LeanTweenType.easeOutElastic);
                MulaiBernafas();
            });
    }

    void MulaiBernafas()
    {
        float randomDelay = Random.Range(0f, 0.5f);

        // Animasi Y (Memanjang ke atas)
        LeanTween.scaleY(gameObject, 0.2f, 1.5f) // Durasi 1.5 detik
            .setDelay(randomDelay)
            .setEase(LeanTweenType.easeInOutSine) // Gerakan halus
            .setLoopPingPong(); // Ulangi bolak-balik selamanya

        // Animasi X (Menipis sedikit - Opsional, hapus kalau aneh)
        // LeanTween.scaleX(gameObject, 0.2f, 1.5f)
        //     .setDelay(randomDelay)
        //     .setEase(LeanTweenType.easeInOutSine)
        //     .setLoopPingPong();
    }

}