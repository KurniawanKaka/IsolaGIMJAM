
using System.Threading;
using TMPro;
using UnityEngine;

public class WaitingState : GameBaseState
{
    [SerializeField] private float durasiAwal = 10;
    float timer;

    public ItemLookDownSwitcher pm;
    public PhotoMechanic cam;

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
            // Pastikan timer berhenti di 0 secara visual
            timer = 0;
            // tek.text = "0";
            // cam.TriggerShake(0.5f, 0.5f);


            // Cek apakah gm ditemukan untuk menghindari crash

            gamestate.SwitchState(gamestate.setupstate);
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
}
