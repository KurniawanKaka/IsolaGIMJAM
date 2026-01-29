using UnityEngine;
using UnityEngine.Video;
using UnityEngine.SceneManagement;
using UnityEngine.UI; // Buat akses Image
using System.Collections;

public class LoadingVideo : MonoBehaviour
{
    public string nextSceneName;
    public Image overlay; // Drag Image Hitam (Cover) ke sini
    public float delayBeforeFade = 0.5f; // Jeda biar video bener-bener 'stabil' dulu

    private VideoPlayer vPlayer;
    private bool isDone = false;

    void Awake()
    {
        vPlayer = GetComponent<VideoPlayer>();
        vPlayer.playOnAwake = false; // Kita kendalikan manual

        // Paksa overlay item pekat di awal
        if (overlay != null)
        {
            overlay.color = Color.black;
            overlay.gameObject.SetActive(true);
        }

        vPlayer.Prepare();
    }

    void Start()
    {
        vPlayer.loopPointReached += (vp) => StartCoroutine(EndTransition());
        StartCoroutine(StartVideoSequence());
    }

    IEnumerator StartVideoSequence()
    {
        // 1. Tunggu video siap dimuat
        while (!vPlayer.isPrepared) yield return null;

        // 2. Play video SAAT OVERLAY MASIH ITEM PEKAT
        vPlayer.Play();

        // 3. JEDA KRUSIAL: Biar video lewat dari frame 0 yang biasanya bikin kedip
        yield return new WaitForSeconds(delayBeforeFade);

        // 4. Fade out overlay (Video sekarang sudah jalan mulus di belakang)
        if (overlay != null)
        {
            LeanTween.alpha(overlay.rectTransform, 0f, 0.7f).setEase(LeanTweenType.easeOutQuad);
        }
    }

    IEnumerator EndTransition()
    {
        if (isDone) yield break;
        isDone = true;

        // Fade in lagi ke item sebelum ganti scene biar smooth
        if (overlay != null)
        {
            LeanTween.alpha(overlay.rectTransform, 1f, 0.5f).setEase(LeanTweenType.easeInQuad);
            yield return new WaitForSeconds(0.6f);
        }

        SceneManager.LoadScene(nextSceneName);
    }
}