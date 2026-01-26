using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;
using System.Collections;

public class DoorLogic : MonoBehaviour
{
    [Header("Settings")]
    public Sprite openSprite;
    public Sprite closedSprite;
    public string nextScene;

    [Header("Camera")]
    public CinemachineVirtualCamera vCam;
    public float zoomSpeed = 40f; // Kecepatan zoom
    public float targetFOV = 20f; // Seberapa nge-zoom (makin kecil makin dekat)

    private SpriteRenderer sr;
    private bool isZooming = false;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void OnMouseEnter()
    {
        if (!isZooming) sr.sprite = openSprite;
    }

    void OnMouseExit()
    {
        if (!isZooming) sr.sprite = closedSprite;
    }

    void OnMouseDown()
    {
        if (!isZooming) StartCoroutine(ZoomAndSwitch());
    }

    IEnumerator ZoomAndSwitch()
    {
        isZooming = true;

        // --- LOGIC BARU UNTUK 3D ---
        // Kita kecilkan Field Of View (FOV) biar seolah-olah kamera maju mendekat
        while (vCam.m_Lens.FieldOfView > targetFOV)
        {
            vCam.m_Lens.FieldOfView -= (zoomSpeed * Time.deltaTime);
            yield return null;
        }

        SceneManager.LoadScene(nextScene);
    }
}