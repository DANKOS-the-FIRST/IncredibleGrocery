using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SettingsMenu : MonoBehaviour
{
    public static bool GameIsPaused = false;
    
    public GameObject settingsMenuUI;
    
    public void ShowSettings()
    {
        Pause();
    }

    public void ExitSettings()
    {
        Resume();
    }
    
    private void Resume()
    {
        settingsMenuUI.SetActive(false);
        Time.timeScale = 1f;
        GameIsPaused = false;
    }

    private void Pause()
    {
        settingsMenuUI.SetActive(true);
        Time.timeScale = 0f;
        GameIsPaused = true;
    }

}
