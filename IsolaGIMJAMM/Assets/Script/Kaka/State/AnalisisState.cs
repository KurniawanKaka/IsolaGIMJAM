
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering;

public class AnalisisState : GameBaseState
{

    public AudioManager am;

    public SistemNapas napas;
    public ItemLookDownSwitcher look;

    public TextMeshProUGUI teks;
    public override void EnterState(GameStateManager gamestate)
    {
        napas.enabled = false;
        look.LockCameraFeature();
        StartCoroutine(Mulai(gamestate));
    }

    public override void UpdateState(GameStateManager gamestate)
    {

    }

    public override void OnEnterState(GameStateManager gamestate)
    {

    }
    public override void ExitState(GameStateManager gamestate)
    {

    }

    IEnumerator Mulai(GameStateManager gamestate)
    {
        yield return new WaitForSeconds(3f);
        am.PlaySFX(am.notif);
        yield return new WaitForSeconds(1f);
        am.PlaySFX(am.vokaka);
        yield return new WaitForSeconds(1f);
        teks.text = "Selamat datang di Cianjir Residence";
        yield return new WaitForSeconds(3f);
        teks.text = "tempat dimana kamu bisa bermain dan tumbuh Bersama";
        yield return new WaitForSeconds(4f);
        teks.text = "lihat kiri kanan kamu, kami sudah menyiapkan fasilitas yang kamu butuhkan";
        yield return new WaitForSeconds(5f);
        teks.text = "jika kamu ingin sesuatu, kita akan wujudkan";
        yield return new WaitForSeconds(3f);
        teks.text = "selamat bermain dan tetaplah";
        yield return new WaitForSeconds(2f);
        teks.text = "BERNAFAS";
        yield return new WaitForSeconds(3f);
        teks.text = "";
        yield return new WaitForSeconds(3f);
        gamestate.SwitchState(gamestate.waitingstate);
    }
}
