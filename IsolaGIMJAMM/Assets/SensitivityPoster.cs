using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SensitivityPoster : MonoBehaviour
{
    [Header("References")]
    public Slider sliderUI;
    public FPSCameraController camController;
    public TextMeshProUGUI valueText;

    [Header("Settings")]
    public float minSens = 0.5f;
    public float maxSens = 5.0f;
    public float scrollStep = 0.05f;

    // Cek Referensi saat Start
    void Start()
    {
        if (sliderUI == null) Debug.LogError($"ERROR di {gameObject.name}: Slot 'Slider UI' KOSONG!");
        if (camController == null) Debug.LogError($"ERROR di {gameObject.name}: Slot 'Cam Controller' KOSONG!");

        // Update tampilan awal
        if (camController != null && sliderUI != null)
        {
            float currentSens = camController.mouseSensitivity;
            sliderUI.value = Mathf.InverseLerp(minSens, maxSens, currentSens);
            UpdateText(currentSens);
        }
    }

    public void ScrollSensitivity(float scrollAmount)
    {
        Debug.Log($"DEBUG POSTER: Menerima Input Scroll {scrollAmount}");

        if (sliderUI == null || camController == null)
        {
            Debug.LogError("ERROR POSTER: Referensi Slider atau CamController hilang!");
            return;
        }

        // Logic Scroll
        float newValue = sliderUI.value + (scrollAmount * scrollStep);
        newValue = Mathf.Clamp01(newValue);
        sliderUI.value = newValue;

        // Update ke Player
        float newSens = Mathf.Lerp(minSens, maxSens, newValue);
        camController.mouseSensitivity = newSens;

        Debug.Log($"DEBUG POSTER: Sensitivitas diubah jadi {newSens}");
        UpdateText(newSens);
    }

    void UpdateText(float val)
    {
        if (valueText != null) valueText.text = val.ToString("F1");
    }
}