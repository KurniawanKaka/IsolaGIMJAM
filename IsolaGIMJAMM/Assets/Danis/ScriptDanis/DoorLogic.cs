using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System.Collections;

public class DoorLogic : MonoBehaviour
{
    [Header("Scene Settings")]
    public string nextScene;
    public float animDuration = 0.8f;

    [Header("Camera Object")]
    public CinemachineVirtualCamera vCam;

    // Titik Z awal dan akhir yang kamu mau
    public float startZ = -9.06f;
    public float targetZ = -3f;

    private bool isZooming = false;
    private Vector3 initialRotation;

    void Start()
    {
        initialRotation = transform.localEulerAngles;

        // Set posisi awal kamera secara fisik
        Vector3 pos = vCam.transform.localPosition;
        pos.z = startZ;
        vCam.transform.localPosition = pos;
    }

    void OnMouseEnter()
    {
        if (isZooming) return;
        LeanTween.cancel(gameObject);
        // Hover: Pintu terbuka dikit
        LeanTween.rotateLocal(gameObject, new Vector3(-90f, 0, 25f), 0.4f).setEaseOutBack();
    }

    void OnMouseExit()
    {
        if (isZooming) return;
        LeanTween.cancel(gameObject);
        LeanTween.rotateLocal(gameObject, initialRotation, 0.4f).setEaseOutQuad();
    }

    void OnMouseDown()
    {
        if (!isZooming) StartCoroutine(PhysicalMoveAndSwitch());
    }

    IEnumerator PhysicalMoveAndSwitch()
    {
        isZooming = true;

        // 1. Animasi Pintu
        LeanTween.cancel(gameObject);
        LeanTween.rotateLocal(gameObject, new Vector3(-90f, 0, 110f), animDuration).setEaseInOutExpo();

        // 2. Animasi Pergerakan Kamera Fisik (Z)
        // Kita gerakkan transform.localPosition-nya langsung
        LeanTween.value(gameObject, startZ, targetZ, animDuration + 0.5f)
            .setEaseInExpo() // Pake EaseIn biar makin deket pintu makin cepet
            .setOnUpdate((float val) => {
                Vector3 currentPos = vCam.transform.localPosition;
                currentPos.z = val;
                vCam.transform.localPosition = currentPos;
            });

        yield return new WaitForSeconds(animDuration + 0.8f);

        SceneManager.LoadScene(nextScene);
    }
}