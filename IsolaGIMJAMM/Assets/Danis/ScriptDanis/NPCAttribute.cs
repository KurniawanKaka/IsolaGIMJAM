using UnityEngine;
using System.Collections;

public class NPCAttribute : MonoBehaviour
{
    [Header("Info")]
    public string clueInfo;

    [Header("Settings")]
    public float fadeDuration = 0.5f; // Durasi fade (detik)

    private int defaultLayer;
    private int colorLayer;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();

        // Simpan layer awal objek
        defaultLayer = gameObject.layer;

        // Pastikan kamu sudah buat layer "ColoredReveal" di Unity
        colorLayer = LayerMask.NameToLayer("ColoredReveal");

        // Cek jika layer tidak ditemukan
        if (colorLayer == -1)
        {
            Debug.LogError($"Layer 'ColoredReveal' belum dibuat! Klik Edit > Project Settings > Tags and Layers.");
        }
    }

    public void RevealColor()
    {
        // Berhenti jika ada proses reveal yang sedang berjalan (biar gak numpuk)
        StopAllCoroutines();
        StartCoroutine(RevealProcess());
    }

    IEnumerator RevealProcess()
    {
        if (sr == null) yield break;

        // 1. Munculin Warna (Ganti Layer ke ColoredReveal)
        // Objek asli sekarang terlihat berwarna di kamera khusus
        gameObject.layer = colorLayer;

        // 2. Tahan 1 Detik (Fase Full Warna)
        yield return new WaitForSeconds(1.0f);

        // --- MULAI PROSES FADE OUT ---

        // A. Balikin objek asli ke B&W (Layer Default)
        gameObject.layer = defaultLayer;

        // B. Bikin "Ghost" (Kloningan Sementara) yang Berwarna
        GameObject ghost = new GameObject("ColorGhost_" + gameObject.name);
        ghost.transform.SetParent(transform);
        ghost.transform.localPosition = Vector3.zero;
        ghost.transform.localRotation = Quaternion.identity;
        ghost.transform.localScale = Vector3.one;
        ghost.layer = colorLayer;

        // C. Copy Tampilan Sprite ke Ghost
        SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
        ghostSr.sprite = sr.sprite;
        ghostSr.color = sr.color;
        ghostSr.flipX = sr.flipX;
        ghostSr.flipY = sr.flipY;
        ghostSr.sortingLayerID = sr.sortingLayerID;
        ghostSr.sortingOrder = sr.sortingOrder + 1; // Taruh sedikit di depan agar tidak flickering (Z-fight)

        // D. Animasi Fade Out pada Ghost
        float t = 0;
        Color startColor = ghostSr.color;

        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;

            // Kurangi Alpha (Transparansi) pelan-pelan dari 1 ke 0
            float alpha = Mathf.Lerp(startColor.a, 0, t);
            ghostSr.color = new Color(startColor.r, startColor.g, startColor.b, alpha);

            yield return null;
        }

        // E. Hapus Ghost (Bersih-bersih memori)
        Destroy(ghost);
    }
}