using UnityEngine;

public class PlayerInteraction : MonoBehaviour
{
    [Header("Settings")]
    public LayerMask npcLayer; // Pastikan NPC ada di layer Default atau set layer khusus
    public DebugGameManager gm; // Untuk kurangi nyawa kalau salah

    void Update()
    {
        // Deteksi Klik Kiri Mouse
        if (Input.GetMouseButtonDown(0))
        {
            ShootRaycast();
        }
    }

    void ShootRaycast()
    {
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;

        if (Physics.Raycast(ray, out hit, 100f)) // Pastikan hapus filter layer di inspector atau biarkan default
        {
            // Cek apakah kita mengenai BAGIAN TUBUH (Hitbox)?
            NPCPartHitbox hitPart = hit.collider.GetComponent<NPCPartHitbox>();

            if (hitPart != null)
            {
                CheckBodyPart(hitPart);
            }
        }
    }

    void CheckBodyPart(NPCPartHitbox part)
    {
        // 1. Ambil Kunci Jawaban
        int targetColorIndex = GameColorManager.Instance.currentRoundTargetColorIndex;

        // 2. Ambil Warna dari bagian tubuh yang DIKLIK
        int clickedColorIndex = part.GetMyColorIndex();

        // 3. Debugging biar jelas
        Debug.Log($"Klik Bagian: {part.bodyPartIndex} | Warnanya: {clickedColorIndex} | Target: {targetColorIndex}");

        // 4. VALIDASI
        if (clickedColorIndex == targetColorIndex)
        {
            Debug.Log("BENAR! Bagian tubuh ini mengandung warna target!");
            GameColorManager.Instance.UnlockColor(targetColorIndex);

            // Efek tambahan: Bikin NPC kaget/senang?
            // part.parentNPC.AnimateSuccess(); 
        }
        else
        {
            Debug.Log("SALAH PILIH! (Salah NPC atau Salah Bagian Tubuh)");
            if (gm != null) gm.nyawa--;
        }
    }
    void CheckNPC(NPCVisualControl npc)
    {
        // 1. Ambil Kunci Jawaban dari Manager
        int targetIndex = GameColorManager.Instance.currentRoundTargetColorIndex;

        // 2. Cek apakah NPC ini memiliki warna tersebut?
        // Kita cek seluruh badannya (Baju, Celana, dll)
        bool isCorrect = false;

        if (npc.myData.rambutColorIndex == targetIndex) isCorrect = true;
        else if (npc.myData.bajuColorIndex == targetIndex) isCorrect = true;
        else if (npc.myData.celanaColorIndex == targetIndex) isCorrect = true;
        else if (npc.myData.sepatuColorIndex == targetIndex) isCorrect = true;

        // 3. Penentuan Menang/Kalah
        if (isCorrect)
        {
            Debug.Log("BENAR! Warna Ditemukan!");

            // Unlock warna tersebut secara global
            GameColorManager.Instance.UnlockColor(targetIndex);

            // Tambahkan logika lain: Misal panggil fungsi 'LevelSelesai' di GameManager
            // FindObjectOfType<SetUpState>().targetdone = true; // Contoh trigger selesai
        }
        else
        {
            Debug.Log("SALAH! Itu bukan warna yang dicari.");

            // Kurangi Nyawa / Baterai
            if (gm != null) gm.nyawa--;
        }
    }
}