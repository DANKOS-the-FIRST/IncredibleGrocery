using UnityEngine;

public class AudioManager : MonoBehaviour
{
    private int _firstPlayInt;
    public GameObject soundsOn;
    
    public GameObject soundsOff;
    
    public GameObject musicOn;
    
    public GameObject musicOff;

    public AudioSource[] soundsEffectsAudio;
    
    public AudioSource backgroundMusicAudio;
    
    private int _soundsEnabled;
    
    private int _musicEnabled;
    
    
    // Start is called before the first frame update
    void Start()
    {
        _firstPlayInt = PlayerPrefs.GetInt("FirstPlay");

        // if there are no saved data from the previous game session 
        if (_firstPlayInt == 0)
        {
            _soundsEnabled = 0;
            _musicEnabled = 0;
            PlayerPrefs.SetInt("_soundsEnabled", 0);
            PlayerPrefs.SetInt("_musicEnabled", 0);
            PlayerPrefs.SetInt("FirstPlay", -1);
        }
        // if there are saved data from a previous game session
        else
        {
            _soundsEnabled = PlayerPrefs.GetInt("_soundsEnabled");
            _musicEnabled = PlayerPrefs.GetInt("_musicEnabled");
        }
        UpdateAudio();
        UpdateMusicButtonIcon();
        UpdateSoundsButtonIcon();
        // // if there are no saved sounds mode data from the previous game session 
        // if (!PlayerPrefs.HasKey("_soundsEnabled"))
        // {
        //     PlayerPrefs.SetInt("_soundsEnabled", 0);
        // }
        // // if there are no saved music mode data from the previous game session
        // if (!PlayerPrefs.HasKey("_musicEnabled"))
        // {
        //     PlayerPrefs.SetInt("_musicEnabled", 0);
        // }
        //
        // // if there are saved data from a previous game session
        // Load();
        //
        // UpdateMusicButtonIcon();
        // UpdateSoundsButtonIcon();
        // AudioListener.pause = _musicEnabled;
    }

    public void SaveSoundSettings()
    {
        PlayerPrefs.SetInt("_soundsEnabled", _soundsEnabled);
        PlayerPrefs.SetInt("_musicEnabled", _musicEnabled);
    }

    private void UpdateAudio()
    {
        backgroundMusicAudio.volume = _musicEnabled;
        for (int i = 0; i < soundsEffectsAudio.Length; ++i)
        {
            soundsEffectsAudio[i].volume = _soundsEnabled;
        }
    }

    public void OnMusicButtonPress()
    {
        _musicEnabled = _musicEnabled == 0 ? 1 : 0;
        UpdateAudio();
        UpdateMusicButtonIcon();
    }

    public void OnSoundsButtonPress()
    {
        _soundsEnabled = _soundsEnabled == 0 ? 1 : 0;
        UpdateAudio();
        UpdateSoundsButtonIcon();
    }
    private void UpdateMusicButtonIcon()
    {
        if (_musicEnabled == 1)
        {
            musicOn.SetActive(true);
            musicOff.SetActive(false);
        }
        else
        {
            musicOn.SetActive(false);
            musicOff.SetActive(true);
        }
    }
    private void UpdateSoundsButtonIcon()
    {
        if (_soundsEnabled == 1)
        {
            soundsOn.SetActive(true);
            soundsOff.SetActive(false);
        }
        else
        {
            soundsOn.SetActive(false);
            soundsOff.SetActive(true);
        }
    }
}
