using UnityEngine;
using UnityEngine.UIElements;
using UnityEditor;
using System;

public class QuickTool : EditorWindow
{

    [MenuItem("ToolTest/QuickTool _%#T")]
    private static void ShowWindow()
    {
        // open or focus the window
        var window = GetWindow<QuickTool>();
        // set the title
        window.titleContent = new GUIContent("QuickTool");
        // set minimum size
        window.minSize = new Vector2(250, 50);

        // window.Show();
    }
    private void OnEnable()
    {
        var root = rootVisualElement;
        // manual way
        // var btn = new Button() { text = "my button" };
        // root.Add(btn);
        // btn.style.width = 160;
        // btn.style.height = 30;

        // style sheet
        root.styleSheets.Add(Resources.Load<StyleSheet>("QuickTool_Style"));
        // uxml
        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("QuickTool_Main");
        quickToolVisualTree.CloneTree(root);

        // setup buttons
        var toolButtons = root.Query<Button>();
        toolButtons.ForEach(SetupButton);
    }
    void SetupButton(Button button)
    {
        // set the icon
        var buttonIcon = button.Q(className: "quicktool-button-icon");
        var iconPath = "Icons/" + button.parent.name + "icon";
        var iconAsset = Resources.Load<Texture2D>(iconPath);
        buttonIcon.style.backgroundImage = iconAsset;
        // click event
        button.clickable.clicked += () => CreateObject(button.parent.name);
        // basic tooltip
        button.tooltip = button.parent.name;
    }
    private void CreateObject(string primitiveTypeName)
    {
        var pt = (PrimitiveType)Enum.Parse
                     (typeof(PrimitiveType), primitiveTypeName, true);
        var go = ObjectFactory.CreatePrimitive(pt);
        go.transform.position = Vector3.zero;
    }

}