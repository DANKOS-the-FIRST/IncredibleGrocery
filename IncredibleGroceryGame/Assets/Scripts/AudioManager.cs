using DefaultNamespace;
using UnityEngine;
using UnityEngine.UI;

public class AudioManager : MonoBehaviour
{
    private int _firstPlayInt;

    private int _soundsEnabled;

    private int _musicEnabled;
    [SerializeField]
    private GameObject soundsButton;
    [SerializeField]
    private GameObject musicButton;
    [SerializeField]
    private Sprite greenImage;
    [SerializeField]
    private Sprite redImage;

    [SerializeField]
    private AudioSource[] soundsEffectsAudio;
    [SerializeField]
    private AudioSource backgroundMusicAudio;
    
    // Start is called before the first frame update
    private void Start()
    {
        _firstPlayInt = PlayerPrefs.GetInt(Constants.FirstPlay);

        // if there are no saved data from the previous game session 
        if (_firstPlayInt == 0)
        {
            _soundsEnabled = 0;
            _musicEnabled = 0;
            PlayerPrefs.SetInt(nameof(_soundsEnabled), 0);
            PlayerPrefs.SetInt(nameof(_musicEnabled), 0);
            PlayerPrefs.SetInt(Constants.FirstPlay, -1);
        }
        // if there are saved data from a previous game session
        else
        {
            _soundsEnabled = PlayerPrefs.GetInt(nameof(_soundsEnabled));
            _musicEnabled = PlayerPrefs.GetInt(nameof(_musicEnabled));
        }

        UpdateAudio();
        UpdateMusicButtonIcon();
        UpdateSoundsButtonIcon();
    }

    public void SaveSoundSettings()
    {
        PlayerPrefs.SetInt(nameof(_soundsEnabled), _soundsEnabled);
        PlayerPrefs.SetInt(nameof(_musicEnabled), _musicEnabled);
    }

    private void UpdateAudio()
    {
        backgroundMusicAudio.volume = _musicEnabled;
        foreach (var t in soundsEffectsAudio)
        {
            t.volume = _soundsEnabled;
        }
    }

    public void OnMusicButtonPress()
    {
        _musicEnabled = _musicEnabled == 0 ? 1 : 0;
        UpdateSettingsValues();
    }

    public void OnSoundsButtonPress()
    {
        _soundsEnabled = _soundsEnabled == 0 ? 1 : 0;
        UpdateSettingsValues();
    }

    public void UpdateSettingsValues()
    {
        UpdateMusicButtonIcon();
        UpdateSoundsButtonIcon();
    }

    private void UpdateMusicButtonIcon()
    {
        if (_musicEnabled == 1)
        {
            musicButton.GetComponent<Button>().image.sprite = greenImage;
            musicButton.transform.GetChild(0).GetComponent<Text>().text = Constants.ON;
        }
        else
        {
            musicButton.GetComponent<Button>().image.sprite = redImage;
            musicButton.transform.GetChild(0).GetComponent<Text>().text = Constants.OFF;
        }

        UpdateAudio();
    }

    private void UpdateSoundsButtonIcon()
    {
        if (_soundsEnabled == 1)
        {
            soundsButton.GetComponent<Button>().image.sprite = greenImage;
            soundsButton.transform.GetChild(0).GetComponent<Text>().text = Constants.ON;
        }
        else
        {
            soundsButton.GetComponent<Button>().image.sprite = redImage;
            soundsButton.transform.GetChild(0).GetComponent<Text>().text = Constants.OFF;
        }

        UpdateAudio();
    }
}