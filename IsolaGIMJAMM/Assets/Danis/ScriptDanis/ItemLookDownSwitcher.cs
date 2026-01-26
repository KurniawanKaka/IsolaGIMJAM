using UnityEngine;

public class ItemLookDownSwitcher : MonoBehaviour
{
    [Header("References")]
    public PhotoMechanic photoScript;
    public Transform bookParent;

    [Header("Settings")]
    public float thresholdAngle = 47f; // Mentok di 50, jadi 47 sudah mulai switch
    public float transitionSpeed = 8f;

    [Header("Book Y Movement")]
    public float bookYHidden = -2.07f;
    public float bookYVisible = -0.846f;

    [Header("Cam Y Movement (Offset)")]
    public float camYOffsetMax = -1.5f;

    private float currentRatio = 0f;

    void Update()
    {
        // 1. Cek Pitch Kamera
        float pitch = transform.localEulerAngles.x;
        if (pitch > 180) pitch -= 360;

        // 2. Binary Switch Logic
        float targetRatio = (pitch >= thresholdAngle) ? 1f : 0f;

        // 3. Interpolasi Smooth
        if (currentRatio != targetRatio)
        {
            currentRatio = Mathf.MoveTowards(currentRatio, targetRatio, Time.deltaTime * transitionSpeed);
        }

        ApplyAnimation();
    }

    void ApplyAnimation()
    {
        // Pakai SmoothStep agar transisi kerasa premium
        float smoothCurve = currentRatio * currentRatio * (3f - 2f * currentRatio);

        // Update Posisi Buku
        float newBookY = Mathf.Lerp(bookYHidden, bookYVisible, smoothCurve);
        bookParent.localPosition = new Vector3(bookParent.localPosition.x, newBookY, bookParent.localPosition.z);

        // Update Offset Kamera
        photoScript.yOffset = Mathf.Lerp(0, camYOffsetMax, smoothCurve);

        // Lock Aim jika sedang nunduk
        photoScript.canAim = (currentRatio < 0.1f);
    }
}