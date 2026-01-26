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
    private SpriteRenderer sr; // Kita butuh ini buat nge-copy sprite ke Ghost

    void Start()
    {
        sr = GetComponent<SpriteRenderer>(); // Ambil komponen Sprite
        defaultLayer = gameObject.layer;
        colorLayer = LayerMask.NameToLayer("ColoredReveal");
    }

    public void RevealColor()
    {
        StopAllCoroutines();
        StartCoroutine(RevealProcess());
    }

    IEnumerator RevealProcess()
    {
        // 1. Munculin Warna (Ganti Layer)
        gameObject.layer = colorLayer;

        // 2. Tahan 1 Detik (Full Warna)
        yield return new WaitForSeconds(1.0f);

        // --- MULAI PROSES FADE OUT ---

        // A. Balikin aslinya ke B&W dulu (Layer Default)
        // Visually: Di kamera utama dia sudah jadi hitam putih
        gameObject.layer = defaultLayer;

        // B. Bikin "Ghost" (Kloningan Sementara) yang Berwarna
        // Tujuannya: Menutupi si Hitam Putih, lalu menghilang pelan-pelan
        GameObject ghost = new GameObject("ColorGhost");
        ghost.transform.SetParent(transform); // Tempel ke NPC biar kalau gerak ikut
        ghost.transform.localPosition = Vector3.zero;
        ghost.transform.localRotation = Quaternion.identity;
        ghost.transform.localScale = Vector3.one;
        ghost.layer = colorLayer; // Ghost ini yang berwarna (Overlay Camera)

        // C. Copy Tampilan Sprite ke Ghost
        SpriteRenderer ghostSr = ghost.AddComponent<SpriteRenderer>();
        ghostSr.sprite = sr.sprite;
        ghostSr.color = sr.color; // Copy warna merah/kuning dari inspector
        ghostSr.flipX = sr.flipX;
        ghostSr.flipY = sr.flipY;
        ghostSr.sortingLayerID = sr.sortingLayerID;
        ghostSr.sortingOrder = sr.sortingOrder; // Urutan sama ga masalah karena beda kamera

        // D. Lakukan Animasi Fade Out pada Ghost
        float t = 0;
        Color startColor = ghostSr.color;

        while (t < 1)
        {
            t += Time.deltaTime / fadeDuration;
            // Kurangi Alpha (Transparansi) pelan-pelan dari 1 ke 0
            ghostSr.color = new Color(startColor.r, startColor.g, startColor.b, Mathf.Lerp(startColor.a, 0, t));
            yield return null;
        }

        // E. Hapus Ghost (Bersih-bersih)
        Destroy(ghost);
    }
}