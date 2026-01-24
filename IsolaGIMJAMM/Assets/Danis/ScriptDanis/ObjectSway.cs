using UnityEngine;

public class ObjectSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float amount = 0.02f;
    public float maxAmount = 0.06f;
    public float smoothAmount = 6f;

    private Vector3 initialPos;

    void Start()
    {
        // Simpan posisi lokal awal (biasanya 0,0,0 kalau udah diparenting)
        initialPos = transform.localPosition;
    }

    void Update()
    {
        // 1. Ambil Input Mouse
        float movementX = -Input.GetAxis("Mouse X") * amount;
        float movementY = -Input.GetAxis("Mouse Y") * amount;

        // 2. Batasin (Clamp)
        movementX = Mathf.Clamp(movementX, -maxAmount, maxAmount);
        movementY = Mathf.Clamp(movementY, -maxAmount, maxAmount);

        // 3. Hitung Target Posisi
        Vector3 finalPosition = new Vector3(movementX, movementY, 0);

        // 4. Gerakin Spritenya (Relatif terhadap Holder)
        transform.localPosition = Vector3.Lerp(transform.localPosition, finalPosition + initialPos, Time.deltaTime * smoothAmount);
    }
}