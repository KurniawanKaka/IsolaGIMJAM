using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("======= Audio Source =======")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("======= Music Clip =======")]
    public AudioClip Background;

    [Header("======= SFX Lists =======")]
    public AudioClip[] LiftSfx;       // Untuk Button 1
    public AudioClip[] SecondSfxList;  // Untuk Button 2

    private void Start()
    {
        if (Background != null && musicSource != null)
        {
            musicSource.clip = Background;
            musicSource.Play();
        }
    }

    // Fungsi untuk Button 1
    public void PlayRandomLiftSFX()
    {
        ExecuteRandomSFX(LiftSfx);
    }

    // Fungsi untuk Button 2
    public void PlayRandomSecondSFX()
    {
        ExecuteRandomSFX(SecondSfxList);
    }

    // Logika internal (Ducking otomatis aktif karena lewat SFXSource)
    private void ExecuteRandomSFX(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0 && SFXSource != null)
        {
            int randomIndex = Random.Range(0, clips.Length);
            SFXSource.PlayOneShot(clips[randomIndex]);
        }
    }
}