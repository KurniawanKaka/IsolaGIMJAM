using UnityEngine;

public class TestColor : MonoBehaviour
{
    public NPCVisualControl npcTarget; // Drag salah satu NPC di scene kesini

    void Start()
    {
        // Kita pura-pura bikin data NPC manual
        NPCInstanceData dataDummy = new NPCInstanceData();

        // Anggap index 0 itu Merah, 1 itu Biru
        dataDummy.bajuColorIndex = 0;
        dataDummy.celanaColorIndex = 1;

        // Masukkan data ke NPC
        npcTarget.SetupVisuals(dataDummy);
    }

    void Update()
    {
        // Tekan SPASI untuk membuka warna Merah (Index 0)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            GameColorManager.Instance.UnlockColor(0);
        }
    }
}