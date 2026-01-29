using UnityEngine;

[CreateAssetMenu(fileName = "New Part", menuName = "NPC System/Body Part")]
public class NPCBodyPart : ScriptableObject
{
    public string idName; // Untuk teks dulu: "Rambut Cepak", "Kemeja", dll
    public Sprite visual; // Biarkan kosong dulu, nanti diisi kalau aset sudah ada
    public Sprite visualBack;

}