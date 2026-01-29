using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NpcJalan : MonoBehaviour
{

    private void Awake()
    {
        StartCoroutine(jalankan());
    }
    IEnumerator jalankan()
    {
        yield return new WaitForSeconds(3.0f);
        LeanTween.moveZ(gameObject, -0.1f, 1.5f).setEase(LeanTweenType.easeInOutBack);
    }

}
