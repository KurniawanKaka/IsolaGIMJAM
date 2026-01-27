using UnityEngine;
using System.Collections.Generic;

public class SmartClueManager : MonoBehaviour
{
    public static SmartClueManager Instance;
    public NPCDatabase database; // Referensi ke database ScriptableObject

    public NPCStyleData currentTarget; // Target yang harus dicari

    private void Awake() { Instance = this; }

    // 1. GENERATE TARGET UTAMA (Full Random)
    public void GenerateTarget()
    {
        currentTarget = new NPCStyleData();
        currentTarget.rambut = GetRandom(database.hairs);
        currentTarget.baju = GetRandom(database.shirts);
        currentTarget.celana = GetRandom(database.pants);
        currentTarget.sepatu = GetRandom(database.shoes);

    }

    // 2. GENERATE PENGECOH (Dengan Logika Kemiripan)
    // similarityChance: 0.0 = Beda Total, 1.0 = Sangat Mirip
    public NPCStyleData GenerateDistraction(float similarityLevel)
    {
        NPCStyleData distraction = new NPCStyleData();

        // Loop safety agar tidak sengaja membuat "Target Ganda"
        int safetyCount = 0;
        do
        {
            // Reset ke full random dulu

            distraction.rambut = GetRandom(database.hairs);
            distraction.baju = GetRandom(database.shirts);
            distraction.celana = GetRandom(database.pants);
            distraction.sepatu = GetRandom(database.shoes);

            // --- LOGIKA KEMIRIPAN (THE TRICK) ---
            // Kita paksa beberapa bagian SAMA dengan target berdasarkan similarityLevel

            // Cek Rambut: Apakah harus disamakan?
            if (Random.value < similarityLevel) distraction.rambut = currentTarget.rambut;

            // Cek Baju: Apakah harus disamakan?
            if (Random.value < similarityLevel) distraction.baju = currentTarget.baju;

            // Cek Celana: Apakah harus disamakan?
            if (Random.value < similarityLevel) distraction.celana = currentTarget.celana;
            // Cek Celana: Apakah harus disamakan?

            // Cek Celana: Apakah harus disamakan?
            if (Random.value < similarityLevel) distraction.sepatu = currentTarget.sepatu;

            safetyCount++;

        } while (distraction.IsIdentical(currentTarget) && safetyCount < 100);
        // Loop 'while' di atas memastikan pengecoh TIDAK BOLEH 100% sama dengan target

        return distraction;
    }

    private NPCBodyPart GetRandom(List<NPCBodyPart> list)
    {
        // --- PENGAMAN ---
        // Cek apakah listnya kosong atau null?
        if (list == null || list.Count == 0)
        {
            Debug.LogError("GAWAT! Ada list di Database yang KOSONG (Size 0). Cek NPCDatabase di Inspector!");
            return null; // Kembalikan null biar gak crash
        }
        // ----------------

        return list[Random.Range(0, list.Count)];
    }
}