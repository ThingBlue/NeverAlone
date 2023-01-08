using NeverAlone;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeverAlone
{
    public class UIManager : MonoBehaviour
    {
        // Singleton
        public static UIManager instance;

        #region Inspector members

        public GameSettings gameSettings;

        /*
        // Pause menu
        public GameObject pauseMenuPanel;
        public Button continueButton;
        public Button settingsButton;
        public Button exitToMenuButton;

        // Settings menu
        public GameObject settingsMenuPanel;
        public Button backButton;
        // Graphics tab
        public GameObject graphicsTabPanel;
        public TMP_Dropdown resolutionDropdown;
        public TMP_Dropdown refreshRateDropdown;
        public TMP_Dropdown fullScreenDropdown;
        public Slider brightnessSlider;
        public TMP_InputField brightnessInputField;
        public Button applyChangesGraphicsButton;
        public Button applyDefaultsGraphicsButton;
        */

        #endregion

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        /*
        private void Start()
        {
            // Disable menus
            pauseMenuPanel.SetActive(false);
            settingsMenuPanel.SetActive(false);
        }

        #region Pause menu

        public void togglePause(bool pause)
        {
            pauseMenuPanel.SetActive(pause);
            if (!pause) settingsMenuPanel.SetActive(false);
        }

        public void onContinueButtonPressed() { GameManager.instance.togglePause(false); }

        public void onSettingsButtonPressed()
        {
            pauseMenuPanel.SetActive(false);
            settingsMenuPanel.SetActive(true);

            // Start up on graphics tab
            graphicsTabPanel.SetActive(true);

            // Set display values for graphics tab
            setGraphicsTabDisplayValues();
        }

        public void onExitToMenuButtonPressed()
        {

        }

        public void onDebugButtonPressed()
        {
            pauseMenuPanel.SetActive(false);
        }

        #endregion

        #region Settings menu

        #region Graphics tab

        public void setGraphicsTabDisplayValues()
        {
            // Resolution dropdown
            if (gameSettings.resolution == new Vector2Int(2560, 1440)) resolutionDropdown.value = 0;
            if (gameSettings.resolution == new Vector2Int(1920, 1080)) resolutionDropdown.value = 1;
            if (gameSettings.resolution == new Vector2Int(1600, 900)) resolutionDropdown.value = 2;
            if (gameSettings.resolution == new Vector2Int(1366, 768)) resolutionDropdown.value = 3;
            if (gameSettings.resolution == new Vector2Int(1360, 768)) resolutionDropdown.value = 4;
            if (gameSettings.resolution == new Vector2Int(1280, 720)) resolutionDropdown.value = 5;
            if (gameSettings.resolution == new Vector2Int(1176, 664)) resolutionDropdown.value = 6;

            // Refresh rate dropdown
            switch (gameSettings.refreshRate)
            {
                case 144:
                    refreshRateDropdown.value = 0;
                    break;
                case 120:
                    refreshRateDropdown.value = 1;
                    break;
                case 100:
                    refreshRateDropdown.value = 2;
                    break;
                case 60:
                    refreshRateDropdown.value = 3;
                    break;
                case 50:
                    refreshRateDropdown.value = 4;
                    break;
                default:
                    break;
            }

            // Full screen dropdown
            switch (gameSettings.fullScreenMode)
            {
                case FullScreenMode.ExclusiveFullScreen:
                    fullScreenDropdown.value = 0;
                    break;
                case FullScreenMode.FullScreenWindow:
                    fullScreenDropdown.value = 1;
                    break;
                case FullScreenMode.Windowed:
                    fullScreenDropdown.value = 2;
                    break;
                default:
                    break;
            }

            // Brightness slider
            brightnessSlider.value = gameSettings.brightness / 200f;

            // Brightness input field
            brightnessInputField.text = Mathf.Round(gameSettings.brightness).ToString();
        }

        public void onGraphicsTabButtonPressed()
        {
            // Activate only current tab
            graphicsTabPanel.SetActive(true);

            // Set display values
            setGraphicsTabDisplayValues();
        }

        public void onBrightnessSliderValueChanged(Slider change)
        {
            // Set input field value
            brightnessInputField.text = Mathf.Round(change.value * 200).ToString();
        }

        public void onBrightnessInputFieldEndEdit(TMP_InputField change)
        {
            // Set slider value
            brightnessSlider.value = int.Parse(change.text) / 200f;
        }

        public void onApplyChangesGraphicsButtonPressed()
        {
            // Resolution
            switch (resolutionDropdown.value)
            {
                case 0:
                    gameSettings.resolution = new(2560, 1440);
                    break;
                case 1:
                    gameSettings.resolution = new(1920, 1080);
                    break;
                case 2:
                    gameSettings.resolution = new(1600, 900);
                    break;
                case 3:
                    gameSettings.resolution = new(1366, 768);
                    break;
                case 4:
                    gameSettings.resolution = new(1360, 768);
                    break;
                case 5:
                    gameSettings.resolution = new(1280, 720);
                    break;
                case 6:
                    gameSettings.resolution = new(1176, 664);
                    break;
                default:
                    break;
            }

            // Refresh rate
            switch (refreshRateDropdown.value)
            {
                case 0:
                    gameSettings.refreshRate = 144;
                    break;
                case 1:
                    gameSettings.refreshRate = 120;
                    break;
                case 2:
                    gameSettings.refreshRate = 100;
                    break;
                case 3:
                    gameSettings.refreshRate = 60;
                    break;
                case 4:
                    gameSettings.refreshRate = 50;
                    break;
                default:
                    break;
            }

            // Full screen
            switch (fullScreenDropdown.value)
            {
                case 0:
                    gameSettings.fullScreenMode = FullScreenMode.ExclusiveFullScreen;
                    break;
                case 1:
                    gameSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
                    break;
                case 2:
                    gameSettings.fullScreenMode = FullScreenMode.Windowed;
                    break;
                default:
                    break;
            }

            // Brightness
            gameSettings.brightness = brightnessSlider.value * 200;

            // Apply changes
            GameManager.instance.applyGraphicsSettings();
        }

        public void onApplyDefaultsGraphicsButtonPressed()
        {
            GameManager.instance.applyGraphicsDefaults();
            setGraphicsTabDisplayValues();
        }

        #endregion

        #endregion
        */
    }
}
