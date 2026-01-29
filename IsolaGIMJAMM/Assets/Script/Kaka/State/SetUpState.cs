using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SetUpState : GameBaseState
{
    public AudioManager am;



    public Transform playertransform;
    public ItemLookDownSwitcher pm;

    public PhotoMechanic cam;

    [Header("Book Visuals")]
    public GameObject bookOpenObj;   // Drag Objek 'Book_Visual_OPEN' kesini
    public GameObject bookClosedObj;

    public NPCBodyPart ekspresiMarahSO;


    [Header("Databases")]
    public NPCDatabase npcDatabase; // <--- WAJIB ADA
    [Header("Door System")]
    public DoorController doorController; // Drag script LiftDoorController kesini
    float targetSize = 0.1889712f;
    float flipDuration = 1f;


    public Image rambutt, kepalaa, bajuu, celanaa, sepatuu;

    [Header("NPC References")]
    public GameObject npcPrefab;     // Prefab NPC
    public Transform spawnContainer; // WADAH KOSONG khusus untuk anak-anak NPC (JANGAN TARUH LAINNYA DISINI)
    public Transform[] spawnPoints;  // Titik Kursi di dalam lift
    public Transform doorStartPoint; // Titik Antrean di luar pintu
    public Transform lurusan;

    [Header("Floor System (Timer Pengganti)")]
    public int minFloor = 1;      // Lantai paling bawah
    public int maxFloor = 20;     // Lantai paling atas
    public float secondsPerFloor = 1.5f; // Kecepatan ganti angka lantai

    // Variable Tracking Lantai
    private int startFloor = 1;   // Lantai keberangkatan
    private int targetFloor;      // Lantai tujuan
    private float currentFloorFloat; // Untuk hitungan halus di background
    private int currentDisplayFloor; // Angka bulat yang tampil di UI

    [Header("Game Settings")]
    public int numberOfOptions;      // Akan di-overwrite oleh Random
    [Range(0f, 1f)] public float distractionSimilarity = 0.5f;

    [Header("Time Settings")]
    public float timePerNPC = 5f;
    private float currentTimer;

    [Header("UI References")]
    public TextMeshProUGUI missionText, timerUI;
    public GameObject UI; // Panel Misi Game

    public DebugGameManager gm; // Referensi GameManager untuk Nyawa
    public TextMeshProUGUI floorUI;

    // --- LOGIC FLAGS ---
    public bool targetdone = false;      // Ditandai TRUE jika pemain memilih BENAR/SALAH (Logika klik diatur PlayerCamera)
    private bool isSequenceActive = false; // Pengunci agar Update tidak jalan saat animasi pintu/jalan
    private bool hasTriggeredStop = false;

    private GameStateManager cachedGameState;

    // List untuk melacak NPC yang hidup agar bisa disuruh keluar nanti
    private List<GameObject> currentNPCs = new List<GameObject>();




    #region EnterState

    // ---------------------------------------------------------
    // 1. FASE INISIALISASI (ENTER STATE)
    // ---------------------------------------------------------
    public override void EnterState(GameStateManager gamestate)
    {
        Debug.Log("--- MASUK SETUP: MEMULAI SEQUENCE ---");
        cachedGameState = gamestate; // Simpan referensi
                                     // [FIX 1] Cek 'cam' langsung, bukan 'pm'
        hasTriggeredStop = false; // Reset flag stop biar bisa foto lagi ronde depan
        if (cam != null)
        {
            Debug.Log($"[LISTENER] Saya SetUpState. Saya akan subscribe ke PhotoMechanic dengan ID: {cam.GetInstanceID()}");
            PhotoMechanic.OnCekrek += HandleCekrek;
        }
        // Mulai rangkaian animasi masuk
        gamestate.StartCoroutine(EntrySequence(gamestate));

    }
    #endregion

    #region EntySequence

    // ---------------------------------------------------------
    // 2. SEQUENCE MASUK (SPAWN -> BUKA PINTU -> JALAN MASUK -> TUTUP)
    // ---------------------------------------------------------
    IEnumerator EntrySequence(GameStateManager gamestate)
    {
        isSequenceActive = true; // Kunci Update Loop
        targetdone = false;

        // Matikan UI Misi dulu saat animasi
        if (UI != null) UI.SetActive(false);

        // A. BERSIH-BERSIH (Pastikan wadah kosong)
        currentNPCs.Clear();
        if (spawnContainer != null)
        {
            // Hapus sisa NPC jika ada (Loop terbalik biar aman)
            for (int i = spawnContainer.childCount - 1; i >= 0; i--)
            {
                UnityEngine.Object.Destroy(spawnContainer.GetChild(i).gameObject);
            }
        }

        numberOfOptions = UnityEngine.Random.Range(2, Mathf.Min(6, spawnPoints.Length + 1));
        float totalTimeNeeded = numberOfOptions * timePerNPC;

        currentTimer = totalTimeNeeded;

        // -----------------------------------------------------------
        // [PERBAIKAN LOGIKA] Jarak ditentukan Waktu, Bukan Random
        // -----------------------------------------------------------

        // Tetapkan kecepatan lift yang "Enak" dilihat (Constant)
        // Misal: 1 lantai butuh 1.5 detik (sesuai variabel di inspector)
        // Kita percepat sedikit (-0.2f) sebagai buffer agar lift sampai duluan sebelum timer habis
        float targetSpeed = secondsPerFloor;
        float safeTime = Mathf.Max(1f, totalTimeNeeded - 0.5f); // Safety buffer

        // Hitung berapa lantai yang BISA ditempuh dalam waktu tersebut?
        // Contoh: 11.5 detik / 1.5 detik = 7.6 lantai -> Dibulatkan jadi 8 lantai
        int floorDistance = Mathf.RoundToInt(safeTime / targetSpeed);

        // Pastikan jarak minimal 1 lantai (biar gak diam)
        if (floorDistance < 1) floorDistance = 1;

        // -----------------------------------------------------------
        // 3. Tentukan Arah & Target Lantai (Cek Mentok Atas/Bawah)
        // -----------------------------------------------------------

        // Cek apakah kalau naik bakal nabrak atap (MaxFloor)?
        bool canGoUp = (startFloor + floorDistance) <= maxFloor;
        // Cek apakah kalau turun bakal nabrak tanah (MinFloor)?
        bool canGoDown = (startFloor - floorDistance) >= minFloor;

        if (canGoUp && canGoDown)
        {
            // Kalau bisa dua arah, pilih random (Naik atau Turun)
            targetFloor = (UnityEngine.Random.value > 0.5f) ? (startFloor + floorDistance) : (startFloor - floorDistance);
        }
        else if (canGoUp)
        {
            // Kalau cuma bisa naik (misal dari lantai 1)
            targetFloor = startFloor + floorDistance;
        }
        else
        {
            // Kalau cuma bisa turun (misal dari lantai paling atas)
            targetFloor = startFloor - floorDistance;
        }

        // -----------------------------------------------------------
        // 4. Hitung Ulang Speed Presisi (Finishing Touch)
        // -----------------------------------------------------------
        // Karena pembulatan RoundToInt tadi, waktu tempuhnya mungkin geser sedikit.
        // Kita kunci lagi speednya supaya PAS BANGET dengan safety time.

        float finalDistance = Mathf.Abs(targetFloor - startFloor);
        secondsPerFloor = safeTime / finalDistance;

        Debug.Log($"LOGIC: NPC {numberOfOptions} | Waktu {totalTimeNeeded}s | Jarak {finalDistance} Lt | Target Lantai {targetFloor}");

        // Setup Visual
        currentFloorFloat = startFloor;
        currentDisplayFloor = startFloor;
        UpdateFloorUI(false);

        // currentFloorFloat = startFloor;
        // currentDisplayFloor = startFloor;

        // // Tampilkan UI Awal: "5 -> 12"
        // //  UpdateFloorUI(false);

        // // --- ATAU ---
        // // Jika kamu ingin waktunya tetap bergantung jumlah NPC seperti request sebelumnya:
        // numberOfOptions = UnityEngine.Random.Range(2, Mathf.Min(6, spawnPoints.Length + 1));

        // // Kita balik logikanya: Waktu ditentukan NPC, Lantai menyesuaikan Waktu
        // float totalTimeNeeded = numberOfOptions * timePerNPC; // Misal 3 detik per NPC
        // currentTimer = totalTimeNeeded;

        // do
        // {
        //     targetFloor = UnityEngine.Random.Range(minFloor, maxFloor + 1);
        // } while (targetFloor == startFloor);

        // float floorDistance = Mathf.Abs(targetFloor - startFloor);
        // secondsPerFloor = totalTimeNeeded / floorDistance;

        // currentFloorFloat = startFloor;
        // currentDisplayFloor = startFloor;
        // UpdateFloorUI(false);
        // Kita cari lantai tujuan yang jaraknya pas dengan waktu tersebut
        // Arah random (Naik atau Turun)
        // int direction = (UnityEngine.Random.value > 0.5f) ? 1 : -1;

        // // Hitung berapa lantai yang harus dilewati
        // int floorDistance = Mathf.CeilToInt(totalTimeNeeded / secondsPerFloor);

        // targetFloor = startFloor + (floorDistance * direction);

        // // Cek batasan (Clamping) agar tidak minus atau kelebihan
        // if (targetFloor > maxFloor)
        // {
        //     targetFloor = startFloor - floorDistance; // Kalau mentok atas, paksa turun
        // }
        // else if (targetFloor < minFloor)
        // {
        //     targetFloor = startFloor + floorDistance; // Kalau mentok bawah, paksa naik
        // }

        // // Update currentDisplay lagi incase logic berubah
        // UpdateFloorUI(false);


        // numberOfOptions = UnityEngine.Random.Range(2, Mathf.Min(6, spawnPoints.Length + 1));
        // currentTimer = numberOfOptions * timePerNPC;

        // 1. Generate Visual Target (Model Rambut/Baju/dll)
        SmartClueManager.Instance.GenerateTarget();
        NPCStyleData targetData = SmartClueManager.Instance.currentTarget;

        // 2. Generate Logic Rahasia (Warna & Bagian Tubuh) SEKARANG JUGA
        if (GameColorManager.Instance != null)
        {
            // A. Pilih Warna yang belum unlock
            int winningColor = GameColorManager.Instance.GetRandomLockedColorIndex();

            // Cek Win Condition
            if (winningColor == -1)
            {
                Debug.Log("SEMUA WARNA SUDAH TERKUMPUL!");
                winningColor = UnityEngine.Random.Range(0, 10); // Fallback
            }

            // B. Pilih Bagian Tubuh (0-3)
            int secretBodyPart = UnityEngine.Random.Range(1, 4);

            // C. SIMPAN KE MANAGER SEBELUM UI DIPANGGIL
            GameColorManager.Instance.currentRoundTargetColorIndex = winningColor;
            GameColorManager.Instance.currentRoundBodyPartIndex = secretBodyPart;

            Debug.Log($"LOGIC ESTABLISHED: Cari Index {winningColor} di BodyPart {secretBodyPart}");
        }

        // 3. Update UI (Sekarang UI sudah bisa baca data dari Manager dengan benar)
        UpdateMissionText(targetData);


        // 4. Random Kursi
        List<int> seatIndexes = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++) seatIndexes.Add(i);
        Shuffle(seatIndexes);

        List<bool> isTargetList = new List<bool>();
        isTargetList.Add(true);
        for (int i = 0; i < numberOfOptions - 1; i++) isTargetList.Add(false);
        Shuffle(isTargetList);


        // D. SPAWN & JALAN MASUK (LeanTween)
        float moveDuration = 3f;
        float delayPerNPC = 0.1f;

        // Loop Spawn NPC
        for (int i = 0; i < numberOfOptions; i++)
        {
            // A. Tentukan Posisi
            Vector3 queuePos = lurusan.position - (lurusan.forward * (i * 1.0f));
            Transform targetSeat = spawnPoints[seatIndexes[i]];

            // B. Spawn & Reset Rotasi
            GameObject newNPC = UnityEngine.Object.Instantiate(npcPrefab, queuePos, Quaternion.identity, spawnContainer);
            newNPC.transform.rotation = Quaternion.Euler(0, 0, 0); // Hadap depan (ke kamera)

            currentNPCs.Add(newNPC);
            NPCVisualControl visualCtrl = newNPC.GetComponent<NPCVisualControl>();

            // -----------------------------------------------------------------------
            // [FIX UTAMA] LOGIKA DATA: Membedakan Target vs Random (Distraction)
            // -----------------------------------------------------------------------
            NPCInstanceData finalData;

            if (isTargetList[i])
            {
                // JIKA TARGET: Convert Clue menjadi Data Warna
                // Fungsi ini memastikan baju/rambut sesuai Clue, tapi warnanya diatur sistem
                finalData = ConvertStyleToInstance(targetData);
                finalData = ConvertStyleToInstance(targetData);

                // Setup Identity (Agar game tahu ini target yang benar saat diklik)
                // Pastikan fungsi helper 'SetupNPCIdentityOnly' ada di bawah script ini
                SetupNPCIdentityOnly(newNPC, true, targetData);
            }
            else
            {
                // JIKA BUKAN TARGET: Generate Random Total
                finalData = GenerateRandomNPC();

                // Setup Identity (Agar game tahu ini SALAH jika diklik)
                SetupNPCIdentityOnly(newNPC, false, null);
            }

            // Terapkan Visual ke NPC (Ini yang membuat NPC jadi Hitam Putih / Berwarna)
            if (visualCtrl != null) visualCtrl.SetupVisuals(finalData);


            // -----------------------------------------------------------------------
            // [FIX ANIMASI] LOGIKA JALAN & FLIP
            // -----------------------------------------------------------------------
            float myStartDelay = i * delayPerNPC;

            // 1. Jalan ke Pintu
            LeanTween.move(newNPC, doorStartPoint.position, 3f)
                .setDelay(myStartDelay)
                .setEase(LeanTweenType.easeOutSine)
                .setOnComplete(() =>
                {
                    if (newNPC != null)
                    {
                        // [FIX] AnimateFlip harus pakai parameter 'true' (Wajah Depan)
                        // Ini dipanggil saat sampai pintu sebelum jalan ke kursi
                        if (visualCtrl != null) visualCtrl.AnimateFlip();

                        // 2. Jalan ke Kursi
                        LeanTween.move(newNPC, targetSeat.position, 3f)
                            .setEase(LeanTweenType.easeInOutQuad)
                            .setOnComplete(() =>
                            {
                                if (newNPC != null)
                                {
                                    // Pastikan posisi pas di kursi
                                    newNPC.transform.position = targetSeat.position;
                                    newNPC.transform.rotation = targetSeat.rotation;

                                    if (visualCtrl != null)
                                    {
                                        visualCtrl.SetDirection(false);
                                        // [FIX] AnimateFlip pakai parameter 'false' (Punggung)
                                        visualCtrl.AnimateFlip();
                                        UpdateBuku(true);


                                    }
                                }
                            });
                    }
                });
        }
        // C. BUKA PINTU
        doorController.OpenDoors();
        am.PlayRandomLoopingSFX(am.AmbienceSfx);
        yield return new WaitForSeconds(1.0f); // Tunggu pintu terbuka
        // D. SPAWN & JALAN MASUK (PERBAIKAN VISUAL)
        float walkToDoorTime = 2.0f; // Waktu jalan dari lorong ke pintu
        float walkToSeatTime = 3f; // Waktu dari pintu ke kursi

        float totalWaitTime = ((numberOfOptions - 1) * delayPerNPC) + walkToDoorTime + walkToSeatTime;

        yield return new WaitForSeconds(totalWaitTime + 0.5f); // Tambah buffer 0.5 detik

        // E. TUTUP PINTU
        if (doorController != null) doorController.CloseDoors();
        am.PlaySFX(am.tingtung);
        am.StopRandomLoopingSFX(am.AmbienceSfx);
        am.PlayRandomLoopingSFX(am.npc);
        am.PlayLoopingSFX(am.Ambiance4);
        cam.StartConstantShake(0.05f);
        pm.UnlockCameraFeature();
        yield return new WaitForSeconds(1.0f);

        // F. MULAI GAMEPLAY


        Debug.Log($"LIFT BERGERAK: {startFloor} menuju {targetFloor}");
        if (UI != null) UI.SetActive(true);
        isSequenceActive = false;
    }
    #endregion

    #region gameloopUpdateState


    // ---------------------------------------------------------
    // 3. GAME LOOP (TIMER & CHECKING)
    // ---------------------------------------------------------
    public override void UpdateState(GameStateManager gamestate)
    {
        // Kalau sequence animasi sedang jalan, jangan lakukan apa-apa
        if (isSequenceActive) return;

        // --- LOGIKA TIMER ---
        if (currentTimer > 0 && !targetdone)
        {
            currentTimer -= Time.deltaTime;
            if (timerUI != null) timerUI.text = currentTimer.ToString("F0");

            if (currentTimer <= 0)
            {
                currentTimer = 0;
                UpdateBuku(false);
                Debug.Log("WAKTU HABIS!");
                // Trigger Sequence Keluar (Time Up / Salah)
                gamestate.StartCoroutine(ExitSequence(gamestate));
            }
        }



        if (!targetdone)
        {
            // Gerakkan float mendekati targetFloor
            // Mathf.MoveTowards mirip timer tapi berdasarkan target angka
            // Speed = 1 / secondsPerFloor (artinya 1 lantai per X detik)
            float speed = 1.0f / secondsPerFloor;

            currentFloorFloat = Mathf.MoveTowards(currentFloorFloat, targetFloor, speed * Time.deltaTime);

            // Update UI jika angka bulat berubah
            int newDisplayFloor = Mathf.RoundToInt(currentFloorFloat);
            if (newDisplayFloor != currentDisplayFloor)
            {
                currentDisplayFloor = newDisplayFloor;
                UpdateFloorUI(false); // Update teks "5 -> 12"
            }

            // Cek Apakah Sampai Tujuan (Waktu Habis)
            // Mathf.Approximately untuk membandingkan float dengan aman
            if (Mathf.Abs(currentFloorFloat - targetFloor) < 0.01f)
            {
                Debug.Log("SAMPAI TUJUAN (WAKTU HABIS)!");
                targetdone = true; // Trigger Sequence Keluar

                // Kurangi nyawa karena gagal menemukan target sebelum sampai
                if (gm != null) gm.nyawa--;
            }
        }
        else
        {
            // Jika targetdone = true (Entah karena Spasi, Waktu Habis, atau Ketemu Target)
            // Langsung masuk sequence keluar
            gamestate.StartCoroutine(ExitSequence(gamestate));
            //  pm.SetInputActive(false);

            // Reset flag biar gak dipanggil berkali-kali
            targetdone = false;
        }
    }
    #endregion

    #region ExitSquence

    // ---------------------------------------------------------
    // 4. SEQUENCE KELUAR (BUKA PINTU -> JALAN KELUAR -> TUTUP -> DESTROY)
    // ---------------------------------------------------------
    IEnumerator ExitSequence(GameStateManager gamestate)
    {
        pm.LockCameraFeature();
        am.StopRandomLoopingSFX(am.npc);
        isSequenceActive = true; // Kunci lagi
        if (UI != null) UI.SetActive(false); // Sembunyikan UI Misi
        UpdateFloorUI(true);
        startFloor = currentDisplayFloor;
        Debug.Log($"Lift Berhenti di Lantai {startFloor}");
        // A. BUKA PINTU
        am.PlaySFX(am.tingtung);
        am.PlayRandomLoopingSFX(am.AmbienceSfx);
        if (doorController != null) doorController.OpenDoors();
        cam.StopShake();
        yield return new WaitForSeconds(1.0f);
        // B. NPC JALAN KELUAR
        float moveOutDuration = 1.9f;
        foreach (GameObject npc in currentNPCs)
        {
            if (npc != null)
            {
                LeanTween.scaleX(npc, 0f, 1f).setEase(LeanTweenType.easeOutElastic);
                LeanTween.scaleX(npc, 0.1889712f, 1f).setEase(LeanTweenType.easeOutElastic);

                Vector3 lookSpot = doorStartPoint.position;

                // 2. Samakan tinggi (Y) target dengan tinggi NPC saat ini
                // Ini triknya: Kita paksa NPC melihat "lurus" secara horizontal
                lookSpot.y = npc.transform.position.y;

                // 3. Perintahkan LookAt ke posisi yang sudah "didatarkan" tadi
                npc.transform.LookAt(lookSpot);

                // Jalan ke pintu
                LeanTween.move(npc, doorStartPoint.position, moveOutDuration).setEase(LeanTweenType.easeInQuad).setOnComplete(lurus);

            }
            void lurus()
            {


                LeanTween.scaleX(npc, 0f, 1f).setEase(LeanTweenType.easeOutElastic);
                LeanTween.scaleX(npc, 0.1889712f, 1f).setEase(LeanTweenType.easeOutElastic);
                npc.transform.rotation = Quaternion.identity;
                LeanTween.move(npc, lurusan.position, 3);
            }
        }


        // Tunggu NPC sampai di luar
        yield return new WaitForSeconds(moveOutDuration + 0.2f);

        // C. TUTUP PINTU (Sesuai request: NPC masih ada di luar, belum didestroy)
        if (doorController != null) doorController.CloseDoors();
        am.StopRandomLoopingSFX(am.AmbienceSfx);
        am.StopLoopingSFX(am.Ambiance4);
        yield return new WaitForSeconds(3.0f); // Tunggu tertutup rapat


        // D. HAPUS NPC (Sekarang aman karena pintu tertutup)
        foreach (GameObject npc in currentNPCs)
        {
            if (npc != null) UnityEngine.Object.Destroy(npc);
        }
        currentNPCs.Clear();

        // E. CEK NYAWA & PINDAH STATE
        // Logic: Kita cek apakah game over atau lanjut
        // Catatan: Pengurangan nyawa sebaiknya dilakukan saat KLIK SALAH di PlayerCamera, 
        // tapi jika mau dikurangi saat waktu habis, tambahkan disini:

        // if (waktuHabis) gm.nyawa--; (Opsional logika tambahan)

        if (gm != null && gm.nyawa <= 0)
        {
            gamestate.SwitchState(gamestate.gameoverstate);
        }
        else
        {
            gamestate.SwitchState(gamestate.waitingstate);
        }
    }
    #endregion

    #region Function

    // ---------------------------------------------------------
    // HELPER FUNCTIONS
    // ---------------------------------------------------------

    // Helper untuk mengatur identitas NPC (Nama & Komponen Identity) tanpa mengganggu Visual
    void SetupNPCIdentityOnly(GameObject npcObj, bool isTarget, NPCStyleData data)
    {
        NPCIdentity identity = npcObj.GetComponent<NPCIdentity>();
        if (identity != null)
        {
            // Sesuaikan dengan script NPCIdentity kamu. 
            // Jika script NPCIdentity kamu masih pakai 'SetupIdentity(NPCStyleData, bool)', 
            // pastikan dia tidak mencoba mengganti visual lagi, hanya simpan data bool isTarget-nya.
            identity.SetupIdentity(data, isTarget);
        }
        npcObj.name = isTarget ? "NPC_TARGET" : "NPC_Distraction";
    }


    void UpdateMissionText(NPCStyleData data)
    {
        if (data == null) return;


        // Ambil index bagian tubuh yang dicari dari Manager
        int targetPart = GameColorManager.Instance.currentRoundBodyPartIndex;

        if (data.celana != null)
            Debug.Log("TARGET CELANA ADALAH: " + data.celana.name + " | Sprite: " + data.celana.visual.name);
        // -----------------
        // 0=Rambut, 1=Baju, 2=Celana, 3=Sepatu

        // Helper lokal untuk mewarnai UI
        void SetClueVisual(Image img, Sprite sprite, bool isTargetPart)
        {
            if (img != null && sprite != null)
            {
                img.sprite = sprite;
                img.gameObject.SetActive(true);
                // Jika ini target -> HITAM, Jika bukan -> PUTIH
                img.color = isTargetPart ? Color.gray : Color.white;
            }
            else if (img != null) img.gameObject.SetActive(false);
        }

        SetClueVisual(rambutt, data.rambut?.visual, false); // Cek Rambut
        SetClueVisual(bajuu, data.baju?.visual, targetPart == 1); // Cek Baju
        SetClueVisual(celanaa, data.celana?.visual, targetPart == 2); // Cek Celana
        SetClueVisual(sepatuu, data.sepatu?.visual, targetPart == 3); // Cek Sepatu
                                                                      // Kepala selalu putih (bukan clue)
    }

    void Shuffle<T>(List<T> list)
    {
        for (int i = 0; i < list.Count; i++)
        {
            T temp = list[i];
            int randomIndex = UnityEngine.Random.Range(i, list.Count);
            list[i] = list[randomIndex];
            list[randomIndex] = temp;
        }
    }

    void UpdateFloorUI(bool isStopped)
    {
        if (floorUI != null)
        {
            if (isStopped)
            {
                // Jika berhenti, cuma tampilkan lantai saat ini
                floorUI.text = $"{currentDisplayFloor}";
            }
            else
            {
                // Jika jalan, tampilkan "Asal -> Tujuan"
                // Menggunakan panah atas/bawah biar keren
                string arrow = (targetFloor > startFloor) ? "▲" : "▼";
                floorUI.text = $"{currentDisplayFloor} {arrow} {targetFloor}";
            }
        }
    }

    void UpdateBuku(bool asli)
    {

        if (asli)
        {
            bookOpenObj.SetActive(true);
            bookClosedObj.SetActive(false);
        }
        else
        {
            bookOpenObj.SetActive(false);
            bookClosedObj.SetActive(true);
        }

    }
    NPCInstanceData ConvertStyleToInstance(NPCStyleData style)
    {
        NPCInstanceData newData = new NPCInstanceData();

        // 1. Ambil Data yang SUDAH DITENTUKAN di EntrySequence tadi
        int winningColorIndex = 0;

        int clueBodyPart = UnityEngine.Random.Range(1, 4);

        if (GameColorManager.Instance != null)
        {
            winningColorIndex = GameColorManager.Instance.currentRoundTargetColorIndex;
            clueBodyPart = GameColorManager.Instance.currentRoundBodyPartIndex;
        }

        // 2. Copy Model Fisik
        newData.ekspresiModel = style.ekspresi;
        newData.rambutModel = style.rambut;
        newData.bajuModel = style.baju;
        newData.celanaModel = style.celana;
        newData.sepatuModel = style.sepatu;


        // 3. Isi warna random dulu untuk semua (selain winning color)
        newData.rambutColorIndex = GetRandomDifferentColor(winningColorIndex);
        newData.bajuColorIndex = GetRandomDifferentColor(winningColorIndex);
        newData.celanaColorIndex = GetRandomDifferentColor(winningColorIndex);
        newData.sepatuColorIndex = GetRandomDifferentColor(winningColorIndex);


        // 4. Timpa bagian target dengan Warna Pemenang
        switch (clueBodyPart)
        {
            case 0: newData.rambutColorIndex = winningColorIndex; break;
            case 1: newData.bajuColorIndex = winningColorIndex; break;
            case 2: newData.celanaColorIndex = winningColorIndex; break;
            case 3: newData.sepatuColorIndex = winningColorIndex; break;
        }

        return newData;
    }

    int GetRandomDifferentColor(int forbiddenIndex)
    {
        int randomColor;
        do { randomColor = UnityEngine.Random.Range(0, 10); }
        while (randomColor == forbiddenIndex);
        return randomColor;
    }

    NPCInstanceData GenerateRandomNPC()
    {
        NPCInstanceData newData = new NPCInstanceData();

        // Pastikan variabel 'npcDatabase' sudah di-drag di Inspector
        if (npcDatabase != null)
        {
            // Ambil model acak dari database
            newData.rambutModel = npcDatabase.GetRandomHair();
            newData.ekspresiModel = npcDatabase.GetRandomEkspresi();
            newData.bajuModel = npcDatabase.GetRandomShirt();
            newData.celanaModel = npcDatabase.GetRandomPants();
            newData.sepatuModel = npcDatabase.GetRandomShoes();

        }
        else
        {
            Debug.LogError("DATABASE KOSONG! Drag NPCDatabase ke slot script SetUpState di Inspector.");
        }

        // Tentukan Warna Random (0-9) untuk setiap bagian
        newData.rambutColorIndex = UnityEngine.Random.Range(0, 10);
        newData.bajuColorIndex = UnityEngine.Random.Range(0, 10);
        newData.celanaColorIndex = UnityEngine.Random.Range(0, 10);
        newData.sepatuColorIndex = UnityEngine.Random.Range(0, 10);

        return newData;
    }

    public void OnPhotoDone(GameStateManager gamestate)
    {
        if (hasTriggeredStop) return; // Cegah double call
        hasTriggeredStop = true;

        Debug.Log("SetupState: Menerima laporan foto selesai. Menjalankan Sequence...");

        // Jalankan logika Emergency Stop / Pindah State
        gamestate.StartCoroutine(EmergencyStopSequence(gamestate));
    }

    private void HandleCekrek()
    {
        Debug.Log("SetUpState: Menerima sinyal Cekrek!");

        OnPhotoDone(cachedGameState);

    }
    #endregion

    #region StopSequence

    // --- SEQUENCE BARU: SAAT SPASI DITEKAN ---
    IEnumerator EmergencyStopSequence(GameStateManager gamestate)
    {
        // 1. Matikan UI & Logic Update
        pm.LockCameraFeature();


        // 1. Matikan UI & Logic Update
        isSequenceActive = true;
        if (UI != null) UI.SetActive(false);
        UpdateFloorUI(true); // Tampilkan status "Lift Berhenti"

        Debug.Log("NPC MARAH: Menoleh ke Pemain...");

        // 2. PHASE 1: MELIHAT KE PEMAIN (Wajah)
        foreach (GameObject npc in currentNPCs)
        {
            if (npc != null)
            {

                NPCVisualControl visual = npc.GetComponent<NPCVisualControl>();
                visual.AnimateFlip();
                if (visual != null)
                {


                    visual.SetDirection(true); // Ganti sprite ke WAJAH (Depan)
                    visual.ChangeToSpecificExpression(ekspresiMarahSO);


                }
                npc.transform.LookAt(playertransform); // Pastikan tegak
            }
        }

        // Tahan tatapan selama 2 detik
        yield return new WaitForSeconds(2.0f);

        // 3. PHASE 2: MELIHAT KE DEPAN LAGI (Punggung)
        foreach (GameObject npc in currentNPCs)
        {
            if (npc != null)
            {
                NPCVisualControl visual = npc.GetComponent<NPCVisualControl>();
                visual.AnimateFlip();


                // TAHAP 2: Saat scale 0 (Hilang), Ganti Gambar
                if (visual != null) visual.SetDirection(false);




            }
        }

        // Jeda sedikit sebelum pintu terbuka (biar tidak kaget)
        yield return new WaitForSeconds(1.0f);

        // 4. LANJUT KE SEQUENCE KELUAR
        // Panggil ExitSequence untuk membuka pintu dan jalan keluar
        yield return gamestate.StartCoroutine(ExitSequence(gamestate));

    }

    // FUNGSI HELPER: Ubah Clue jadi Data Warna
    #endregion

    public override void OnEnterState(GameStateManager gamestate) { }

    public override void ExitState(GameStateManager gamestate)
    {

        // [FIX 2] Unsubscribe dari 'cam', cek 'cam' bukan 'pm'
        if (cam != null)
        {
            PhotoMechanic.OnCekrek -= HandleCekrek;
        }

        cachedGameState = null;
    }
}