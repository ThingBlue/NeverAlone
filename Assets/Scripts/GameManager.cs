using NeverAlone;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace NeverAlone
{
    public enum GameState
    {
        NONE = 0,
        MAIN_MENU,
        LOADING,
        GAME,
        PAUSED
    }

    public class GameManager : MonoBehaviour
    {
        // Singleton
        public static GameManager instance;

        public static GameState gameState = GameState.NONE;
        public GameSettings gameSettings;

        private float timeScaleBeforePause = 0;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);

            // TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP TEMP
            gameState = GameState.GAME;
        }

        private void Start()
        {
            // Apply settings at the start of the game
            applyGraphicsSettings();
        }

        private void Update()
        {
            // Pause
            if (gameState == GameState.GAME && InputManager.instance.getKeyDown("pause")) togglePause(true);
            // Unpause
            else if (gameState == GameState.PAUSED && InputManager.instance.getKeyDown("pause")) togglePause(false);
        }

        public void togglePause(bool pause)
        {
            // Pause
            if (pause)
            {
                gameState = GameState.PAUSED;
                timeScaleBeforePause = Time.timeScale;
                Time.timeScale = 0;
            }
            // Unpause
            else if (!pause)
            {
                gameState = GameState.GAME;
                Time.timeScale = timeScaleBeforePause;
            }

            // Trigger togglePause callback in UIManager
            //UIManager.instance.togglePause(pause);
        }

        #region Apply settings

        public void applyGraphicsSettings()
        {
            Screen.SetResolution(gameSettings.resolution.x, gameSettings.resolution.y, gameSettings.fullScreenMode, gameSettings.refreshRate);
            Time.fixedDeltaTime = (1f / gameSettings.targetFrameRate);
            Screen.brightness = gameSettings.brightness;
        }

        public void applyAudioSettings()
        {

        }

        public void applyControlsSettings()
        {
            // Game controls
            foreach (KeyCode keyBind in gameSettings.upKeyBinds) InputManager.instance.addKeyToMap("up", keyBind);
            foreach (KeyCode keyBind in gameSettings.downKeyBinds) InputManager.instance.addKeyToMap("down", keyBind);
            foreach (KeyCode keyBind in gameSettings.leftKeyBinds) InputManager.instance.addKeyToMap("left", keyBind);
            foreach (KeyCode keyBind in gameSettings.rightKeyBinds) InputManager.instance.addKeyToMap("right", keyBind);
            foreach (KeyCode keyBind in gameSettings.jumpKeyBinds) InputManager.instance.addKeyToMap("jump", keyBind);
            foreach (KeyCode keyBind in gameSettings.dashKeyBinds) InputManager.instance.addKeyToMap("dash", keyBind);

            // Menu controls
            foreach (KeyCode keyBind in gameSettings.pauseKeyBinds) InputManager.instance.addKeyToMap("pause", keyBind);
        }

        #endregion

        #region Apply defaults

        public void applyGraphicsDefaults()
        {
            gameSettings.resolution = new(1920, 1080);
            gameSettings.refreshRate = 60;
            gameSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            gameSettings.targetFrameRate = 60;
            gameSettings.brightness = 100;

            applyGraphicsSettings();
        }

        public void applyControlsDefaults()
        {
            // Game controls
            gameSettings.upKeyBinds = new List<KeyCode> { KeyCode.W, KeyCode.UpArrow };
            gameSettings.downKeyBinds = new List<KeyCode> { KeyCode.S, KeyCode.DownArrow };
            gameSettings.leftKeyBinds = new List<KeyCode> { KeyCode.A, KeyCode.LeftArrow };
            gameSettings.rightKeyBinds = new List<KeyCode> { KeyCode.D, KeyCode.RightArrow };
            gameSettings.jumpKeyBinds = new List<KeyCode> { KeyCode.Space };
            gameSettings.dashKeyBinds = new List<KeyCode> { KeyCode.LeftShift };

            // Menu controls
            gameSettings.pauseKeyBinds = new List<KeyCode> { KeyCode.Escape };

            applyControlsSettings();
        }

        #endregion
    }
}
