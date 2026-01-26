using UnityEngine;

public class ObjectSway : MonoBehaviour
{
    [Header("Sway Settings")]
    public float amount = 0.05f;
    public float maxAmount = 0.1f;
    public float smoothAmount = 5f;

    [HideInInspector] public bool canSway = true;
    private Vector3 initialPos;

    void Start()
    {
        // Mencatat posisi awal relatif terhadap Parent
        initialPos = transform.localPosition;
    }

    void Update()
    {
        Vector3 targetPos = initialPos;

        if (canSway)
        {
            float moveX = -Input.GetAxis("Mouse X") * amount;
            float moveY = -Input.GetAxis("Mouse Y") * amount;

            moveX = Mathf.Clamp(moveX, -maxAmount, maxAmount);
            moveY = Mathf.Clamp(moveY, -maxAmount, maxAmount);

            targetPos = new Vector3(initialPos.x + moveX, initialPos.y + moveY, initialPos.z);
        }

        // Lerp biar goyangannya kerasa smooth, nggak kaku
        transform.localPosition = Vector3.Lerp(transform.localPosition, targetPos, Time.deltaTime * smoothAmount);
    }
}