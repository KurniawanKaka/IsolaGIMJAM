using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bernafas : MonoBehaviour
{



    // Opsional: Bersihkan saat objek hancur agar tidak memory leak
    void OnDisable()
    {
        LeanTween.cancel(gameObject);
    }


}
