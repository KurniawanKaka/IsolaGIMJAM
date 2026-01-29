
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GameOverPolisiState : GameBaseState
{
    public DoorController teskan;
    public GameObject npcpolisi;
    public Vector3 spawnpoint;
    public NpcJalan jalankan;
    public ItemLookDownSwitcher item;
    public SistemNapas napas;
    public PhotoMechanic pm;

    public Image img;

    public override void EnterState(GameStateManager gamestate)
    {
        item.LockCameraFeature();
        napas.enabled = false;
        Instantiate(npcpolisi, spawnpoint, Quaternion.identity);
        teskan.OpenDoors();
        StartCoroutine(masukpolisi());

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
    IEnumerator masukpolisi()
    {

        yield return new WaitForSeconds(1.9f);
        pm.StartConstantShake(0.3f);
        yield return new WaitForSeconds(2f);

        LeanTween.alpha(img.rectTransform, 1f, 0.01f);
    }

}
