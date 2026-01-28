using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using UnityEngine.Rendering; // Library dasar Volume
using UnityEngine.Rendering.Universal; // Library khusus URP
using System.Collections;

public class DoorLogic : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextScene;
    public float animDuration = 0.8f;

    [Header("Camera Move")]
    public CinemachineVirtualCamera vCam;
    public float startZ = -9.06f;
    public float targetZ = -3f;

    [Header("Post Processing (URP)")]
    public Volume globalVolume; // Seret Global Volume (1) ke sini
    private Vignette vignette;
    private ColorAdjustments colorAdjustments;

    private bool isZooming = false;
    private Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localEulerAngles;

        // Ambil komponen dari Profile Volume URP
        if (globalVolume != null && globalVolume.profile.TryGet<Vignette>(out vignette))
        {
            vignette.intensity.value = 0.24f; // Nilai awal sesuai screenshotmu
        }

        if (globalVolume != null && globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            colorAdjustments.saturation.value = -100f; // Tetap BnW
        }

        // Set posisi Z awal kamera
        Vector3 pos = vCam.transform.localPosition;
        pos.z = startZ;
        vCam.transform.localPosition = pos;
    }

    void OnMouseEnter()
    {
        if (isZooming) return;
        LeanTween.rotateLocal(gameObject, new Vector3(-90f, 0, -25f), 0.4f).setEaseOutBack();
    }

    void OnMouseExit()
    {
        if (isZooming) return;
        LeanTween.rotateLocal(gameObject, initialRotation, 0.4f).setEaseOutQuad();
    }

    void OnMouseDown()
    {
        if (!isZooming && !string.IsNullOrEmpty(nextScene))
            StartCoroutine(DeepZoomSequence());
    }

    IEnumerator DeepZoomSequence()
    {
        isZooming = true;

        // 1. Pintu Terbuka
        LeanTween.rotateLocal(gameObject, new Vector3(-90f, 0, -60f), animDuration).setEaseInOutExpo();

        // 2. Kamera Maju Fisik (Z)
        LeanTween.value(gameObject, startZ, targetZ, animDuration + 0.5f)
            .setEaseInExpo()
            .setOnUpdate((float val) => {
                Vector3 currentPos = vCam.transform.localPosition;
                currentPos.z = val;
                vCam.transform.localPosition = currentPos;
            });

        // 3. Vignette Fade In (Makin Gelap ke angka 1)
        if (vignette != null)
        {
            LeanTween.value(gameObject, 0.45f, 1f, animDuration + 0.5f)
                .setOnUpdate((float val) => {
                    vignette.intensity.value = val;
                });
        }

        yield return new WaitForSeconds(animDuration + 1f);
        SceneManager.LoadScene(nextScene);
    }
}