using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement; 

public class GameSceneManager : MonoBehaviour
{
    public static GameSceneManager Instance;

    [Header("Nama Scene (Harus sama persis dengan di Build Settings)")]
    public string sceneMainMenu = "MainMenu";
    public string sceneGameplay = "Gameplay"; // Scene lift utama
    public string sceneProlog = "Prolog";
    public string sceneEpilog = "Epilog";

    [Header("Scene Game Over")]
    public string sceneGONapas = "GameOver_Stress"; // Game Over Napas Habis
    public string sceneGOSalah = "GameOver_Baterai"; // Game Over Gagal 3x

    private void Awake()
    {
        // Pola Singleton: Agar script ini tidak hancur saat pindah scene
        // dan bisa dipanggil dari mana saja.
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

    // --- FUNGSI NAVIGASI ---

    public void KeMainMenu()
    {
        SceneManager.LoadScene(sceneMainMenu);
    }

    public void MulaiGame()
    {
        // Alur: Main Menu -> Prolog -> Gameplay
        SceneManager.LoadScene(sceneProlog);
    }

    public void KeGameplay()
    {
        // Dipanggil setelah Prolog selesai
        SceneManager.LoadScene(sceneGameplay);
    }

    public void KeEpilog()
    {
        // Dipanggil jika player Menang/Selesai level
        SceneManager.LoadScene(sceneEpilog);
    }

    // --- FUNGSI GAME OVER ---

    public void TriggerGameOverNapas()
    {
        Debug.Log("Game Over: Stress Penuh / Napas Habis");
        SceneManager.LoadScene(sceneGONapas);
    }

    public void TriggerGameOverGagal3x()
    {
        Debug.Log("Game Over: Baterai Habis / Salah 3x");
        SceneManager.LoadScene(sceneGOSalah);
    }

    // Fungsi Pembantu untuk Keluar Game
    public void KeluarGame()
    {
        Application.Quit();
        Debug.Log("Keluar Game");
    }
}