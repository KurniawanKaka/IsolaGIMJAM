using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "NPC_Database", menuName = "NPC System/Database")]
public class NPCDatabase : ScriptableObject
{
    [Header("Koleksi Aset")]
    public List<NPCBodyPart> hairs;
    public List<NPCBodyPart> heads; // Jenis bentuk kepala
    public List<NPCBodyPart> shirts;
    public List<NPCBodyPart> pants;
    public List<NPCBodyPart> shoes;

    public NPCBodyPart GetRandomHair()
    {
        if (hairs == null || hairs.Count == 0) return null;
        return hairs[Random.Range(0, hairs.Count)];
    }

    public NPCBodyPart GetRandomHead()
    {
        if (heads == null || heads.Count == 0) return null;
        return heads[Random.Range(0, heads.Count)];
    }

    public NPCBodyPart GetRandomShirt()
    {
        if (shirts == null || shirts.Count == 0) return null;
        return shirts[Random.Range(0, shirts.Count)];
    }

    public NPCBodyPart GetRandomPants()
    {
        if (pants == null || pants.Count == 0) return null;
        return pants[Random.Range(0, pants.Count)];
    }

    public NPCBodyPart GetRandomShoes()
    {
        if (shoes == null || shoes.Count == 0) return null;
        return shoes[Random.Range(0, shoes.Count)];
    }
}