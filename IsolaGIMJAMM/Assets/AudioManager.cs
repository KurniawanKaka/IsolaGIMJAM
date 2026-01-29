using System.Collections;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    [Header("======= Audio Source =======")]
    [SerializeField] public AudioSource musicSource; // Untuk BGM & Ambiance
    [SerializeField] public AudioSource SFXSource;   // Untuk SFX

    [Header("======= Music (BGM & Ambiance) =======")]
    public AudioClip Epilog;
    public AudioClip Lift;
    public AudioClip Mainmenu;
    public AudioClip Prolog;
    public AudioClip shutter;

    public AudioClip Ambiance4;

    [Header("======= SFX (Environment) =======")]
    public AudioClip Liftbuka, liftutup, tingtung, vokaka, notif, marah;
    public AudioClip Liftbell;
    public AudioClip Shutter;
    public AudioClip PintuBuka;
    public AudioClip PintuDikit;
    public AudioClip PintuTutup;

    [Header("======= SFX (Player) =======")]
    public AudioClip Nafasmin;
    public AudioClip Nafasmid;
    public AudioClip Nafasmax;



    [Header("======= SFX (Randomized Variants) =======")]

    public AudioClip[] npc;
    public AudioClip[] AmbienceSfx;
    public AudioClip[] ButtonSfx;


    public AudioClip[] Whoosh;

    // ==========================================================
    // LOGIKA TAMBAHAN (RANDOMISASI)
    // ==========================================================

    private void Start()
    {
        PlayMusic(Lift);
    }

    public void PlayRandomLoopingSFX(AudioClip[] clips, float targetVolume = 1f, float fadeDuration = 1f)
    {
        if (clips == null || clips.Length == 0)
        {
            Debug.LogWarning("PlayRandomLoopingSFX: Daftar Clip Kosong!");
            return;
        }

        // A. Cek apakah ada salah satu clip dari list ini yang sedang bunyi?
        // (Opsional: Biar tidak tumpang tindih suara lift yang sama)
        if (IsAnyClipPlayingFromList(clips)) return;


        // B. Pilih Random
        int randomIndex = Random.Range(0, clips.Length);
        AudioClip selectedClip = clips[randomIndex];

        // C. Panggil fungsi PlayLoopingSFX yang sudah kita buat sebelumnya
        // (Kita "numpang" logika yang sudah ada biar hemat kode)
        PlayLoopingSFX(selectedClip, targetVolume, fadeDuration);
    }

    // 2. STOP RANDOM: Matikan APAPUN clip dari list tersebut yang sedang bunyi
    public void StopRandomLoopingSFX(AudioClip[] clips, float fadeDuration = 1f)
    {
        if (clips == null || clips.Length == 0) return;

        // Ambil semua AudioSource yang aktif
        AudioSource[] allSources = GetComponents<AudioSource>();

        foreach (var source in allSources)
        {
            // Abaikan BGM/SFX Utama
            if (source == musicSource || source == SFXSource) continue;

            // Cek: Apakah clip yang sedang diputar source ini...
            // ...ada di dalam daftar 'clips' yang kita kasih?
            if (IsInArray(source.clip, clips))
            {
                // Kalau ketemu, matikan dengan Fade Out!
                StartCoroutine(FadeOutAndDestroy(source, fadeDuration));

            }
        }
    }

    public void PlayLoopingSFX(AudioClip clip, float targetVolume = 1f, float fadeDuration = 1f)
    {
        if (clip == null) return;

        // 1. Cek apakah suara ini sudah bunyi?
        if (IsSoundPlaying(clip)) return;


        // 2. Buat AudioSource baru
        AudioSource newSource = gameObject.AddComponent<AudioSource>();

        // 3. Setting suaranya
        newSource.clip = clip;
        newSource.loop = true;
        newSource.spatialBlend = 0f;

        // --- LOGIKA FADE IN ---
        newSource.volume = 0f; // Mulai dari bisu (0)
        newSource.Play();

        // Jalankan Coroutine untuk menaikkan volume pelan-pelan
        StartCoroutine(FadeInSequence(newSource, targetVolume, fadeDuration));
    }

    public void StopLoopingSFX(AudioClip clip, float fadeDuration = 1f)
    {
        if (clip == null) return;

        AudioSource[] allSources = GetComponents<AudioSource>();

        foreach (var source in allSources)
        {
            if (source == musicSource || source == SFXSource) continue;

            if (source.clip == clip)
            {
                // --- LOGIKA FADE OUT ---
                // Jangan langsung Destroy. Kita fade out dulu, baru destroy di dalam Coroutine.
                StartCoroutine(FadeOutAndDestroy(source, fadeDuration));

            }
        }
    }
    IEnumerator FadeInSequence(AudioSource source, float targetVol, float duration)
    {
        float currentTime = 0f;
        float startVol = 0f;

        while (currentTime < duration)
        {
            if (source == null) yield break; // Safety check jika object hancur

            currentTime += Time.deltaTime;
            // Rumus Lerp: Mengubah angka pelan-pelan dari 0 ke Target
            source.volume = Mathf.Lerp(startVol, targetVol, currentTime / duration);
            yield return null; // Tunggu frame berikutnya
        }

        // Pastikan volume pas di target di akhir
        if (source != null) source.volume = targetVol;
    }

    IEnumerator FadeOutAndDestroy(AudioSource source, float duration)
    {
        float currentTime = 0f;
        float startVol = source.volume; // Mulai dari volume saat ini (bukan selalu 1)

        while (currentTime < duration)
        {
            if (source == null) yield break;

            currentTime += Time.deltaTime;
            // Rumus Lerp: Mengubah angka pelan-pelan dari Current ke 0
            source.volume = Mathf.Lerp(startVol, 0f, currentTime / duration);
            yield return null;
        }


        source.Stop();
        Destroy(source); // Hancurkan komponen setelah suara benar-benar hilang

    }
    // Fungsi Helper (Cuma buat ngecek)
    private bool IsSoundPlaying(AudioClip clip)
    {
        AudioSource[] allSources = GetComponents<AudioSource>();
        foreach (var source in allSources)
        {
            if (source == musicSource || source == SFXSource) continue;
            // Cek isPlaying juga biar kalau lagi proses fade out (dan belum mati), gak dianggap play baru
            if (source.clip == clip) return true;
        }
        return false;
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

    private bool IsInArray(AudioClip clip, AudioClip[] array)
    {
        if (clip == null) return false;
        foreach (var item in array)
        {
            if (item == clip) return true;
        }
        return false;
    }

    private bool IsAnyClipPlayingFromList(AudioClip[] clips)
    {
        AudioSource[] allSources = GetComponents<AudioSource>();
        foreach (var source in allSources)
        {
            if (source == musicSource || source == SFXSource) continue;
            if (IsInArray(source.clip, clips)) return true; // Ada yang lagi bunyi nih
        }
        return false;
    }

    // --- FUNGSI RANDOMISASI UNTUK VARIANT ---


    // Fungsi Reusable (Hanya bisa diakses di dalam script ini)
    public void PlayRandomFromList(AudioClip[] clips)
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