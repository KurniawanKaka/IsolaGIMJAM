
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class EndingState : GameBaseState
{
    public ItemLookDownSwitcher item;
    public AudioManager am;
    public SistemNapas napas;
    public GameObject npcanak;
    public Vector3 spawnpoint;
    public DoorController teskan;
    public PhotoMechanic pm;
    public Image img;
    public override void EnterState(GameStateManager gamestate)
    {
        am.musicSource.Stop();
        item.LockCameraFeature();
        napas.enabled = false;
        Instantiate(npcanak, spawnpoint, Quaternion.identity);
        teskan.OpenDoors();
        StartCoroutine(endingkan());
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

    IEnumerator endingkan()
    {
        yield return new WaitForSeconds(3);
        yield return new WaitForSeconds(1.9f);
        pm.StartConstantShake(0.3f);
        yield return new WaitForSeconds(2f);

        LeanTween.alpha(img.rectTransform, 1f, 0.01f);

    }
}
