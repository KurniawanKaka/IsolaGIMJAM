using UnityEngine;

public class NPCIdentity : MonoBehaviour
{
    [Header("Status Identitas")]
    public bool isTarget = false; // TRUE jika dia target, FALSE jika pengecoh

    [Header("Data Visual (Untuk Debugging)")]
    // Variabel ini opsional, hanya untuk memudahkan kita melihat data di Inspector
    public NPCStyleData myData;

    // Fungsi ini dipanggil oleh SetUpState saat NPC baru lahir (Spawn)
    public void SetupIdentity(NPCStyleData data, bool status)
    {
        // 1. Simpan Data
        myData = data;
        isTarget = status;

        // 2. Ubah Nama GameObject (Agar mudah dicek di Hierarchy Unity)
        if (isTarget)
        {
            gameObject.name = "NPC_TARGET (Cari Saya!)";
        }
        else
        {
            gameObject.name = "NPC_PENGECOH";
        }
    }
}