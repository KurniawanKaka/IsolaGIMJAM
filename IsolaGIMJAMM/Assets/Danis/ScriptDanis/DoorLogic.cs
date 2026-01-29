using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.UI;
using System.Collections;

public class DoorLogic : MonoBehaviour
{
    [Header("Visual Target")]
    public GameObject doorVisual; // SERET OBJEK PINTU (YANG MUTER) KE SINI

    [Header("Scene Settings")]
    public string nextScene;
    public float animDuration = 1.0f;
    public float fadeDuration = 1.5f;

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
        // Ambil rotasi awal dari visualnya, bukan dari trigger
        if (doorVisual != null)
            initialRotation = doorVisual.transform.localEulerAngles;

        if (fadeImage != null)
        {
            fadeImage.color = new Color(0, 0, 0, 0);
            fadeImage.raycastTarget = false;
        }
    }

    void OnMouseEnter()
    {
        // Animasi diarahkan ke doorVisual
        if (isZooming || doorVisual == null) return;
        LeanTween.cancel(doorVisual);
        LeanTween.rotateLocal(doorVisual, new Vector3(-90f, 0, -40f), 0.3f).setEaseOutBack();
    }

    void OnMouseExit()
    {
        // Kembalikan rotasi doorVisual
        if (isZooming || doorVisual == null) return;
        LeanTween.cancel(doorVisual);
        LeanTween.rotateLocal(doorVisual, initialRotation, 0.3f).setEaseOutQuad();
    }

    void OnMouseDown()
    {
        if (!isZooming && !string.IsNullOrEmpty(nextScene))
            StartCoroutine(PunchyTransition());
    }

    IEnumerator PunchyTransition()
    {
        isZooming = true;

        if (doorVisual != null)
        {
            LeanTween.cancel(doorVisual);
            // Pintu terbuka penuh (Visualnya saja yang bergerak)
            LeanTween.rotateLocal(doorVisual, new Vector3(-90f, 0, -80f), 1.5f).setEaseInOutExpo();
        }

        // Gerakan Kamera
        LeanTween.value(gameObject, startZ, targetZ, fadeDuration)
            .setEaseInExpo()
            .setOnUpdate((float val) => {
                Vector3 currentPos = vCam.transform.localPosition;
                currentPos.z = val;
                vCam.transform.localPosition = currentPos;
            });

        // UI Fade
        if (fadeImage != null)
        {
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