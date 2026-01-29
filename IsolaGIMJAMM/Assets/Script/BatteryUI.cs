using UnityEngine;
using UnityEngine.UI;

public class BatteryUI : MonoBehaviour
{
    [Header("Referensi UI")]
    // Drag 5 Image Bar kamu ke sini (Urut dari Bar 1 sampai Bar 5)
    public GameObject[] batteryBars;

    [Header("Settings")]
    public Color healthyColor = Color.green;
    public Color criticalColor = Color.red;

    // Fungsi ini dipanggil setiap kali nyawa berubah
    public void UpdateBattery(int currentLives)
    {
        // Loop sebanyak jumlah Bar (5 kali)
        if (currentLives == 5)
        {
            batteryBars[4].SetActive(true);
        }
        if (currentLives == 4)
        {
            batteryBars[3].SetActive(true);
            batteryBars[4].SetActive(false);
        }
        if (currentLives == 3)
        {
            batteryBars[2].SetActive(true);
            batteryBars[3].SetActive(false);
        }
        if (currentLives == 2)
        {
            batteryBars[1].SetActive(true);
            batteryBars[2].SetActive(false);
        }
        if (currentLives == 1)
        {
            batteryBars[0].SetActive(true);
            batteryBars[1].SetActive(false);
        }
    }
}