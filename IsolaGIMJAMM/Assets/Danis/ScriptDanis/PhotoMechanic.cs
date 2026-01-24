using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotoMechanic : MonoBehaviour
{
    [Header("Cameras")]
    public Camera fpsCam;       // Drag MAIN CAMERA
    public Camera overlayCam;   // Drag OVERLAY CAMERA (Si Kamera Warna)

    [Header("Controller")]
    public FPSCameraController camController;

    [Header("Visuals")]
    public Transform handSprite;
    public GameObject viewfinderObj;
    public Image flashPanel;

    [Header("Settings")]
    public string targetTag = "Suspect";
    public float detectionRadius = 25f;
    public Vector3 idlePos, aimPos;
    public float normalFOV = 60f, zoomFOV = 30f, animDuration = 0.4f;

    private bool isAiming, hasTakenPhoto;
    private Coroutine aimRoutine, flashRoutine;

    void Start()
    {
        viewfinderObj.SetActive(false);
        flashPanel.color = Color.clear;

        // Samain settingan awal overlay cam
        if (overlayCam != null) overlayCam.fieldOfView = normalFOV;
    }

    void Update()
    {
        if (hasTakenPhoto) return;

        if (Input.GetMouseButtonDown(1)) RunAim(true);
        if (Input.GetMouseButtonUp(1)) RunAim(false);

        if (isAiming && viewfinderObj.activeSelf && Input.GetMouseButtonDown(0))
            StartCoroutine(SequenceFoto());
    }

    // --- BAGIAN UTAMA YANG DIPERBAIKI ---
    IEnumerator AimProc(bool enter)
    {
        isAiming = enter; float t = 0;

        if (!enter) { viewfinderObj.SetActive(false); handSprite.gameObject.SetActive(true); }

        Vector3 sP = handSprite.localPosition, eP = enter ? aimPos : idlePos;
        float sF = fpsCam.fieldOfView, eF = enter ? zoomFOV : normalFOV;

        while (t < 1)
        {
            t += Time.deltaTime / animDuration;

            // 1. Gerakin Tangan
            handSprite.localPosition = Vector3.Lerp(sP, eP, t);

            // 2. Ubah FOV Kamera Utama
            float currentFOV = Mathf.Lerp(sF, eF, t);
            fpsCam.fieldOfView = currentFOV;

            // 3. AUTO SYNC: Paksa Kamera Overlay ikut nge-zoom
            if (overlayCam != null) overlayCam.fieldOfView = currentFOV;

            yield return null;
        }

        // Pastikan angka akhirnya pas
        fpsCam.fieldOfView = eF;
        if (overlayCam != null) overlayCam.fieldOfView = eF;

        if (enter) { viewfinderObj.SetActive(true); handSprite.gameObject.SetActive(false); }
    }

    // --- LOGIKA FOTO & DETEKSI TETAP SAMA ---
    IEnumerator SequenceFoto()
    {
        hasTakenPhoto = true;
        camController.isLocked = true;

        flashPanel.color = Color.white;
        DetectBestObject2D();

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 2f;
            flashPanel.color = new Color(1, 1, 1, 1 - t);
            yield return null;
        }
        flashPanel.color = Color.clear;

        yield return new WaitForSeconds(1.5f);

        camController.isLocked = false;
        RunAim(false);
        this.enabled = false;
    }

    void DetectBestObject2D()
    {
        Collider2D[] hits = Physics2D.OverlapCircleAll(fpsCam.transform.position, detectionRadius);
        Collider2D bestTarget = null;
        float maxArea = 0f;
        Rect viewRect = new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.5f);

        foreach (Collider2D col in hits)
        {
            if (!col.CompareTag(targetTag)) continue;
            Vector3 dir = (col.transform.position - fpsCam.transform.position).normalized;
            if (Vector3.Dot(fpsCam.transform.forward, dir) < 0.5f) continue;

            float area = CalculateOverlap2D(col, viewRect);
            if (area > maxArea) { maxArea = area; bestTarget = col; }
        }

        if (bestTarget != null) bestTarget.GetComponent<NPCAttribute>()?.RevealColor();
    }

    float CalculateOverlap2D(Collider2D col, Rect viewRect)
    {
        Bounds b = col.bounds;
        Vector3[] p = new Vector3[8];
        Vector3 c = b.center, e = b.extents;
        p[0] = fpsCam.WorldToScreenPoint(c + new Vector3(-e.x, -e.y, -e.z));
        p[1] = fpsCam.WorldToScreenPoint(c + new Vector3(e.x, -e.y, -e.z));
        p[2] = fpsCam.WorldToScreenPoint(c + new Vector3(-e.x, e.y, -e.z));
        p[3] = fpsCam.WorldToScreenPoint(c + new Vector3(e.x, e.y, -e.z));
        p[4] = fpsCam.WorldToScreenPoint(c + new Vector3(-e.x, -e.y, e.z));
        p[5] = fpsCam.WorldToScreenPoint(c + new Vector3(e.x, -e.y, e.z));
        p[6] = fpsCam.WorldToScreenPoint(c + new Vector3(-e.x, e.y, e.z));
        p[7] = fpsCam.WorldToScreenPoint(c + new Vector3(e.x, e.y, e.z));

        float minX = Mathf.Infinity, minY = Mathf.Infinity, maxX = -Mathf.Infinity, maxY = -Mathf.Infinity;
        foreach (Vector3 v in p) { minX = Mathf.Min(minX, v.x); minY = Mathf.Min(minY, v.y); maxX = Mathf.Max(maxX, v.x); maxY = Mathf.Max(maxY, v.y); }

        Rect objRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
        float xOv = Mathf.Max(0, Mathf.Min(objRect.xMax, viewRect.xMax) - Mathf.Max(objRect.xMin, viewRect.xMin));
        float yOv = Mathf.Max(0, Mathf.Min(objRect.yMax, viewRect.yMax) - Mathf.Max(objRect.yMin, viewRect.yMin));
        return xOv * yOv;
    }

    void RunAim(bool enter)
    {
        if (aimRoutine != null) StopCoroutine(aimRoutine);
        aimRoutine = StartCoroutine(AimProc(enter));
        if (!enter && !hasTakenPhoto) { if (flashRoutine != null) StopCoroutine(flashRoutine); flashPanel.color = Color.clear; }
    }
}