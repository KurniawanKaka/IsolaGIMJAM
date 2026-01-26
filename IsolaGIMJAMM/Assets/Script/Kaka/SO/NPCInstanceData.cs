using UnityEngine;

[System.Serializable]
public class NPCInstanceData
{
    [Header("1. Model Fisik (Sprite Putih)")]
    public NPCBodyPart rambutModel;
    public NPCBodyPart kepalaModel;
    public NPCBodyPart bajuModel;
    public NPCBodyPart celanaModel;
    public NPCBodyPart sepatuModel;

    [Header("2. Index Warna (0-9)")]
    // Angka ini mereferensikan urutan warna di GameColorManager
    public int rambutColorIndex;
    public int kepalaColorIndex;
    public int bajuColorIndex;
    public int celanaColorIndex;
    public int sepatuColorIndex;

    // Constructor kosong (diperlukan untuk inisialisasi)
    public NPCInstanceData() { }

    // Constructor helper untuk clonning data (opsional)
    public NPCInstanceData(NPCInstanceData original)
    {
        this.rambutModel = original.rambutModel;
        this.kepalaModel = original.kepalaModel;
        this.bajuModel = original.bajuModel;
        this.celanaModel = original.celanaModel;
        this.sepatuModel = original.sepatuModel;

        this.rambutColorIndex = original.rambutColorIndex;
        this.kepalaColorIndex = original.kepalaColorIndex;
        this.bajuColorIndex = original.bajuColorIndex;
        this.celanaColorIndex = original.celanaColorIndex;
        this.sepatuColorIndex = original.sepatuColorIndex;
    }
}