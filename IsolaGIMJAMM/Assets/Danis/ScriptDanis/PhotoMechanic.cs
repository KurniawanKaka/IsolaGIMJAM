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

    [Header("Settings (Default Hardcoded)")]
    public string targetTag = "Suspect";
    public float detectionRadius = 25f;

    // Koordinat Posisi & Scale
    public Vector3 idlePos = new Vector3(0.333f, -0.184f, 0.4f);
    public Vector3 aimPos = new Vector3(0.115f, -0.116f, 0.4f);
    public Vector3 idleScale = new Vector3(0.172881f, 0.172881f, 0.172881f);
    public Vector3 aimScale = new Vector3(0.61393f, 0.61393f, 0.61393f);

    [Header("Animation Settings")]
    public float normalFOV = 60f;
    public float zoomFOV = 30f;
    public float animDuration = 0.35f;

    [Header("External Sync")]
    [HideInInspector] public float yOffset = 0f;
    [HideInInspector] public bool canAim = true;

    private bool isAiming, hasTakenPhoto;
    private Vector3 currentBasePos;

    void Start()
    {
        // Inisialisasi awal
        if (viewfinderObj != null) viewfinderObj.SetActive(false);
        if (flashPanel != null) flashPanel.color = Color.clear;

        currentBasePos = transform.localPosition = idlePos;
        transform.localScale = idleScale;

        if (overlayCam != null) overlayCam.fieldOfView = normalFOV;
    }

    void Update()
    {
        // Jika sudah foto, stop semua input (Kamera mati permanen)
        if (hasTakenPhoto) return;

        // Logika Bidik
        if (canAim)
        {
            if (Input.GetMouseButtonDown(1)) RunAim(true);
            if (Input.GetMouseButtonUp(1)) RunAim(false);

            // Input Motret: Hanya jika sedang bidik & viewfinder sudah aktif
            if (isAiming && viewfinderObj != null && viewfinderObj.activeSelf && Input.GetMouseButtonDown(0))
            {
                StartCoroutine(SequenceFoto());
            }
        }
        else if (isAiming)
        {
            RunAim(false);
        }
    }

    void LateUpdate()
    {
        // Update posisi visual: Base Animasi + Offset Nunduk
        transform.localPosition = currentBasePos + Vector3.up * yOffset;
    }

    public bool IsAiming() => isAiming;

    public void RunAim(bool enter)
    {
        isAiming = enter;
        LeanTween.cancel(gameObject);

        // Feedback visual awal
        if (!enter && viewfinderObj != null) viewfinderObj.SetActive(false);

        if (swayScript != null)
        {
            swayScript.canSway = !enter;
            swayScript.gameObject.SetActive(true);
        }

        // 1. Animasi POSISI - EaseInOutQuad agar "Seeeeet"
        LeanTween.value(gameObject, currentBasePos, enter ? aimPos : idlePos, animDuration)
            .setEaseInOutQuad()
            .setOnUpdateVector3((Vector3 val) => { currentBasePos = val; });

        // 2. Animasi SCALE
        LeanTween.scale(gameObject, enter ? aimScale : idleScale, animDuration)
            .setEaseInOutQuad();

        // 3. Animasi FOV
        LeanTween.value(gameObject, fpsCam.fieldOfView, enter ? zoomFOV : normalFOV, animDuration)
            .setEaseInOutQuad()
            .setOnUpdate((float val) => {
                fpsCam.fieldOfView = val;
                if (overlayCam != null) overlayCam.fieldOfView = val;

                // Threshold progres (0-1)
                float t = enter ?
                    Mathf.InverseLerp(normalFOV, zoomFOV, val) :
                    Mathf.InverseLerp(zoomFOV, normalFOV, val);

                // Aktifkan viewfinder jika zoom sudah 70%
                if (enter && t > 0.7f && viewfinderObj != null && !viewfinderObj.activeSelf)
                {
                    viewfinderObj.SetActive(true);
                    if (swayScript != null) swayScript.gameObject.SetActive(false);
                }
            });
    }

    IEnumerator SequenceFoto()
    {
        // Kunci status agar tidak bisa motret/aim lagi
        hasTakenPhoto = true;

        if (camController != null) camController.isLocked = true;

        // 1. Efek Flash
        flashPanel.color = Color.white;

        // 2. Jalankan Deteksi
        DetectSuspect();

        // Flash Fade Out
        float tFlash = 0;
        while (tFlash < 1)
        {
            tFlash += Time.deltaTime * 4f;
            flashPanel.color = new Color(1, 1, 1, 1 - tFlash);
            yield return null;
        }
        flashPanel.color = Color.clear;

        // 3. Freeze Dramatis
        yield return new WaitForSeconds(1.0f);

        if (camController != null) camController.isLocked = false;

        // 4. TURUN OTOMATIS & MATI TOTAL
        canAim = false;
        RunAim(false);

        Debug.Log("Foto selesai. Kamera dinonaktifkan untuk level ini.");
    }

    void DetectSuspect()
    {
        GameObject[] suspects = GameObject.FindGameObjectsWithTag(targetTag);
        GameObject bestTarget = null;
        float maxArea = 0f;
        Rect viewRect = new Rect(Screen.width * 0.25f, Screen.height * 0.25f, Screen.width * 0.5f, Screen.height * 0.5f);

        foreach (GameObject obj in suspects)
        {
            float dist = Vector3.Distance(fpsCam.transform.position, obj.transform.position);
            if (dist > detectionRadius) continue;

            Vector3 dirToObj = (obj.transform.position - fpsCam.transform.position).normalized;
            if (Vector3.Dot(fpsCam.transform.forward, dirToObj) < 0.5f) continue;

            // Raycast agar tidak tembus dinding
            RaycastHit hit;
            if (Physics.Raycast(fpsCam.transform.position, dirToObj, out hit, detectionRadius))
            {
                if (!hit.collider.CompareTag(targetTag) && hit.collider.gameObject != obj) continue;
            }

            Collider2D col2D = obj.GetComponent<Collider2D>();
            if (col2D == null) continue;

            float area = CalculateOverlap2D(col2D, viewRect);
            if (area > maxArea)
            {
                maxArea = area;
                bestTarget = obj;
            }
        }

        if (bestTarget != null && maxArea > 0)
        {
            Debug.Log("<color=green>SUCCESS:</color> " + bestTarget.name);
            bestTarget.GetComponent<NPCAttribute>()?.RevealColor();
        }
        else
        {
            Debug.Log("<color=red>FAILED:</color> Target tidak ditemukan di frame.");
        }
    }

    float CalculateOverlap2D(Collider2D col, Rect viewRect)
    {
        Bounds b = col.bounds;
        Vector3 c = b.center, e = b.extents;
        Vector3[] worldPoints = {
            c + new Vector3(-e.x, -e.y, -e.z), c + new Vector3(e.x, -e.y, -e.z),
            c + new Vector3(-e.x, e.y, -e.z), c + new Vector3(e.x, e.y, -e.z),
            c + new Vector3(-e.x, -e.y, e.z), c + new Vector3(e.x, -e.y, e.z),
            c + new Vector3(-e.x, e.y, e.z), c + new Vector3(e.x, e.y, e.z)
        };

        float minX = Mathf.Infinity, minY = Mathf.Infinity, maxX = -Mathf.Infinity, maxY = -Mathf.Infinity;
        int visibleCount = 0;

        foreach (Vector3 wp in worldPoints)
        {
            Vector3 sP = fpsCam.WorldToScreenPoint(wp);
            if (sP.z < 0) continue;
            minX = Mathf.Min(minX, sP.x); minY = Mathf.Min(minY, sP.y);
            maxX = Mathf.Max(maxX, sP.x); maxY = Mathf.Max(maxY, sP.y);
            visibleCount++;
        }

        if (visibleCount == 0) return 0;
        Rect objRect = Rect.MinMaxRect(minX, minY, maxX, maxY);
        float xOv = Mathf.Max(0, Mathf.Min(objRect.xMax, viewRect.xMax) - Mathf.Max(objRect.xMin, viewRect.xMin));
        float yOv = Mathf.Max(0, Mathf.Min(objRect.yMax, viewRect.yMax) - Mathf.Max(objRect.yMin, viewRect.yMin));
        return xOv * yOv;
    }
}