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
}