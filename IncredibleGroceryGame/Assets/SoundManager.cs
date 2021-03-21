using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundManager : MonoBehaviour
{
    [SerializeField] Image musicOnBg;
    [SerializeField] Text musicOnText;
    [SerializeField] Image musicOffBg;
    [SerializeField] Text musicOffText;
    private bool musicMuted = false;
    
    // Start is called before the first frame update
    void Start()
    {
        // if there are no saved data from the previous game session
        if (!PlayerPrefs.HasKey("muted"))
        {
            PlayerPrefs.SetInt("muted", 0);
            Load();
        }
        // if there are saved data from a previous game session
        else
        {
            Load();
        }
        UpdateButtonIcon();
        AudioListener.pause = musicMuted;
    }

    public void OnButtonPress()
    {
        if (musicMuted == false)
        {
            musicMuted = true;
            AudioListener.pause = true;
        }

        else
        {
            musicMuted = false;
            AudioListener.pause = false;
        }

        Save();
        UpdateButtonIcon();
    }

    private void UpdateButtonIcon()
    {
        if (musicMuted == false)
        {
            musicOnBg.enabled = true;
            musicOnText.enabled = true;
            musicOffBg.enabled = false;
            musicOffText.enabled = false;
        }
        
        else
        {
            musicOnBg.enabled = false;
            musicOnText.enabled = false;
            musicOffBg.enabled = true;
            musicOffText.enabled = true;
        }
    }
    
    private void Load()
    {
        musicMuted = PlayerPrefs.GetInt("muted") == 1;
    }

    private void Save()
    {
        PlayerPrefs.SetInt("muted", musicMuted ? 1 : 0);
    }
}
