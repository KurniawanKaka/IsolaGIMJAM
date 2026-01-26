using UnityEngine;

public class DoorController : MonoBehaviour
{
    [Header("Door Objects")]
    public Transform leftDoor;
    public Transform rightDoor;

    [Header("Settings")]
    public float openDistance = 1.5f; // Jarak geser pintu saat membuka
    public float doorSpeed = 1.0f;    // Durasi animasi

    private Vector3 leftClosedPos;
    private Vector3 rightClosedPos;

    void Awake()
    {
        // Script ini akan dijalankan paling pertama saat game nyala
        // Jadi posisi tertutup sudah aman tersimpan sebelum ada yang memerintah buka
        if (leftDoor) leftClosedPos = leftDoor.localPosition;
        if (rightDoor) rightClosedPos = rightDoor.localPosition;
    }

    public void OpenDoors()
    {
        if (leftDoor)
            LeanTween.moveLocalX(leftDoor.gameObject, leftClosedPos.x - openDistance, doorSpeed).setEaseInCubic();

        if (rightDoor)
            LeanTween.moveLocalX(rightDoor.gameObject, rightClosedPos.x + openDistance, doorSpeed).setEaseInCubic();
    }

    public void CloseDoors()
    {
        if (leftDoor)
            LeanTween.moveLocalX(leftDoor.gameObject, leftClosedPos.x, doorSpeed).setEaseOutQuart();

        if (rightDoor)
            LeanTween.moveLocalX(rightDoor.gameObject, rightClosedPos.x, doorSpeed).setEaseOutQuart();
    }
}