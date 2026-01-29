using UnityEngine;
using UnityEngine.UI;

public class FadeIn : MonoBehaviour
{
    public float duration = 0.5f; // Durasi sesuai request kamu
    private Image img;

    void Awake()
    {
        img = GetComponent<Image>();

        // Pastikan di awal banget (sebelum frame 1), alpha-nya hitam pekat
        if (img != null)
        {
            Color c = img.color;
            c.a = 1f;
            img.color = c;
        }
    }

    void Start()
    {
        if (img != null)
        {
            // Pake LeanTween buat nurunin Alpha ke 0
            LeanTween.value(gameObject, 1f, 0f, duration)
                .setEase(LeanTweenType.easeOutQuad)
                .setOnUpdate((float val) => {
                    Color c = img.color;
                    c.a = val;
                    img.color = c;
                })
                .setOnComplete(() => {
                    // Opsional: Matikan object biar hemat performa setelah fade kelar
                    gameObject.SetActive(false);
                });
        }
    }
}