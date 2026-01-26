using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LiftController : MonoBehaviour
{
    [Header("Referensi")]
    public LiftRumble scriptGetarLift;

    [Header("Setting Lift")]
    public float durasiPerjalanan = 5f;
    public KeyCode tombolInteraksi = KeyCode.Space; // Bisa diganti jadi E atau F

    // Variabel pengecek
    private bool playerDiDalamLift = false;

    void Update()
    {
        // Logika: 
        // 1. Apakah pemain ada di dalam lift?
        // 2. Apakah pemain menekan Spasi?
        // 3. Apakah lift sedang TIDAK jalan (biar gak spam)?
        if (playerDiDalamLift && Input.GetKeyDown(tombolInteraksi) && !scriptGetarLift.sedangJalan)
        {
            StartCoroutine(ProsesLiftJalan());
        }
    }

    IEnumerator ProsesLiftJalan()
    {
        Debug.Log("Lift Berjalan...");
        scriptGetarLift.MulaiGetar();

        // (Opsional) Play sound lift jalan disini

        yield return new WaitForSeconds(durasiPerjalanan);

        scriptGetarLift.StopGetar();
        Debug.Log("Lift Sampai!");

        // (Opsional) Play sound 'Ting' disini
    }

    // --- LOGIKA DETEKSI PEMAIN ---

    // Saat pemain masuk ke area tombol/lift
    private void OnTriggerEnter(Collider other)
    {
        // Pastikan objek player kamu punya Tag "Player"
        if (other.CompareTag("Player"))
        {
            playerDiDalamLift = true;
            Debug.Log("Siap tekan Spasi");
        }
    }

    // Saat pemain keluar dari area
    private void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            playerDiDalamLift = false;
        }
    }
}