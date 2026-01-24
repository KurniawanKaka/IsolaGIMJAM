
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("======= Audio Source =======")]
    [SerializeField] AudioSource musicSource;
    [SerializeField] AudioSource SFXSource;

    [Header("======= Audio Clip =======")]
    public AudioClip Background;
    public AudioClip Shutter;

    public void Start()
    {
        musicSource.clip = Background;
        musicSource.Play();

    }

    public void PlaySFX(AudioClip clip)
    {
        SFXSource.PlayOneShot(clip);
    }

   
}

