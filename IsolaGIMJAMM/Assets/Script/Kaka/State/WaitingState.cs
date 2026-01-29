
using System.Threading;
using TMPro;
using UnityEngine;

public class WaitingState : GameBaseState
{
    [SerializeField] private float durasiAwal = 10;
    float timer;

    public ItemLookDownSwitcher pm;
    public PhotoMechanic cam;

    public GameColorManager color;
    public GameDifficultyManager gm;

    // public TextMeshProUGUI tek; // Panel Kiri
    //[SerializeField] public GameObject tekk;

    public override void EnterState(GameStateManager gamestate)
    {
        Debug.Log("masukk, tunggu waktu habis");


        pm.LockCameraFeature();
        // 1. RESET TIMER (Penting! Agar kalau masuk state ini lagi, waktu ulang dari 10)
        timer = durasiAwal;
        //   pm.SetInputActive(false);


        // tekk.SetActive(true);
    }

    public override void UpdateState(GameStateManager gamestate)
    {
        ; timer -= Time.deltaTime;

        // Tampilkan angka bulat (F0) agar rapi
        //  tek.text = timer.ToString("F0");

        // PERBAIKAN DISINI: Gunakan Kurang Dari Sama Dengan (<=)

        if (timer <= 0)
        {
            checker(gamestate);
            // Pastikan timer berhenti di 0 secara visual
            timer = 0;
            // tek.text = "0";
            // cam.TriggerShake(0.5f, 0.5f);


            // Cek apakah gm ditemukan untuk menghindari crash
            // if (gm.nyawa <= 0) // Gunakan <= untuk nyawa juga biar aman
            // {
            //     gamestate.SwitchState(gamestate.gameoverstate);
            // }
            // else
            // {
            // }



            // Matikan UI setelah pindah
            // tekk.SetActive(false);
        }
    }

    public override void OnEnterState(GameStateManager gamestate)
    {

    }
    public override void ExitState(GameStateManager gamestate)
    {

    }

    void checker(GameStateManager gameState)
    {

        float warnaterbuka = color.GetUnlockedColorsCount();
        if (gm.nyawa <= 0)
        {
            gameState.SwitchState(gameState.gameoverstate);
        }
        else if (warnaterbuka == 9)
        {
            gameState.SwitchState(gameState.endingstate);
        }
        else
        {
            gameState.SwitchState(gameState.setupstate);
        }
    }
}
