using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class Audio : MonoBehaviour
{
    private enum AudioSlider { GENERAL, MUSIC, EFFECTS, VOICES, COUNT };
    private AudioSlider aSlider = AudioSlider.GENERAL;
    public TMP_Text SliderText;

    public AudioMixer master;
    public Slider volume;

    private void Start()
    {
        GetSliderPositions();
    }

    #region SliderControls
    public void SetSlider(float _sliderValue)
    {
        if (volume != null)
        {
            volume.value = _sliderValue;
        }
    }
    #endregion SliderControls

    #region SetMixerPrefs
    public void SetMasterPref(float _value)
    {
        master.SetFloat("MasterVol", Mathf.Log10(_value) * 20);
        UpdateMasterVol(_value);
        SliderText.text = _value.ToString("F0");
    }

    public void SetMusicPref(float _value)
    {
        master.SetFloat("MusicVol", Mathf.Log10(_value) * 20);
        UpdateMusicVol(_value);
        SliderText.text = _value.ToString("F0");
    }

    public void SetEffectsPref(float _value)
    {
        master.SetFloat("EffectsVol", Mathf.Log10(_value) * 20);
        UpdateEffectsVol(_value);
        SliderText.text = _value.ToString("F0");
    }

    public void SetVoicesPref(float _value)
    {
        master.SetFloat("VoicesVol", Mathf.Log10(_value) * 20);
        UpdateVoicesVol(_value);
        SliderText.text = _value.ToString("F0");
    }
    #endregion SetMixerPrefs

    public void GetAudioPrefabs()
    {
        SetMasterPref(GetMasterVolPrefs());
        SetMusicPref(GetMusicVolPrefs());
        SetEffectsPref(GetEffectsVolPrefs());
        SetEffectsPref(GetVoicesVolPrefs());
    }

    public void GetSliderPositions()
    {
        if (this.name == "General Volume Slider")
        {
            aSlider = AudioSlider.GENERAL;
        }

        else if (this.name == "Music Volume Slider")
        {
            aSlider = AudioSlider.MUSIC;
        }

        else if (this.name == "Effects Volume Slider")
        {
            aSlider = AudioSlider.EFFECTS;
        }

        else if (this.name == "Voices Volume Slider")
        {
            aSlider = AudioSlider.VOICES;
        }

        switch (aSlider)
        {
            case AudioSlider.GENERAL:
                SetSlider(GetMasterVolPrefs());
                break;

            case AudioSlider.MUSIC:
                SetSlider(GetMusicVolPrefs());
                break;

            case AudioSlider.EFFECTS:
                SetSlider(GetEffectsVolPrefs());
                break;

            case AudioSlider.VOICES:
                SetSlider(GetVoicesVolPrefs());
                break;

            default:
                SetSlider(0f);
                break;
        }
    }

    #region Setters
    public static void UpdateMasterVol(float _newMixerVol)
    {
        PlayerPrefs.SetFloat("MasterVol", _newMixerVol);
        PlayerPrefs.Save();
    }

    public static void UpdateMusicVol(float _newMixerVol)
    {
        PlayerPrefs.SetFloat("MusicVol", _newMixerVol);
        PlayerPrefs.Save();
    }

    public static void UpdateEffectsVol(float _newMixerVol)
    {
        PlayerPrefs.SetFloat("EffectsVol", _newMixerVol);
        PlayerPrefs.Save();
    }

    public static void UpdateVoicesVol(float _newMixerVol)
    {
        PlayerPrefs.SetFloat("VoicesVol", _newMixerVol);
        PlayerPrefs.Save();
    }
    #endregion Setters
    #region Getters
    public static float GetMasterVolPrefs()
    {
        if (!PlayerPrefs.HasKey("MasterVol"))
        {
            UpdateMasterVol(1f);
        }
        return PlayerPrefs.GetFloat("MasterVol");
    }

    public static float GetMusicVolPrefs()
    {
        if (!PlayerPrefs.HasKey("MusicVol"))
        {
            UpdateMusicVol(1f);
        }
        return PlayerPrefs.GetFloat("MusicVol");
    }

    public static float GetEffectsVolPrefs()
    {
        if (!PlayerPrefs.HasKey("EffectsVol"))
        {
            UpdateEffectsVol(0.5f);
        }
        return PlayerPrefs.GetFloat("EffectsVol");
    }

    public static float GetVoicesVolPrefs()
    {
        if (!PlayerPrefs.HasKey("VoicesVol"))
        {
            UpdateVoicesVol(1f);
        }
        return PlayerPrefs.GetFloat("VoicesVol");
    }
    #endregion Getters
}
