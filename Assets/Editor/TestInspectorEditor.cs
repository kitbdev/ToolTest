using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;

[CustomEditor(typeof(TestInspector))]
public class TestInspectorEditor : Editor
{
    public override VisualElement CreateInspectorGUI()
    {
        // return base.CreateInspectorGUI();
        VisualElement root = new VisualElement();
        IMGUIContainer def = new IMGUIContainer(() => {
            // DrawDefaultInspector(); 
            // DrawFoldoutInspector(target, ref );
            DrawPropertiesExcluding(serializedObject, "myInt");
        });
        root.Add(def);
        Button btn = new Button();
        btn.text = "Push me!";
        btn.clicked += () => { Debug.Log("pushed!"); };
        root.Add(btn);
        return root;
    }
}