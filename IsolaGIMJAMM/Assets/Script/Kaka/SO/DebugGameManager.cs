using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DebugGameManager : MonoBehaviour
{
    public float nyawa = 3;
    public TextMeshProUGUI nyawaa;

    void Update()
    {
        nyawaa.text = $"Nyawa sisa {nyawa}";
    }
}
