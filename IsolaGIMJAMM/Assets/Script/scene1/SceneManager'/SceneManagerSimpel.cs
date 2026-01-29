using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneManagerSimpel : MonoBehaviour
{
    public static SceneManagerSimpel Instance;

    // 1. DAFTAR PILIHAN SCENE (Enum)
    // Ini memudahkanmu saat coding, jadi gak perlu ketik string manual (anti typo)
    public enum TipeScene
    {
        MainMenu,
        Prolog,
        Gameplay,
        Epilog,
        GameOver_NapasHabis,
        GameOver_Gagal3x
    }

    [Header("Nama File Scene di Build Settings")]
    // Isi nama scene asli kamu di sini lewat Inspector
    public string fileMainMenu = "MainMenu";
    public string fileProlog = "Prolog";
    public string fileGameplay = "Gameplay";
    public string fileEpilog = "Epilog";
    public string fileGONapas = "GameOver_Stress";
    public string fileGOGagal = "GameOver_Baterai";

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    // --- 2. SATU FUNGSI UNTUK SEMUA ---

    public void GantiScene(TipeScene tujuan)
    {
        switch (tujuan)
        {
            case TipeScene.MainMenu:
                SceneManager.LoadScene(fileMainMenu);
                break;

            case TipeScene.Prolog:
                SceneManager.LoadScene(fileProlog);
                break;

            case TipeScene.Gameplay:
                SceneManager.LoadScene(fileGameplay);
                break;

            case TipeScene.Epilog:
                SceneManager.LoadScene(fileEpilog);
                break;

            case TipeScene.GameOver_NapasHabis:
                Debug.Log("Kalah karena panik!");
                SceneManager.LoadScene(fileGONapas);
                break;

            case TipeScene.GameOver_Gagal3x:
                Debug.Log("Kalah karena baterai/salah tebak!");
                SceneManager.LoadScene(fileGOGagal);
                break;
        }
    }

    // --- KHUSUS TOMBOL (Opsional) ---
    // Karena tombol UI Unity susah baca Enum, kita buat fungsi bantuan string
    // Pasang ini di tombol "Start" atau "Back to Menu"
    public void TombolPindah(string namaTipe)
    {
        // Konversi string ke Enum
        if (System.Enum.TryParse(namaTipe, out TipeScene hasil))
        {
            GantiScene(hasil);
        }
        else
        {
            Debug.LogError("Nama scene salah ketik di tombol!");
        }
    }
}