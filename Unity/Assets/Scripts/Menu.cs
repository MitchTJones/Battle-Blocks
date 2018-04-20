using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Menu : MonoBehaviour {
    public GameObject escMenu;
    public GameObject settingsMenu;
    [Header("Settings Menu")]
    public Dropdown resDropdown;
    public Toggle fullscreenToggle;
    public Text volText;

    public bool muted;
    public float currentVolume;

    public bool fullscreen;
    public Vector2 resolution = new Vector2(Screen.width, Screen.height);

    private void Start()
    {
        resolution.Set(Screen.width, Screen.height);
    }

    #region EscMenu
    public void OnClick_Settings()
    {
        escMenu.SetActive(false);
        settingsMenu.SetActive(true);
    }

    public void OnClick_MainMenu()
    {
        Manager.Instance.ReturnToMenu();
    }
    #endregion

    #region SettingsMenu
    public void OnClick_ChangeRes(int value)
    {
        string res = resDropdown.options[resDropdown.value].text;
        resolution.Set(int.Parse(res.Split('×')[0]), int.Parse(res.Split('×')[1]));
        ApplyResolution();
    }

    public void OnClick_Fullscreen(bool fs)
    {
        fullscreen = fullscreenToggle.isOn;
        ApplyResolution();
    }

    public void OnClick_ChangeVol(Slider slider)
    {
        currentVolume = slider.value;
        ApplyVolume();
        volText.text = "" + (int)(slider.value*100);
    }

    public void OnClick_Mute(bool m)
    {
        muted = m;
        ApplyVolume();
    }

    public void OnClick_Apply()
    {
        ApplyVolume();
        ApplyResolution();
    }

    public void OnClick_Back()
    {
        Manager.Instance.MenuBack();
    }
    #endregion

    public void ApplyVolume()
    {
        if (muted)
        {
            AudioListener.volume = 0;
        }
        else
        {
            AudioListener.volume = currentVolume;
        }
    }

    public void ApplyResolution()
    {
        Screen.SetResolution((int)resolution.x, (int)resolution.y, fullscreen);
    }
}
