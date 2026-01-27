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
    public float bookYHidden = -0.407f;
    public float bookYVisible = 0.013f;
    public float camYOffsetMax = -0.21f;

    public bool isFeatureUnlocked = false;



    private float currentRatio = 0f;
    private Coroutine transitionRoutine;

    void Start()
    {

    }

    public void UnlockCameraFeature()
    {
        Debug.Log("Switcher: Fitur Kamera Dibuka.");
        isFeatureUnlocked = true;

        // Cek posisi kepala sekarang, kalau lagi lurus (tidak nunduk), langsung nyalakan
        OnAimReleased();
    }

    // 2. UNTUK MEMATIKAN KAMERA (Lock)
    public void LockCameraFeature()
    {
        Debug.Log("Switcher: Fitur Kamera Dikunci.");
        isFeatureUnlocked = false;

        // Matikan paksa di PhotoMechanic
        if (photoScript != null)
        {
            photoScript.SetCanAim(false);

            // Jika sedang membidik, paksa turun
            if (photoScript.IsAiming()) photoScript.RunAim(false);
        }
    }

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
            if (photoScript.IsAiming() && isFeatureUnlocked)
            {
                target = 0f;
                photoScript.SetCanAim(true);
            }

            currentRatio = Mathf.MoveTowards(currentRatio, target, Time.deltaTime * transitionSpeed);

            float smoothCurve = currentRatio * currentRatio * (3f - 2f * currentRatio);

            // Update Visual
            bookParent.localPosition = new Vector3(bookParent.localPosition.x, Mathf.Lerp(bookYHidden, bookYVisible, smoothCurve), bookParent.localPosition.z);
            photoScript.yOffset = Mathf.Lerp(0, camYOffsetMax, smoothCurve);

            if (target == 0f && currentRatio < 0.1f)
            {
                if (isFeatureUnlocked) photoScript.SetCanAim(true);
                else photoScript.SetCanAim(false);
            }

            yield return null;
        }
        transitionRoutine = null;
    }
}