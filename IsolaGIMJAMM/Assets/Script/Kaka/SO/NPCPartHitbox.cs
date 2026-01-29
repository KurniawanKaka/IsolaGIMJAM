using UnityEngine;

public class NPCPartHitbox : MonoBehaviour
{
    [Header("Setting")]
    // 0=Rambut, 1=Baju, 2=Celana, 3=Sepatu, 4=Kepala(Netral)
    public int bodyPartIndex;

    [Header("Reference")]
    public NPCVisualControl parentNPC; // Induknya siapa?

    // Fungsi helper untuk mengambil warna dari bagian tubuh ini
    public int GetMyColorIndex()
    {
        if (parentNPC == null || parentNPC.myData == null) return -1;

        switch (bodyPartIndex)
        {
            case 0: return parentNPC.myData.rambutColorIndex;
            case 1: return parentNPC.myData.bajuColorIndex;
            case 2: return parentNPC.myData.celanaColorIndex;
            case 3: return parentNPC.myData.sepatuColorIndex;
            default: return -1;
        }
    }
}