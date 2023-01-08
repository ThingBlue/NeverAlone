using NeverAlone;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace NeverAlone
{
    public class InputManager : MonoBehaviour
    {
        // Singleton
        public static InputManager instance;

        public Dictionary<string, List<KeyCode>> keyMap;

        private void Awake()
        {
            // Singleton
            if (instance == null) instance = this;
            else Destroy(this);
        }

        private void Start()
        {
            // Initialize key map
            keyMap = new Dictionary<string, List<KeyCode>>();

            // TEMPORARY: Add default keys
            addKeyToMap("left", KeyCode.A);
            addKeyToMap("left", KeyCode.LeftArrow);
            addKeyToMap("right", KeyCode.D);
            addKeyToMap("right", KeyCode.RightArrow);
            addKeyToMap("up", KeyCode.W);
            addKeyToMap("up", KeyCode.UpArrow);
            addKeyToMap("down", KeyCode.S);
            addKeyToMap("down", KeyCode.DownArrow);

            addKeyToMap("jump", KeyCode.Space);
            addKeyToMap("dash", KeyCode.LeftShift);
        }

        #region Getters

        public bool getKey(string key)
        {
            if (!keyMap.ContainsKey(key)) return false;

            foreach (KeyCode keyCode in keyMap[key])
            {
                if (Input.GetKey(keyCode)) return true;
            }
            return false;
        }

        public bool getKeyDown(string key)
        {
            if (!keyMap.ContainsKey(key)) return false;

            foreach (KeyCode keyCode in keyMap[key])
            {
                if (Input.GetKeyDown(keyCode)) return true;
            }
            return false;
        }

        public bool getKeyUp(string key)
        {
            if (!keyMap.ContainsKey(key)) return false;

            foreach (KeyCode keyCode in keyMap[key])
            {
                if (Input.GetKeyUp(keyCode)) return true;
            }
            return false;
        }

        // Getter for key map
        public List<KeyCode> getKeysInMap(string key)
        {
            if (!keyMap.ContainsKey(key)) return new List<KeyCode>();

            return keyMap[key];
        }

        // Getter for mouse position in world space
        public Vector3 getMousePositionInWorld()
        {
            // Create plane at zero
            Plane plane = new Plane(Vector3.back, Vector3.zero);

            // Cast ray onto plane
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            // Return hit location
            if (plane.Raycast(ray, out float enter)) return ray.GetPoint(enter);
            return Vector3.zero; // Should never execute
        }

        #endregion

        #region Setters

        public void clearKeyListInMap(string key)
        {
            if (!keyMap.ContainsKey(key)) return;

            keyMap[key].Clear();
        }

        public void addKeyToMap(string key, KeyCode value)
        {
            // Create new keycode mapping if it doesn't exist
            if (!keyMap.ContainsKey(key)) keyMap.Add(key, new List<KeyCode>());

            // Check if current value already exists in the list
            if (keyMap[key].Contains(value)) return;

            keyMap[key].Add(value);
        }

        public void setKeyListInMap(string key, List<KeyCode> value)
        {
            // Create new keycode mapping if it doesn't exist
            if (!keyMap.ContainsKey(key)) keyMap.Add(key, value);
            else keyMap[key] = value;
        }

        #endregion
    }
}
