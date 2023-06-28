using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityToolbarExtender;

namespace UMANG
{
    [InitializeOnLoad]
    public class ToolBarExtention
    {
        static GUIContent CLEAR_PLAYERPREFS;
        static GUIContent SETTINGS;
        static GUIContent BUILD_AND_RUN;
        static GUIContent IMPORT_PACKAGE;
        static GUIContent UNPACK_PREFAB;
        static GUIContent SHOW_IN_EXPLORER;
        static GUIContent GAME_SCENE = new GUIContent()
        {
            text = "G",
            tooltip = "Open GameScene"
        };
        static GUIContent TEST_SCENE = new GUIContent()
        {
            text = "T",
            tooltip = "Open TestScene"
        };
        static ToolBarExtention()
        {
            CLEAR_PLAYERPREFS = new GUIContent()
            {
                image = Resources.Load<Texture2D>("SaveFromPlay"),
                tooltip = "Delete All PlayerPrefs"
            };
            SETTINGS = new GUIContent()
            {
                image = EditorGUIUtility.FindTexture("d_Settings"),
                tooltip = "Open Player Settings"
            };
            BUILD_AND_RUN = new GUIContent()
            {
                image = EditorGUIUtility.FindTexture("BuildSettings.Android"),
                tooltip = "Build the APK and run on connected Device"
            };
            IMPORT_PACKAGE = new GUIContent()
            {
                image = EditorGUIUtility.FindTexture("d_Package Manager"),
                tooltip = "Import Custom Package"
            };
            UNPACK_PREFAB = new GUIContent()
            {
                image = EditorGUIUtility.FindTexture("d_PrefabModel Icon"),
                tooltip = "Unpack Prefab Completely"
            };
            SHOW_IN_EXPLORER = new GUIContent()
            {
                image = EditorGUIUtility.FindTexture("d_FolderOpened Icon"),
                tooltip = "Show in Explorer"
            };

            ToolbarExtender.LeftToolbarGUI.Add(OnToolbarGUILeft);
            ToolbarExtender.RightToolbarGUI.Add(OnToolbarGUIRight);
        }

        static void OnToolbarGUILeft()
        {
            GUILayout.FlexibleSpace();
            GUI.enabled = !Application.isPlaying;
            if (GUILayout.Button(SHOW_IN_EXPLORER, ToolbarStyles.commandButtonStyle)) EditorApplication.ExecuteMenuItem("Assets/Show in Explorer");
            if (GUILayout.Button(UNPACK_PREFAB, ToolbarStyles.commandButtonStyle)) Unpack();
            if (GUILayout.Button(BUILD_AND_RUN, ToolbarStyles.commandButtonStyle)) EditorApplication.ExecuteMenuItem("File/Build And Run");
            if (GUILayout.Button(IMPORT_PACKAGE, ToolbarStyles.commandButtonStyle)) EditorApplication.ExecuteMenuItem("Assets/Import Package/Custom Package...");
            if (GUILayout.Button(SETTINGS, ToolbarStyles.commandButtonStyle)) SettingsService.OpenProjectSettings("Project/Player");
            if (GUILayout.Button(CLEAR_PLAYERPREFS, ToolbarStyles.commandButtonStyle)) PlayerPrefs.DeleteAll();
            GUI.enabled = true;
        }

        internal static void Unpack()
        {
            if (Selection.activeGameObject == null) return;
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RegisterCompleteObjectUndo(go, "Prefab Unpacked");
                PrefabUtility.UnpackPrefabInstance(go, PrefabUnpackMode.Completely, InteractionMode.UserAction);
            }
        }

        static void OnToolbarGUIRight()
        {
            GUI.enabled = !Application.isPlaying;

            string path = "Assets/@MyAssets/Scenes/GameScene.unity";
            if (System.IO.File.Exists(path))
            {
                if (GUILayout.Button(GAME_SCENE, ToolbarStyles.commandButtonStyle))
                {
                    EditorSceneManager.OpenScene(path);
                }
            }

            path = "Assets/Test.unity";
            if (System.IO.File.Exists(path))
            {
                if (GUILayout.Button(TEST_SCENE, ToolbarStyles.commandButtonStyle))
                {
                    EditorSceneManager.OpenScene(path);
                }
            }

            GUI.enabled = true;
            GUILayout.FlexibleSpace();

        }
    }
}