using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
#if UNITY_5_6_OR_NEWER
using UnityEditor.IMGUI.Controls;
#endif
using UnityEngine;

namespace UMANG
{
    public class PlayerPrefsEditor : EditorWindow
    {
        private static System.Text.Encoding encoding = new System.Text.UTF8Encoding();

        // Represents a PlayerPref key-value record
        [Serializable]
        private struct PlayerPrefPair
        {
            public string Key { get; set; }
            public object Value { get; set; }
        }
        readonly DateTime MISSING_DATETIME = new DateTime(1601, 1, 1);

        // Natively PlayerPrefs can be one of these three types
        enum PlayerPrefType { Float = 0, Int, String, Bool };

        // The actual cached store of PlayerPref records fetched from registry or plist
        List<PlayerPrefPair> deserializedPlayerPrefs = new List<PlayerPrefPair>();

        // Track last successful deserialisation to prevent doing this too often. On OSX this uses the PlayerPrefs file
        // last modified time, on Windows we just poll repeatedly and use this to prevent polling again too soon.
        DateTime? lastDeserialization = null;

        // The view position of the PlayerPrefs scroll view
        Vector2 scrollPosition;

        // The scroll position from last frame (used with scrollPosition to detect user scrolling)
        Vector2 lastScrollPosition;

        // Prevent OnInspector() forcing a repaint every time it's called
        int inspectorUpdateFrame = 0;

        // Because of some issues with deleting from OnGUI, we defer it to OnInspectorUpdate() instead
        string keyQueuedForDeletion = null;

        #region Adding New PlayerPref
        // This is the current type of PlayerPref that the user is about to create
        PlayerPrefType newEntryType = PlayerPrefType.String;

        // The identifier of the new PlayerPref
        string newEntryKey = "";

        // Value of the PlayerPref about to be created (must be tracked differently for each type)
        float newEntryValueFloat = 0;
        int newEntryValueInt = 0;
        bool newEntryValueBool = false;
        string newEntryValueString = "";
        #endregion

        [MenuItem("Umang Utilities/PlayerPrefs Window", priority = 111)]
        static void Init()
        {
            // Get existing open window or if none, make a new one:
            PlayerPrefsEditor editor = (PlayerPrefsEditor)GetWindow(typeof(PlayerPrefsEditor), false, "Prefs Editor");

            // Require the editor window to be at least 300 pixels wide
            Vector2 minSize = editor.minSize;
            minSize.x = 230;
            editor.minSize = minSize;
        }

        void DeleteAll()
        {
            PlayerPrefs.DeleteAll();
        }

        void DeleteKey(string key)
        {
            PlayerPrefs.DeleteKey(key);
        }

        int GetInt(string key, int defaultValue = 0)
        {
            return PlayerPrefs.GetInt(key, defaultValue);
        }

        float GetFloat(string key, float defaultValue = 0.0f)
        {
            return PlayerPrefs.GetFloat(key, defaultValue);
        }
        string GetString(string key, string defaultValue = "")
        {
            return PlayerPrefs.GetString(key, defaultValue);
        }
        bool GetBool(string key, bool defaultValue = false)
        {
            throw new NotSupportedException("PlayerPrefs interface does not natively support bools");
        }
        void SetInt(string key, int value)
        {
            PlayerPrefs.SetInt(key, value);
        }
        void SetFloat(string key, float value)
        {
            PlayerPrefs.SetFloat(key, value);
        }
        void SetString(string key, string value)
        {
            PlayerPrefs.SetString(key, value);
        }
        void SetBool(string key, bool value)
        {
            throw new NotSupportedException("PlayerPrefs interface does not natively support bools");
        }
        void Save()
        {
            PlayerPrefs.Save();
        }
        /// <summary>
        /// This returns an array of the stored PlayerPrefs from the file system (OSX) or registry (Windows), to allow
        /// us to to look up what's actually in the PlayerPrefs. This is used as a kind of lookup table.
        /// </summary>
        PlayerPrefPair[] RetrieveSavedPrefs(string companyName, string productName)
        {
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                string playerPrefsPath;
                // From Unity Docs: On Mac OS X PlayerPrefs are stored in ~/Library/Preferences folder, in a file named unity.[company name].[product name].plist, where company and product names are the names set up in Project Settings. The same .plist file is used for both Projects run in the Editor and standalone players.
                // Construct the plist filename from the project's settings
                string plistFilename = string.Format("unity.{0}.{1}.plist", companyName, productName);
                // Now construct the fully qualified path
                playerPrefsPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Preferences"), plistFilename);
                // Parse the PlayerPrefs file if it exists
                if (File.Exists(playerPrefsPath))
                {
                    // Parse the plist then cast it to a Dictionary
                    object plist = Plist.readPlist(playerPrefsPath);
                    Dictionary<string, object> parsed = plist as Dictionary<string, object>;
                    // Convert the dictionary data into an array of PlayerPrefPairs
                    List<PlayerPrefPair> tempPlayerPrefs = new List<PlayerPrefPair>(parsed.Count);
                    foreach (KeyValuePair<string, object> pair in parsed)
                    {
                        if (pair.Value.GetType() == typeof(double))
                        {
                            // Some float values may come back as double, so convert them back to floats
                            tempPlayerPrefs.Add(new PlayerPrefPair() { Key = pair.Key, Value = (float)(double)pair.Value });
                        }
                        else if (pair.Value.GetType() == typeof(bool))
                        {
                            // Unity PlayerPrefs API doesn't allow bools, so ignore them
                        }
                        else
                        {
                            tempPlayerPrefs.Add(new PlayerPrefPair() { Key = pair.Key, Value = pair.Value });
                        }
                    }
                    // Return the results
                    return tempPlayerPrefs.ToArray();
                }
                else
                {
                    // No existing PlayerPrefs saved (which is valid), so just return an empty array
                    return new PlayerPrefPair[0];
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                Microsoft.Win32.RegistryKey registryKey;
                // From Unity docs: On Windows, PlayerPrefs are stored in the registry under HKCU\Software\[company name]\[product name] key, where company and product names are the names set up in Project Settings.
#if UNITY_5_5_OR_NEWER
                // From Unity 5.5 editor PlayerPrefs moved to a specific location
                registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\Unity\\UnityEditor\\" + companyName + "\\" + productName);
#else
                    registryKey = Microsoft.Win32.Registry.CurrentUser.OpenSubKey("Software\\" + companyName + "\\" + productName);
#endif
                // Parse the registry if the specified registryKey exists
                if (registryKey != null)
                {
                    // Get an array of what keys (registry value names) are stored
                    string[] valueNames = registryKey.GetValueNames();

                    // Create the array of the right size to take the saved PlayerPrefs
                    PlayerPrefPair[] tempPlayerPrefs = new PlayerPrefPair[valueNames.Length];

                    // Parse and convert the registry saved PlayerPrefs into our array
                    int i = 0;
                    foreach (string valueName in valueNames)
                    {
                        string key = valueName;

                        // Remove the _h193410979 style suffix used on PlayerPref keys in Windows registry
                        int index = key.LastIndexOf("_");
                        key = key.Remove(index, key.Length - index);

                        // Get the value from the registry
                        object ambiguousValue = registryKey.GetValue(valueName);

                        // Unfortunately floats will come back as an int (at least on 64 bit) because the float is stored as
                        // 64 bit but marked as 32 bit - which confuses the GetValue() method greatly!
                        if (ambiguousValue.GetType() == typeof(int))
                        {
                            // If the PlayerPref is not actually an int then it must be a float, this will evaluate to true
                            // (impossible for it to be 0 and -1 at the same time)
                            if (GetInt(key, -1) == -1 && GetInt(key, 0) == 0)
                            {
                                // Fetch the float value from PlayerPrefs in memory
                                ambiguousValue = GetFloat(key);
                            }
                        }
                        else if (ambiguousValue.GetType() == typeof(byte[]))
                        {
                            // On Unity 5 a string may be stored as binary, so convert it back to a string
                            ambiguousValue = encoding.GetString((byte[])ambiguousValue).TrimEnd('\0');
                        }
                        // Assign the key and value into the respective record in our output array
                        tempPlayerPrefs[i] = new PlayerPrefPair() { Key = key, Value = ambiguousValue };
                        i++;
                    }
                    // Return the results
                    return tempPlayerPrefs;
                }
                else
                {
                    // No existing PlayerPrefs saved (which is valid), so just return an empty array
                    return new PlayerPrefPair[0];
                }
            }
            else
            {
                throw new NotSupportedException("PlayerPrefsEditor doesn't support this Unity Editor platform");
            }
        }

        void DrawTopBar()
        {
            int newIndex = GUILayout.Toolbar(0, new string[] { "PlayerPrefs" });
        }

        void DrawMainList()
        {
            // The bold table headings
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Key", EditorStyles.boldLabel);
            GUILayout.Label("Value", EditorStyles.boldLabel);
            GUILayout.Label("Type", EditorStyles.boldLabel, GUILayout.Width(37));
            EditorGUILayout.EndHorizontal();
            // Create a GUIStyle that can be manipulated for the various text fields
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            // Could be dealing with either the full list or search results, so get the right list
            List<PlayerPrefPair> activePlayerPrefs = deserializedPlayerPrefs;

            // Cache the entry count
            int entryCount = activePlayerPrefs.Count;
            // Record the last scroll position so we can calculate if the user has scrolled this frame
            lastScrollPosition = scrollPosition;
            // Start the scrollable area
            scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);
            // Ensure the scroll doesn't go below zero
            if (scrollPosition.y < 0)
            {
                scrollPosition.y = 0;
            }
            // The following code has been optimised so that rather than attempting to draw UI for every single PlayerPref
            // it instead only draws the UI for those currently visible in the scroll view and pads above and below those
            // results to maintain the right size using GUILayout.Space(). This enables us to work with thousands of
            // PlayerPrefs without slowing the interface to a halt.
            // Fixed height of one of the rows in the table
            float rowHeight = 18;
            // Determine how many rows are visible on screen. For simplicity, use Screen.height (the overhead is negligible)
            int visibleCount = Mathf.CeilToInt(Screen.height / rowHeight);
            // Determine the index of the first PlayerPref that should be drawn as visible in the scrollable area
            int firstShownIndex = Mathf.FloorToInt(scrollPosition.y / rowHeight);
            // Determine the bottom limit of the visible PlayerPrefs (last shown index + 1)
            int shownIndexLimit = firstShownIndex + visibleCount;
            // If the actual number of PlayerPrefs is smaller than the caculated limit, reduce the limit to match
            if (entryCount < shownIndexLimit)
            {
                shownIndexLimit = entryCount;
            }
            // If the number of displayed PlayerPrefs is smaller than the number we can display (like we're at the end
            // of the list) then move the starting index back to adjust
            if (shownIndexLimit - firstShownIndex < visibleCount)
            {
                firstShownIndex -= visibleCount - (shownIndexLimit - firstShownIndex);
            }
            // Can't have a negative index of a first shown PlayerPref, so clamp to 0
            if (firstShownIndex < 0)
            {
                firstShownIndex = 0;
            }
            // Pad above the on screen results so that we're not wasting draw calls on invisible UI and the drawn player
            // prefs end up in the same place in the list
            GUILayout.Space(firstShownIndex * rowHeight);
            // For each of the on screen results
            for (int i = firstShownIndex; i < shownIndexLimit; i++)
            {
                // Normal PlayerPrefs are just black
                textFieldStyle.normal.textColor = GUI.skin.textField.normal.textColor;
                textFieldStyle.focused.textColor = GUI.skin.textField.focused.textColor;
                // The full key is the key that's actually stored in PlayerPrefs
                string fullKey = activePlayerPrefs[i].Key;
                // Display key is used so in the case of encrypted keys, we display the decrypted version instead (in
                // auto-decrypt mode).
                string displayKey = fullKey;
                // Used for accessing the type information stored against the PlayerPref
                object deserializedValue = activePlayerPrefs[i].Value;
                // Track whether the auto decrypt failed, so we can instead fallback to encrypted values and mark it red
                bool failedAutoDecrypt = false;
                EditorGUILayout.BeginHorizontal();
                // The type of PlayerPref being stored (in auto decrypt mode this works with the decrypted values too)
                Type valueType;
                // Otherwise fallback to the type of the cached value (for non-encrypted values this will be
                // correct). For encrypted values when not in auto-decrypt mode, this will return string type
                valueType = deserializedValue.GetType();
                // Display the PlayerPref key
                EditorGUILayout.TextField(displayKey, textFieldStyle);
                EditorGUI.EndDisabledGroup();
                // Value display and user editing
                // If we're dealing with a float
                if (valueType == typeof(float))
                {
                    float initialValue;
                    // Otherwise fetch the latest plain value from PlayerPrefs in memory
                    initialValue = GetFloat(fullKey);
                    // Display the float editor field and get any changes in value
                    float newValue = EditorGUILayout.FloatField(initialValue, textFieldStyle);
                    EditorGUI.EndDisabledGroup();
                    // If the value has changed
                    if (newValue != initialValue)
                    {
                        SetFloat(fullKey, newValue);
                        // Save PlayerPrefs
                        Save();
                    }
                    // Display the PlayerPref type
                    GUILayout.Label("float", GUILayout.Width(37));
                }
                else if (valueType == typeof(int)) // if we're dealing with an int
                {
                    int initialValue;
                    // Otherwise fetch the latest plain value from PlayerPrefs in memory
                    initialValue = GetInt(fullKey);
                    // Display the int editor field and get any changes in value
                    int newValue = EditorGUILayout.IntField(initialValue, textFieldStyle);
                    EditorGUI.EndDisabledGroup();
                    // If the value has changed
                    if (newValue != initialValue)
                    {
                        // Store the changed value in PlayerPrefs, encrypting if necessary
                        SetInt(fullKey, newValue);
                        // Save PlayerPrefs
                        Save();
                    }
                    // Display the PlayerPref type
                    GUILayout.Label("int", GUILayout.Width(37));
                }
                else if (valueType == typeof(bool)) // if we're dealing with a bool
                {
                    bool initialValue;
                    // Otherwise fetch the latest plain value from PlayerPrefs in memory
                    initialValue = GetBool(fullKey);
                    // Display the bool toggle editor field and get any changes in value
                    bool newValue = EditorGUILayout.Toggle(initialValue);
                    EditorGUI.EndDisabledGroup();
                    // If the value has changed
                    if (newValue != initialValue)
                    {
                        SetBool(fullKey, newValue);
                        // Save PlayerPrefs
                        Save();
                    }
                    // Display the PlayerPref type
                    GUILayout.Label("bool", GUILayout.Width(37));
                }
                else if (valueType == typeof(string)) // if we're dealing with a string
                {
                    string initialValue;

                    // Otherwise fetch the latest plain value from PlayerPrefs in memory
                    initialValue = GetString(fullKey);
                    // Display the text (string) editor field and get any changes in value
                    string newValue = EditorGUILayout.TextField(initialValue, textFieldStyle);
                    EditorGUI.EndDisabledGroup();
                    // If the value has changed
                    if (newValue != initialValue && !failedAutoDecrypt)
                    {
                        // Store the changed value in PlayerPrefs, encrypting if necessary
                        SetString(fullKey, newValue);
                        // Save PlayerPrefs
                        Save();
                    }
                    // Display the PlayerPref type
                    GUILayout.Label("string", GUILayout.Width(37));
                }
                // Delete button
                if (GUILayout.Button("X", GUILayout.Width(25)))
                {
                    // Delete the key from PlayerPrefs
                    DeleteKey(fullKey);
                    // Tell Unity to Save PlayerPrefs
                    Save();
                    // Delete the cached record so the list updates immediately
                    DeleteCachedRecord(fullKey);
                }
                EditorGUILayout.EndHorizontal();
            }
            // Calculate the padding at the bottom of the scroll view (because only visible PlayerPref rows are drawn)
            float bottomPadding = (entryCount - shownIndexLimit) * rowHeight;

            // If the padding is positive, pad the bottom so that the layout and scroll view size is correct still
            if (bottomPadding > 0)
            {
                GUILayout.Space(bottomPadding);
            }
            EditorGUILayout.EndScrollView();
            // Display the number of PlayerPrefs
            GUILayout.Label("Entry Count: " + entryCount);
            Rect rect = GUILayoutUtility.GetLastRect();
            rect.height = 1;
            rect.y -= 4;
            EditorGUI.DrawRect(rect, new Color(0.5f, 0.5f, 0.5f, 0.5f));
        }

        private void DrawAddEntry()
        {
            // Create a GUIStyle that can be manipulated for the various text fields
            GUIStyle textFieldStyle = new GUIStyle(GUI.skin.textField);
            // Create a space
            EditorGUILayout.Space();
            // Heading
            GUILayout.Label("Add PlayerPref", EditorStyles.boldLabel);
            // UI for whether the new PlayerPref is encrypted and what type it is
            EditorGUILayout.BeginHorizontal();
            if (newEntryType == PlayerPrefType.Bool)
                newEntryType = PlayerPrefType.String;
            newEntryType = (PlayerPrefType)GUILayout.Toolbar((int)newEntryType, new string[] { "float", "int", "string" });
            EditorGUILayout.EndHorizontal();
            // Key and Value headings
            EditorGUILayout.BeginHorizontal();
            GUILayout.Label("Key", EditorStyles.boldLabel);
            GUILayout.Label("Value", EditorStyles.boldLabel);
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            // Track the next control so we can detect key events in it
            GUI.SetNextControlName("newEntryKey");
            // UI for the new key text box
            newEntryKey = EditorGUILayout.TextField(newEntryKey, textFieldStyle);

            // Track the next control so we can detect key events in it
            GUI.SetNextControlName("newEntryValue");

            // Display the correct UI field editor based on what type of PlayerPref is being created
            if (newEntryType == PlayerPrefType.Float)
            {
                newEntryValueFloat = EditorGUILayout.FloatField(newEntryValueFloat, textFieldStyle);
            }
            else if (newEntryType == PlayerPrefType.Int)
            {
                newEntryValueInt = EditorGUILayout.IntField(newEntryValueInt, textFieldStyle);
            }
            else if (newEntryType == PlayerPrefType.Bool)
            {
                newEntryValueBool = EditorGUILayout.Toggle(newEntryValueBool);
            }
            else
            {
                newEntryValueString = EditorGUILayout.TextField(newEntryValueString, textFieldStyle);
            }
            // If the user hit enter while either the key or value fields were being edited
            bool keyboardAddPressed = Event.current.isKey && Event.current.keyCode == KeyCode.Return && Event.current.type == EventType.KeyUp && (GUI.GetNameOfFocusedControl() == "newEntryKey" || GUI.GetNameOfFocusedControl() == "newEntryValue");
            // If the user clicks the Add button or hits return (and there is a non-empty key), create the PlayerPref
            if ((GUILayout.Button("Add", GUILayout.Width(40)) || keyboardAddPressed) && !string.IsNullOrEmpty(newEntryKey))
            {
                if (newEntryType == PlayerPrefType.Float)
                {
                    // Record the new PlayerPref in PlayerPrefs
                    SetFloat(newEntryKey, newEntryValueFloat);
                    // Cache the addition
                    CacheRecord(newEntryKey, newEntryValueFloat);
                }
                else if (newEntryType == PlayerPrefType.Int)
                {
                    // Record the new PlayerPref in PlayerPrefs
                    SetInt(newEntryKey, newEntryValueInt);
                    // Cache the addition
                    CacheRecord(newEntryKey, newEntryValueInt);
                }
                else if (newEntryType == PlayerPrefType.Bool)
                {
                    // Record the new PlayerPref in PlayerPrefs
                    SetBool(newEntryKey, newEntryValueBool);
                    // Cache the addition
                    CacheRecord(newEntryKey, newEntryValueBool);
                }
                else
                {
                    // Record the new PlayerPref in PlayerPrefs
                    SetString(newEntryKey, newEntryValueString);
                    // Cache the addition
                    CacheRecord(newEntryKey, newEntryValueString);
                }
                // Tell Unity to save the PlayerPrefs
                Save();
                // Force a repaint since hitting the return key won't invalidate layout on its own
                Repaint();
                // Reset the values
                newEntryKey = "";
                newEntryValueFloat = 0;
                newEntryValueInt = 0;
                newEntryValueString = "";
                // Deselect
                GUI.FocusControl("");
            }
            EditorGUILayout.EndHorizontal();
        }

        private void DrawBottomMenu()
        {
            EditorGUILayout.Space();
            EditorGUILayout.BeginHorizontal();
            float buttonWidth = (EditorGUIUtility.currentViewWidth - 10) / 2f;
            // Delete all PlayerPrefs
            if (GUILayout.Button("Delete All Preferences", GUILayout.Width(buttonWidth)))
            {
                if (EditorUtility.DisplayDialog("Delete All?", "Are you sure you want to delete all preferences?", "Delete All", "Cancel"))
                {
                    DeleteAll();
                    Save();
                    // Clear the cache too, for an instant visibility update for OSX
                    deserializedPlayerPrefs.Clear();
                }
            }
            GUILayout.FlexibleSpace();
            // Mainly needed for OSX, this will encourage PlayerPrefs to save to file (but still may take a few seconds)
            if (GUILayout.Button("Force Save", GUILayout.Width(buttonWidth)))
            {
                Save();
            }
            EditorGUILayout.EndHorizontal();
        }

        private void OnGUI()
        {
            EditorGUILayout.Space();
            DrawTopBar();
            if (Application.platform == RuntimePlatform.OSXEditor)
            {
                string playerPrefsPath;
                // From Unity Docs: On Mac OS X PlayerPrefs are stored in ~/Library/Preferences folder, in a file named unity.[company name].[product name].plist, where company and product names are the names set up in Project Settings. The same .plist file is used for both Projects run in the Editor and standalone players.
                // Construct the plist filename from the project's settings
                string plistFilename = string.Format("unity.{0}.{1}.plist", PlayerSettings.companyName, PlayerSettings.productName);
                // Now construct the fully qualified path
                playerPrefsPath = Path.Combine(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), "Library/Preferences"), plistFilename);
                // Determine when the plist was last written to
                DateTime lastWriteTime = File.GetLastWriteTimeUtc(playerPrefsPath);
                // If we haven't deserialized the PlayerPrefs already, or the written file has changed then deserialize
                // the latest version
                if (!lastDeserialization.HasValue || lastDeserialization.Value != lastWriteTime)
                {
                    // Deserialize the actual PlayerPrefs from file into a cache
                    deserializedPlayerPrefs = new List<PlayerPrefPair>(RetrieveSavedPrefs(PlayerSettings.companyName, PlayerSettings.productName));

                    // Record the version of the file we just read, so we know if it changes in the future
                    lastDeserialization = lastWriteTime;
                }
                if (lastWriteTime != MISSING_DATETIME)
                {
                    GUILayout.Label("Plist Last Written: " + lastWriteTime.ToString());
                }
                else
                {
                    GUILayout.Label("Plist Does Not Exist");
                }
            }
            else if (Application.platform == RuntimePlatform.WindowsEditor)
            {
                // Windows works a bit differently to OSX, we just regularly query the registry. So don't query too often
                if (!lastDeserialization.HasValue || DateTime.UtcNow - lastDeserialization.Value > TimeSpan.FromMilliseconds(500))
                {
                    // Deserialize the actual PlayerPrefs from registry into a cache
                    deserializedPlayerPrefs = new List<PlayerPrefPair>(RetrieveSavedPrefs(PlayerSettings.companyName, PlayerSettings.productName));

                    // Record the latest time, so we don't fetch again too quickly
                    lastDeserialization = DateTime.UtcNow;
                }
            }
            DrawMainList();
            DrawAddEntry();
            DrawBottomMenu();
            // If the user has scrolled, deselect - this is because control IDs within carousel will change when scrolled
            // so we'd end up with the wrong box selected.
            if (scrollPosition != lastScrollPosition)
            {
                // Deselect
                GUI.FocusControl("");
            }
        }
        private void CacheRecord(string key, object value)
        {
            // First of all check if this key already exists, if so replace it's value with the new value
            bool replaced = false;
            int entryCount = deserializedPlayerPrefs.Count;
            for (int i = 0; i < entryCount; i++)
            {
                // Found the key - it exists already
                if (deserializedPlayerPrefs[i].Key == key)
                {
                    // Update the cached pref with the new value
                    deserializedPlayerPrefs[i] = new PlayerPrefPair() { Key = key, Value = value };
                    // Mark the replacement so we no longer need to add it
                    replaced = true;
                    break;
                }
            }
            // PlayerPref doesn't already exist (and wasn't replaced) so add it as new
            if (!replaced)
            {
                // Cache a PlayerPref the user just created so it can be instantly display (mainly for OSX)
                deserializedPlayerPrefs.Add(new PlayerPrefPair() { Key = key, Value = value });
            }
        }
        private void DeleteCachedRecord(string fullKey)
        {
            keyQueuedForDeletion = fullKey;
        }
        // OnInspectorUpdate() is called by Unity at 10 times a second
        private void OnInspectorUpdate()
        {
            // If a PlayerPref has been specified for deletion
            if (!string.IsNullOrEmpty(keyQueuedForDeletion))
            {
                // If the user just deleted a PlayerPref, find the ID and defer it for deletion by OnInspectorUpdate()
                if (deserializedPlayerPrefs != null)
                {
                    int entryCount = deserializedPlayerPrefs.Count;
                    for (int i = 0; i < entryCount; i++)
                    {
                        if (deserializedPlayerPrefs[i].Key == keyQueuedForDeletion)
                        {
                            deserializedPlayerPrefs.RemoveAt(i);
                            break;
                        }
                    }
                }
                // Remove the queued key since we've just deleted it
                keyQueuedForDeletion = null;
                Repaint();
            }
            else if (inspectorUpdateFrame % 10 == 0) // Once a second (every 10th frame)
            {
                // Force the window to repaint
                Repaint();
            }
            // Track what frame we're on, so we can call code less often
            inspectorUpdateFrame++;
        }
    }
}