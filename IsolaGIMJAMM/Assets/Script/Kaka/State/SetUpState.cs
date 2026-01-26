using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.UI;

public class SetUpState : GameBaseState
{
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
    public float timePerNPC = 3.0f;
    private float currentTimer;

    [Header("UI References")]
    public TextMeshProUGUI missionText, timerUI;
    public GameObject UI; // Panel Misi Game

    public DebugGameManager gm; // Referensi GameManager untuk Nyawa
    public TextMeshProUGUI floorUI;

    // --- LOGIC FLAGS ---
    public bool targetdone = false;      // Ditandai TRUE jika pemain memilih BENAR/SALAH (Logika klik diatur PlayerCamera)
    private bool isSequenceActive = false; // Pengunci agar Update tidak jalan saat animasi pintu/jalan

    // List untuk melacak NPC yang hidup agar bisa disuruh keluar nanti
    private List<GameObject> currentNPCs = new List<GameObject>();


    #region EnterState

    // ---------------------------------------------------------
    // 1. FASE INISIALISASI (ENTER STATE)
    // ---------------------------------------------------------
    public override void EnterState(GameStateManager gamestate)
    {
        Debug.Log("--- MASUK SETUP: MEMULAI SEQUENCE ---");

        // Mulai rangkaian animasi masuk sebagai Coroutine
        // Kita pinjam 'gamestate' (MonoBehaviour) untuk menjalankan Coroutine
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

        do
        {
            targetFloor = UnityEngine.Random.Range(minFloor, maxFloor + 1);
        } while (targetFloor == startFloor);

        currentFloorFloat = startFloor;
        currentDisplayFloor = startFloor;

        // Tampilkan UI Awal: "5 -> 12"
        UpdateFloorUI(false);

        // --- ATAU ---
        // Jika kamu ingin waktunya tetap bergantung jumlah NPC seperti request sebelumnya:
        numberOfOptions = UnityEngine.Random.Range(2, Mathf.Min(6, spawnPoints.Length + 1));

        // Kita balik logikanya: Waktu ditentukan NPC, Lantai menyesuaikan Waktu
        float totalTimeNeeded = numberOfOptions * 3.0f; // Misal 3 detik per NPC

        // Kita cari lantai tujuan yang jaraknya pas dengan waktu tersebut
        // Arah random (Naik atau Turun)
        int direction = (UnityEngine.Random.value > 0.5f) ? 1 : -1;

        // Hitung berapa lantai yang harus dilewati
        int floorDistance = Mathf.CeilToInt(totalTimeNeeded / secondsPerFloor);

        targetFloor = startFloor + (floorDistance * direction);

        // Cek batasan (Clamping) agar tidak minus atau kelebihan
        if (targetFloor > maxFloor)
        {
            targetFloor = startFloor - floorDistance; // Kalau mentok atas, paksa turun
        }
        else if (targetFloor < minFloor)
        {
            targetFloor = startFloor + floorDistance; // Kalau mentok bawah, paksa naik
        }

        // Update currentDisplay lagi incase logic berubah
        UpdateFloorUI(false);



        // B. SIAPKAN DATA (Logic Random)
        // Random jumlah NPC (2 sampai 6, tapi tidak melebihi jumlah kursi)
        numberOfOptions = UnityEngine.Random.Range(2, Mathf.Min(6, spawnPoints.Length + 1));
        currentTimer = numberOfOptions * timePerNPC;

        // Generate Target Data
        SmartClueManager.Instance.GenerateTarget();
        NPCStyleData targetData = SmartClueManager.Instance.currentTarget;
        UpdateMissionText(targetData); // Helper function di bawah

        // Random Kursi & Siapa Targetnya
        List<int> seatIndexes = new List<int>();
        for (int i = 0; i < spawnPoints.Length; i++) seatIndexes.Add(i);
        Shuffle(seatIndexes); // Acak kursi

        List<bool> isTargetList = new List<bool>();
        isTargetList.Add(true);
        for (int i = 0; i < numberOfOptions - 1; i++) isTargetList.Add(false);
        Shuffle(isTargetList); // Acak identitas



        // D. SPAWN & JALAN MASUK (LeanTween)
        float moveDuration = 3f;
        float delayPerNPC = 0.1f;

        for (int i = 0; i < numberOfOptions; i++)
        {
            // Posisi antre di belakang (Lurusan)
            Vector3 queuePos = lurusan.position - (lurusan.forward * (i * 1.0f)); // Jarak antrian dirrenggangkan (1.0f)
            Transform targetSeat = spawnPoints[seatIndexes[i]];

            GameObject newNPC = UnityEngine.Object.Instantiate(npcPrefab, queuePos, Quaternion.identity, spawnContainer);
            newNPC.transform.rotation = Quaternion.Euler(0, 0, 0); // Hadap depan (ke kamera)

            currentNPCs.Add(newNPC);
            NPCVisualControl visualCtrl = newNPC.GetComponent<NPCVisualControl>();
            SetupNPCData(newNPC, isTargetList[i], targetData);

            // --- LOGIKA JALAN ---
            // Delay bertingkat: Orang ke-2 jalan setelah orang ke-1 jalan agak jauh
            float myStartDelay = i * delayPerNPC;

            // 1. Jalan ke Pintu
            LeanTween.move(newNPC, doorStartPoint.position, 3f)
                .setDelay(myStartDelay)
                .setEase(LeanTweenType.easeOutSine) // Linear biar jalannya stabil di lorong
                .setOnComplete(() =>
                {
                    // 2. Sampai Pintu -> Langsung ke Kursi (Tanpa jeda aneh)
                    if (newNPC != null)
                    {
                        // Opsional: Flip kartu jika diperlukan (efek visual)
                        if (visualCtrl != null) visualCtrl.AnimateFlip();

                        // 3. Jalan ke Kursi
                        LeanTween.move(newNPC, targetSeat.position, 3f)
                            .setEase(LeanTweenType.easeInOutQuad) // Melambat saat mau duduk
                            .setOnComplete(() =>
                            {
                                // 4. Sampai Kursi -> Duduk & Hadap Depan
                                if (newNPC != null)
                                {
                                    newNPC.transform.position = targetSeat.position; // Pastikan posisi pas
                                    newNPC.transform.rotation = targetSeat.rotation; // Sesuaikan rotasi kursi

                                    // Efek Balik Badan & Bernafas
                                    if (visualCtrl != null)
                                    {
                                        visualCtrl.AnimateFlip();      // Efek gepeng
                                        visualCtrl.SetDirection(false); // Ganti Punggung
                                                                        // Mulai nafas
                                    }
                                }
                            });
                    }
                });
        }

        // C. BUKA PINTU
        doorController.OpenDoors();
        yield return new WaitForSeconds(1.0f); // Tunggu pintu terbuka
        // D. SPAWN & JALAN MASUK (PERBAIKAN VISUAL)
        float walkToDoorTime = 2.0f; // Waktu jalan dari lorong ke pintu
        float walkToSeatTime = 3f; // Waktu dari pintu ke kursi

        float totalWaitTime = ((numberOfOptions - 1) * delayPerNPC) + walkToDoorTime + walkToSeatTime;

        yield return new WaitForSeconds(totalWaitTime + 0.5f); // Tambah buffer 0.5 detik

        // E. TUTUP PINTU
        if (doorController != null) doorController.CloseDoors();
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
                Debug.Log("WAKTU HABIS!");
                // Trigger Sequence Keluar (Time Up / Salah)
                gamestate.StartCoroutine(ExitSequence(gamestate));
            }
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Debug.Log("DEBUG: REM MENDADAK DI TEKAN!");
            //  targetdone = true;
            gamestate.StartCoroutine(EmergencyStopSequence(gamestate));
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
        isSequenceActive = true; // Kunci lagi
        if (UI != null) UI.SetActive(false); // Sembunyikan UI Misi
        UpdateFloorUI(true);
        startFloor = currentDisplayFloor;
        Debug.Log($"Lift Berhenti di Lantai {startFloor}");
        // A. BUKA PINTU
        if (doorController != null) doorController.OpenDoors();
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

    void SetupNPCData(GameObject npcObj, bool isTarget, NPCStyleData targetData)
    {
        // Ambil Script Controller
        NPCVisualControl visualCtrl = npcObj.GetComponent<NPCVisualControl>();
        NPCIdentity identity = npcObj.GetComponent<NPCIdentity>();

        if (isTarget)
        {
            // SETUP TARGET
            if (visualCtrl != null) visualCtrl.SetupVisuals(targetData);
            if (identity != null) identity.SetupIdentity(targetData, true);
            npcObj.name = "NPC_TARGET";
        }
        else
        {
            // SETUP PENGECOH
            NPCStyleData distraction = SmartClueManager.Instance.GenerateDistraction(distractionSimilarity);
            if (visualCtrl != null) visualCtrl.SetupVisuals(distraction);
            if (identity != null) identity.SetupIdentity(distraction, false);
            npcObj.name = "NPC_Distraction";
        }
    }

    void UpdateMissionText(NPCStyleData data)
    {


        // 1. UPDATE RAMBUT
        if (data.rambut != null)
        {
            rambutt.sprite = data.rambut.visual;
            rambutt.color = Color.white; // <--- PENTING: Paksa Opacity 100%
            rambutt.gameObject.SetActive(true); // Pastikan objek nyala
            // rambutt.preserveAspect = true; // Opsional: Agar gambar tidak gepeng
        }
        else rambutt.gameObject.SetActive(false); // Sembunyikan jika data kosong

        // 2. UPDATE KEPALA
        if (data.kepala != null)
        {
            kepalaa.sprite = data.kepala.visual;
            kepalaa.color = Color.white;
            kepalaa.gameObject.SetActive(true);
        }
        else kepalaa.gameObject.SetActive(false);

        // 3. UPDATE BAJU (Perbaikan: Pakai data.baju, BUKAN data.kepala)
        if (data.baju != null)
        {
            bajuu.sprite = data.baju.visual; // <--- PERBAIKAN DISINI
            bajuu.color = Color.white;
            bajuu.gameObject.SetActive(true);
        }
        else bajuu.gameObject.SetActive(false);

        // 4. UPDATE CELANA (Perbaikan: Pakai data.celana)
        if (data.celana != null)
        {
            celanaa.sprite = data.celana.visual; // <--- PERBAIKAN DISINI
            celanaa.color = Color.white;
            celanaa.gameObject.SetActive(true);
        }
        else celanaa.gameObject.SetActive(false);

        // 5. UPDATE SEPATU (Perbaikan: Pakai data.sepatu)
        if (data.sepatu != null)
        {
            sepatuu.sprite = data.sepatu.visual; // <--- PERBAIKAN DISINI
            sepatuu.color = Color.white;
            sepatuu.gameObject.SetActive(true);
        }
        else sepatuu.gameObject.SetActive(false);

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
    #endregion

    #region StopSequence

    // --- SEQUENCE BARU: SAAT SPASI DITEKAN ---
    IEnumerator EmergencyStopSequence(GameStateManager gamestate)
    {
        // 1. Matikan UI & Logic Update

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



                }
                npc.transform.rotation = Quaternion.identity; // Pastikan tegak
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
    #endregion

    public override void OnEnterState(GameStateManager gamestate) { }
}