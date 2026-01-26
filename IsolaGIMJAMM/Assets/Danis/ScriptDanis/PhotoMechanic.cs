using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PhotoMechanic : MonoBehaviour
{
    [Header("Cameras")]
    public Camera fpsCam;
    public Camera overlayCam;
    public FPSCameraController camController;

    [Header("Visuals")]
    public ObjectSway swayScript;
    public GameObject viewfinderObj;
    public Image flashPanel;

    [Header("Settings")]
    public string targetTag = "Suspect";
    public float detectionRadius = 25f;
    public Vector3 idlePos, aimPos;
    public float normalFOV = 60f, zoomFOV = 30f, animDuration = 0.3f;

    [Header("External Sync")]
    [HideInInspector] public float yOffset = 0f; // Dimanipulasi oleh ItemLookDownSwitcher
    [HideInInspector] public bool canAim = true; // Dikunci oleh ItemLookDownSwitcher saat nunduk

    private bool isAiming, hasTakenPhoto;
    private Coroutine aimRoutine;
    private Vector3 currentBasePos; // Posisi dasar (Idle/Aim) sebelum ditambah offset nunduk

    void Start()
    {
        viewfinderObj.SetActive(false);
        flashPanel.color = Color.clear;

        currentBasePos = idlePos;
        transform.localPosition = idlePos;

        if (overlayCam != null) overlayCam.fieldOfView = normalFOV;
    }

    void Update()
    {
        // Jika sudah jepret, input bidik dimatikan, tapi LateUpdate tetap jalan
        if (hasTakenPhoto) return;

        // Cek apakah boleh bidik (tidak sedang nunduk mentok)
        if (canAim)
        {
            if (Input.GetMouseButtonDown(1)) RunAim(true);
            if (Input.GetMouseButtonUp(1)) RunAim(false);
        }
        else if (isAiming)
        {
            // Paksa turun jika tiba-tiba nunduk saat bidik
            RunAim(false);
        }

        // Ambil Foto
        if (isAiming && viewfinderObj.activeSelf && Input.GetMouseButtonDown(0))
        {
            StartCoroutine(SequenceFoto());
        }
    }

    void LateUpdate()
    {
        // INI KUNCINYA: Posisi selalu diupdate berdasarkan base (Aim/Idle) + offset nunduk
        // Tetap berjalan meskipun hasTakenPhoto = true
        transform.localPosition = new Vector3(
            currentBasePos.x,
            currentBasePos.y + yOffset,
            currentBasePos.z
        );
    }

    public void RunAim(bool enter)
    {
        if (aimRoutine != null) StopCoroutine(aimRoutine);
        aimRoutine = StartCoroutine(AimProc(enter));
    }

    IEnumerator AimProc(bool enter)
    {
        isAiming = enter;
        float t = 0;

        if (swayScript != null) swayScript.canSway = false;
        viewfinderObj.SetActive(false);
        if (swayScript != null) swayScript.gameObject.SetActive(true);

        Vector3 startPos = currentBasePos;
        Vector3 targetPos = enter ? aimPos : idlePos;
        float startFOV = fpsCam.fieldOfView;
        float targetFOV = enter ? zoomFOV : normalFOV;

        while (t < 1)
        {
            t += Time.deltaTime / animDuration;
            float curve = t * t * (3f - 2f * t);

            currentBasePos = Vector3.Lerp(startPos, targetPos, curve);

            float fov = Mathf.Lerp(startFOV, targetFOV, curve);
            fpsCam.fieldOfView = fov;
            if (overlayCam != null) overlayCam.fieldOfView = fov;

            yield return null;
        }

        currentBasePos = targetPos;

        if (enter)
        {
            viewfinderObj.SetActive(true);
            if (swayScript != null) swayScript.gameObject.SetActive(false);
        }
        else
        {
            if (swayScript != null) swayScript.canSway = true;
        }
    }

    IEnumerator SequenceFoto()
    {
        hasTakenPhoto = true;
        if (camController != null) camController.isLocked = true;

        flashPanel.color = Color.white;
        DetectBestObject2D();

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 3f;
            flashPanel.color = new Color(1, 1, 1, 1 - t);
            yield return null;
        }
        flashPanel.color = Color.clear;

        yield return new WaitForSeconds(1.5f);

        if (camController != null) camController.isLocked = false;

        // Turunkan kamera ke posisi idle tangan
        RunAim(false);

        // Tunggu transisi turun selesai
        yield return new WaitForSeconds(animDuration + 0.1f);

        // KITA HAPUS this.enabled = false;
        // Sekarang script tetap hidup, LateUpdate tetap jalan,
        // tapi hasTakenPhoto mengunci input bidik/foto.
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
        p[3] = fpsCam.WorldToScreenPoint(c + new Vector3(e.x, e.y, e.z));
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
}