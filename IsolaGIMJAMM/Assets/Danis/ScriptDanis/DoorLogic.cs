using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.UI;
using System.Collections;

public class DoorLogic : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextScene;
    public float animDuration = 1.0f;
    public float fadeDuration = 1.5f; // Sedikit dicepetin biar nggak dragging

    [Header("Camera Move")]
    public CinemachineVirtualCamera vCam;
    public float startZ = -9.06f;
    public float targetZ = -3f;

    [Header("UI Fade")]
    public Image fadeImage;

    private bool isZooming = false;
    private Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localEulerAngles;
        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.raycastTarget = false;
        }
    }

    void OnMouseEnter()
    {
        if (isZooming) return;
        LeanTween.cancel(gameObject);
        LeanTween.rotateLocal(gameObject, new Vector3(-90f, 0, -30f), 0.3f).setEaseOutBack();
    }

    void OnMouseExit()
    {
        if (isZooming) return;
        LeanTween.cancel(gameObject);
        LeanTween.rotateLocal(gameObject, initialRotation, 0.3f).setEaseOutQuad();
    }

    void OnMouseDown()
    {
        if (!isZooming && !string.IsNullOrEmpty(nextScene))
            StartCoroutine(PunchyTransition());
    }

    IEnumerator PunchyTransition()
    {
        isZooming = true;

        // 1. Pintu Terbuka (Pake Expo biar bukanya "kick" di awal)
        LeanTween.cancel(gameObject);
        LeanTween.rotateLocal(gameObject, new Vector3(-90f, 0, -75f), animDuration).setEaseInOutExpo();

        // 2. Kamera Maju & Fade (Pake EaseInExpo biar makin deket pintu makin kenceng)
        // Ini kuncinya biar nggak linear, bjir!
        LeanTween.value(gameObject, startZ, targetZ, fadeDuration)
            .setEaseInExpo()
            .setOnUpdate((float val) => {
                Vector3 currentPos = vCam.transform.localPosition;
                currentPos.z = val;
                vCam.transform.localPosition = currentPos;
            });

        if (fadeImage != null)
        {
            // Fadenya juga ikut dipercepat di akhir biar sinkron sama gerakan kamera
            LeanTween.value(fadeImage.gameObject, 0f, 1f, fadeDuration)
                .setEaseInQuart()
                .setOnUpdate((float alpha) => {
                    fadeImage.color = new Color(0, 0, 0, alpha);
                });
        }

        yield return new WaitForSeconds(fadeDuration + 0.3f);
        SceneManager.LoadScene(nextScene);
    }
}