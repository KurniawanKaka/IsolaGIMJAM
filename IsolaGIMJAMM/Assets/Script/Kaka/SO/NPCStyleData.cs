// Helper struct untuk menampung kombinasi
[System.Serializable]
public class NPCStyleData
{
    public NPCBodyPart rambut;
    public NPCBodyPart baju;
    public NPCBodyPart ekspresi;
    public NPCBodyPart celana;
    public NPCBodyPart sepatu;


    // Fungsi untuk cek apakah ini sama persis dengan target (Clone check)
    public bool IsIdentical(NPCStyleData other)
    {
        return rambut == other.rambut && baju == other.baju && celana == other.celana && sepatu == other.sepatu;
    }

    // Fungsi untuk meng-copy data dari style lain
    public void CopyFrom(NPCStyleData other)
    {
        rambut = other.rambut;
        baju = other.baju;
        celana = other.celana;
        sepatu = other.sepatu;
    }
}