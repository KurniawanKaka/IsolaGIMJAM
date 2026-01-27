using UnityEngine;

public class NPCVisualControl : MonoBehaviour
{
    [Header("Sprite Renderers")]
    public SpriteRenderer rambutRenderer;
    public SpriteRenderer ekspresiRenderer;
    public SpriteRenderer bajuRenderer;
    public SpriteRenderer celanaRenderer;
    public SpriteRenderer sepatuRenderer;

    // Data probadi NPC ini
    public NPCInstanceData myData;

    // Fungsi ini dipanggil saat Spawn nanti
    public void SetupVisuals(NPCInstanceData data)
    {
        myData = data;

        if (rambutRenderer != null)
        {
            if (data.rambutModel != null)
                rambutRenderer.sprite = data.rambutModel.visual;
            else
                Debug.LogWarning($"NPC {gameObject.name}: Data Rambut Model KOSONG!");
        }
        else Debug.LogError($"NPC {gameObject.name}: Slot 'rambutRenderer' di Inspector BELUM DIISI!");

        // Cek Baju
        if (bajuRenderer != null)
        {
            if (data.bajuModel != null)
                bajuRenderer.sprite = data.bajuModel.visual;
            else
                Debug.LogWarning($"NPC {gameObject.name}: Data Baju Model KOSONG!");
        }
        else Debug.LogError($"NPC {gameObject.name}: Slot 'bajuRenderer' di Inspector BELUM DIISI!");

        // Cek Celana
        if (celanaRenderer != null)
        {
            if (data.celanaModel != null)
                celanaRenderer.sprite = data.celanaModel.visual;
            else
                Debug.LogWarning($"NPC {gameObject.name}: Data Celana Model KOSONG!");
        }
        else Debug.LogError($"NPC {gameObject.name}: Slot 'celanaRenderer' di Inspector BELUM DIISI!");

        // Cek Sepatu
        if (sepatuRenderer != null)
        {
            if (data.sepatuModel != null)
                sepatuRenderer.sprite = data.sepatuModel.visual;
            else
                Debug.LogWarning($"NPC {gameObject.name}: Data Sepatu Model KOSONG!");
        }
        else Debug.LogError($"NPC {gameObject.name}: Slot 'sepatuRenderer' di Inspector BELUM DIISI!");

        // Cek Kepala
        if (ekspresiRenderer != null)
        {
            if (data.ekspresiModel != null)
                ekspresiRenderer.sprite = data.ekspresiModel.visual;
            else
                Debug.LogWarning($"NPC {gameObject.name}: Data Ekspresi KOSONG!");
        }
        else Debug.LogError($"NPC {gameObject.name}: Slot 'Ekspresi' di Inspector BELUM DIISI!");

        // 1. Pasang Gambar (Model Putih)
        if (myData.rambutModel) rambutRenderer.sprite = myData.rambutModel.visual;
        if (myData.ekspresiModel) ekspresiRenderer.sprite = myData.ekspresiModel.visual;
        if (myData.bajuModel) bajuRenderer.sprite = myData.bajuModel.visual;
        if (myData.celanaModel) celanaRenderer.sprite = myData.celanaModel.visual;
        if (myData.sepatuModel) sepatuRenderer.sprite = myData.sepatuModel.visual;

        // 2. Warnai!
        RefreshColor();
    }

    // Fungsi untuk minta update warna ke Manager
    public void RefreshColor()
    {
        if (myData == null || GameColorManager.Instance == null) return;

        // Ambil info: Bagian tubuh mana yang sedang jadi Clue/Misteri ronde ini?
        int mysteryPart = GameColorManager.Instance.currentRoundBodyPartIndex;
        // 0=Rambut, 1=Baju, 2=Celana, 3=Sepatu

        // --- HELPER LOKAL: TENTUKAN WARNA ---
        // Logika: 
        // 1. Jika bagian ini adalah MISTERI RONDE INI -> Paksa Abu-abu (Kecuali sudah diklik benar nanti)
        // 2. Jika bukan -> Tampilkan warna normal (Sesuai status unlock)
        Color DetermineColor(int myPartIndex, int myColorIndex)
        {
            // Jika ini adalah bagian tubuh yang sedang dicari (Clue)
            if (myPartIndex == mysteryPart)
            {
                // Kita cek: Apakah warna INI barusan di-unlock di ronde ini?
                // Kalau sudah unlocked, tampilkan. Kalau belum (walaupun unlocked di ronde sebelumnya),
                // Kita bisa pilih untuk tetap menyembunyikannya atau menampilkannya.

                // BERDASARKAN REQUEST KAMU: "Semua pakaian berwarna locked agar pemain tetap menganalisis"
                // Jadi, kita cek apakah warna spesifik NPC ini sudah 'ketahuan' atau belum.
                // Namun, agar susah, kita anggap visualnya "Locked" kecuali dia adalah target yang SUDAH ditemukan.

                if (GameColorManager.Instance.palette[myColorIndex].isUnlocked)
                {
                    // TAPI tunggu, kalau kita tampilkan yang unlocked, pemain tetap bisa menebak sisa 1.
                    // Jadi strateginya: 
                    // Tampilkan WARNA ASLI hanya jika Pemain SUDAH MENANG ronde ini / atau menggunakan Lensa.
                    // Tapi secara default, renderernya harus ABU-ABU.

                    // Kita gunakan status standar dulu:
                    return GameColorManager.Instance.GetColorStatus(myColorIndex);
                }
                else
                {
                    return GameColorManager.Instance.lockedColor; // Abu-abu
                }
            }
            else
            {
                // Bagian tubuh lain (bukan clue) tampil normal apa adanya
                return GameColorManager.Instance.GetColorStatus(myColorIndex);
            }
        }

        // --- TAPI... ---
        // Untuk memenuhi request "Semua Baju Abu-abu", kita harus memanipulasi logika GetColorStatus sedikit.
        // Kita buat logika manual di sini:

        void ApplyColor(SpriteRenderer renderer, int partIndex, int colorIndex)
        {
            if (renderer == null) return;

            // APAKAH INI BAGIAN MISTERI?
            if (partIndex == mysteryPart)
            {
                // Logika Hardcore: 
                // Walaupun warnanya sudah Unlocked dari level sebelumnya, 
                // untuk ronde ini kita paksa terlihat ABU-ABU agar pemain bingung.
                // KECUALI: Warna tersebut adalah Target Ronde Ini DAN sudah ditemukan (Game Menang).

                bool isTargetColor = (colorIndex == GameColorManager.Instance.currentRoundTargetColorIndex);
                bool isActuallyUnlocked = GameColorManager.Instance.palette[colorIndex].isUnlocked;

                // Jika ini warna target DAN sudah terbuka (artinya baru saja ditembak benar), Tampilkan.
                if (isTargetColor && isActuallyUnlocked)
                {
                    renderer.color = GameColorManager.Instance.palette[colorIndex].actualColor;
                }
                else
                {
                    // Sisanya (Distraction maupun Target yang belum ketemu) -> ABU-ABU
                    renderer.color = GameColorManager.Instance.lockedColor;
                }
            }
            else
            {
                // Bagian tubuh lain tampil normal (Bisa warna-warni kalau sudah unlock)
                renderer.color = GameColorManager.Instance.GetColorStatus(colorIndex);
            }
        }

        // Terapkan ke semua renderer

        ApplyColor(bajuRenderer, 1, myData.bajuColorIndex);
        ApplyColor(celanaRenderer, 2, myData.celanaColorIndex);
        ApplyColor(sepatuRenderer, 3, myData.sepatuColorIndex);
    }

    // Static Helper: Biar Manager bisa suruh semua NPC refresh barengan
    public static void RefreshAllNPCs()
    {
        NPCVisualControl[] allNPCs = FindObjectsOfType<NPCVisualControl>();
        foreach (var npc in allNPCs)
        {
            npc.RefreshColor();
        }
    }

    // Saya tambahkan parameter bool agar bisa dipakai buat hadap depan lagi nanti saat keluar
    public void SetDirection(bool isFront)
    {
        if (myData == null) return;

        if (isFront)
        {
            // TAMPILAN DEPAN (Visual)
            if (myData.rambutModel) rambutRenderer.sprite = myData.rambutModel.visual;
            if (myData.ekspresiModel) ekspresiRenderer.sprite = myData.ekspresiModel.visual; // Tambah kepala
            if (myData.bajuModel) bajuRenderer.sprite = myData.bajuModel.visual;
            if (myData.celanaModel) celanaRenderer.sprite = myData.celanaModel.visual;
            if (myData.sepatuModel) sepatuRenderer.sprite = myData.sepatuModel.visual;
        }
        else
        {
            // TAMPILAN BELAKANG (VisualBack)
            if (myData.rambutModel) rambutRenderer.sprite = myData.rambutModel.visualBack;
            if (myData.ekspresiModel) ekspresiRenderer.sprite = myData.ekspresiModel.visualBack; // Tambah kepala
            if (myData.bajuModel) bajuRenderer.sprite = myData.bajuModel.visualBack;
            if (myData.celanaModel) celanaRenderer.sprite = myData.celanaModel.visualBack;
            if (myData.sepatuModel) sepatuRenderer.sprite = myData.sepatuModel.visualBack;
        }
    }

    public void AnimateFlip()
    {
        // 1. Stop semua animasi lain (misal lagi bernafas) biar tidak tabrakan
        LeanTween.cancel(gameObject);


        // 2. Animasi Mengecil (Scale X -> 0)
        LeanTween.scaleX(gameObject, 0f, 0.08f)
            .setEase(LeanTweenType.easeInElastic)
            .setOnComplete(() =>
            {



                LeanTween.scaleX(gameObject, 0.1889712f, 0.5f)
                    .setEase(LeanTweenType.easeOutElastic);
                MulaiBernafas();
            });
    }

    void MulaiBernafas()
    {
        float randomDelay = Random.Range(0f, 0.5f);

        // Animasi Y (Memanjang ke atas)
        LeanTween.scaleY(gameObject, 0.2f, 1.5f) // Durasi 1.5 detik
            .setDelay(randomDelay)
            .setEase(LeanTweenType.easeInOutSine) // Gerakan halus
            .setLoopPingPong(); // Ulangi bolak-balik selamanya

        // Animasi X (Menipis sedikit - Opsional, hapus kalau aneh)
        // LeanTween.scaleX(gameObject, 0.2f, 1.5f)
        //     .setDelay(randomDelay)
        //     .setEase(LeanTweenType.easeInOutSine)
        //     .setLoopPingPong();
    }
    public void ChangeToSpecificExpression(NPCBodyPart targetSO)
    {
        ekspresiRenderer.sprite = targetSO.visual;
    }
}





[System.Serializable]
public class NPCInstanceData
{
    // Model Fisik (Sprite Putih)
    public NPCBodyPart ekspresiModel;
    public NPCBodyPart rambutModel;
    public NPCBodyPart bajuModel;
    public NPCBodyPart celanaModel;
    public NPCBodyPart sepatuModel;

    // Index Warna (0-9)
    public int rambutColorIndex;
    public int bajuColorIndex;
    public int celanaColorIndex;
    public int sepatuColorIndex;
}