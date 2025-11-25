#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(GroqLLMUISetup))]
public class GroqLLMUISetupEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        
        GroqLLMUISetup setupScript = (GroqLLMUISetup)target;
        
        GUILayout.Space(20);
        
        // Create a big, obvious button
        GUI.backgroundColor = Color.green;
        GUIStyle buttonStyle = new GUIStyle(GUI.skin.button);
        buttonStyle.fontSize = 14;
        buttonStyle.fontStyle = FontStyle.Bold;
        buttonStyle.fixedHeight = 40;
        
        if (GUILayout.Button("🎨 SETUP UI AUTOMATICALLY 🎨", buttonStyle))
        {
            if (EditorUtility.DisplayDialog(
                "Setup Groq LLM UI",
                "This will automatically create a complete chat interface with:\n\n" +
                "• Canvas and Panel\n" +
                "• Input Field (top)\n" +
                "• Scrollable Output (bottom)\n" +
                "• Send Button\n" +
                "• All components linked\n\n" +
                "Continue?",
                "Yes, Setup UI",
                "Cancel"))
            {
                setupScript.SetupUI();
                EditorUtility.DisplayDialog(
                    "Setup Complete!",
                    "UI has been created successfully!\n\n" +
                    "You can now:\n" +
                    "1. Remove this GroqLLMUISetup component\n" +
                    "2. Press Play to test the chat\n" +
                    "3. Customize the UI as needed",
                    "OK");
            }
        }
        
        GUI.backgroundColor = Color.white;
        
        GUILayout.Space(10);
        
        EditorGUILayout.HelpBox(
            "INSTRUCTIONS:\n\n" +
            "1. Click the green button above\n" +
            "2. Confirm the setup\n" +
            "3. UI will be generated automatically\n" +
            "4. Remove this component when done\n" +
            "5. Press Play to start chatting!",
            MessageType.Info);
    }
}
#endif
