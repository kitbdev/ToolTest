using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;

public class QuickTool : EditorWindow {

    [MenuItem("ToolTest/QuickTool _%#T")]
    private static void ShowWindow() {
        var window = GetWindow<QuickTool>();
        window.titleContent = new GUIContent("QuickTool");
        // window.minSize = new Vector2(250, 50);
        window.Show();
    }
    private void OnEnable() {
        var root = rootVisualElement;
        var btn = new Button() { text = "my button" };
        root.Add(btn);
        btn.style.width = 160;
        btn.style.height = 30;
    }

}