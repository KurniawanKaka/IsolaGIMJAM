using UnityEngine;
using UnityEngine.UI;

public partial class AudioTestTrigger : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private AudioManager audioManager;

    // Kita panggil AudioClip yang ingin ditest (misal: Shutter)
    public void TestSFX()
    {
        if (audioManager != null)
        {
            // Memanggil fungsi PlaySFX dan mengirimkan clip 'Shutter'
            audioManager.PlaySFX(audioManager.Shutter);
            Debug.Log("SFX Test: Button Clicked!");
        }
        else
        {
            Debug.LogWarning("AudioManager belum dimasukkan ke dalam slot Inspector!");
        }
    }
}