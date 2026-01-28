using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("======= Audio Source =======")]
    [SerializeField] AudioSource musicSource; // Untuk BGM & Ambiance
    [SerializeField] AudioSource SFXSource;   // Untuk SFX

    [Header("======= Music (BGM & Ambiance) =======")]
    public AudioClip Epilog;
    public AudioClip Lift;
    public AudioClip Mainmenu;
    public AudioClip Prolog;
    public AudioClip Ambiance1;
    public AudioClip Ambiance2;
    public AudioClip Ambiance3;
    public AudioClip Ambiance4;
    public AudioClip Ambiance5;

    [Header("======= SFX (Environment) =======")]
    public AudioClip Liftbukatutup;
    public AudioClip Liftbell;
    public AudioClip Shutter;

    [Header("======= SFX (Player) =======")]
    public AudioClip Nafasmin;
    public AudioClip Nafasmid;
    public AudioClip Nafasmax;

    [Header("======= SFX (Npc) =======")]
    public AudioClip NpcBatuk;
    public AudioClip NpcBersin;
    public AudioClip NpcGaruk;
    public AudioClip NpcHmm;
    public AudioClip NpcHitut;
    public AudioClip NpcLapar;
    public AudioClip NpcKetawa;
    public AudioClip NpcBukaMakanan;
    public AudioClip NpcMakan;
    public AudioClip NpcNgantuk;

    [Header("======= SFX (Randomized Variants) =======")]
    public AudioClip[] LiftgerakSfx;
    public AudioClip[] ButtonSfx;
    public AudioClip[] HimbauanSfx;
    public AudioClip[] LifttingtungSfx;
    public AudioClip[] Whoosh;

    // ==========================================================
    // LOGIKA TAMBAHAN (RANDOMISASI)
    // ==========================================================

    private void Start()
    {
        // Contoh memutar Main Menu saat start
        PlayMusic(Mainmenu);
    }

    // Fungsi Dasar Memutar Music (Looping)
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null) return;
        musicSource.clip = clip;
        musicSource.loop = true;
        musicSource.Play();
    }

    // Fungsi Dasar Memutar SFX Tunggal
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null) SFXSource.PlayOneShot(clip);
    }

    // --- FUNGSI RANDOMISASI UNTUK VARIANT ---

    public void PlayRandomLiftGerak() => PlayRandomFromList(LiftgerakSfx);
    public void PlayRandomButton() => PlayRandomFromList(ButtonSfx);
    public void PlayRandomHimbauan() => PlayRandomFromList(HimbauanSfx);
    public void PlayRandomTingTung() => PlayRandomFromList(LifttingtungSfx);
    public void PlayRandomWhoosh() => PlayRandomFromList(Whoosh);

    // Fungsi Reusable (Hanya bisa diakses di dalam script ini)
    private void PlayRandomFromList(AudioClip[] clips)
    {
        if (clips != null && clips.Length > 0)
        {
            int randomIndex = Random.Range(0, clips.Length);
            SFXSource.PlayOneShot(clips[randomIndex]);
        }
        else
        {
            Debug.LogWarning("Daftar SFX Random masih kosong di Inspector!");
        }
    }
}