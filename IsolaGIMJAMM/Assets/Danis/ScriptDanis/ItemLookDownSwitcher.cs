using UnityEngine;
using System.Collections;

public class ItemLookDownSwitcher : MonoBehaviour
{
    [Header("References")]
    public PhotoMechanic photoScript;
    public Transform bookParent;

    [Header("Settings")]
    public float thresholdAngle = 47f;
    public float transitionSpeed = 8f;

    [Header("Positions")]
    public float bookYHidden = -0.3892f;
    public float bookYVisible = -0.179f;
    public float camYOffsetMax = -0.21f;

    private float currentRatio = 0f;
    private Coroutine transitionRoutine;

    // Dipanggil dari FPSCameraController
    public void CheckPitch(float currentPitch)
    {
        float target = (currentPitch >= thresholdAngle && !photoScript.IsAiming()) ? 1f : 0f;

        if (target != currentRatio && transitionRoutine == null)
        {
            transitionRoutine = StartCoroutine(SmoothTransition(target));
        }
    }

    // Dipanggil saat PhotoMechanic lepas klik kanan
    public void OnAimReleased()
    {
        // Re-check pitching saat bidik dilepas
        float pitch = transform.localEulerAngles.x;
        if (pitch > 180) pitch -= 360;
        CheckPitch(pitch);
    }

    IEnumerator SmoothTransition(float target)
    {
        while (!Mathf.Approximately(currentRatio, target))
        {
            // Jika tiba-tiba bidik pas transisi buku naik, batalkan!
            if (photoScript.IsAiming()) target = 0f;

            currentRatio = Mathf.MoveTowards(currentRatio, target, Time.deltaTime * transitionSpeed);

            float smoothCurve = currentRatio * currentRatio * (3f - 2f * currentRatio);

            // Update Visual
            bookParent.localPosition = new Vector3(bookParent.localPosition.x, Mathf.Lerp(bookYHidden, bookYVisible, smoothCurve), bookParent.localPosition.z);
            photoScript.yOffset = Mathf.Lerp(0, camYOffsetMax, smoothCurve);

            // Sync canAim
            photoScript.canAim = (currentRatio < 0.1f);

            yield return null;
        }
        transitionRoutine = null;
    }
}