using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class SnappingWindow : EditorWindow
{
    Button snapBtn;
    FloatField gridSizeField;
    FloatField angularSizeField;
    FloatField raidalSizeField;

    [SerializeField]
    float lastGridcSize = 1;
    float lastGridAngularSize = 1;
    float lastGridRadialSize = 1;

    [MenuItem("ToolTest/Snapper")]
    private static void ShowWindow()
    {
        var window = GetWindow<SnappingWindow>();
        window.titleContent = new GUIContent("Snapper");
        window.Show();
    }

    private void OnEnable()
    {
        var root = rootVisualElement;

        snapBtn = new Button(SnapSelection);
        snapBtn.text = "Snap Selected Objects";
        snapBtn.tooltip = "Snaps selected objects to nearest grid point";
        root.Add(snapBtn);

        var cGridSettingsGroup = new VisualElement();
        root.Add(cGridSettingsGroup);
        gridSizeField = new FloatField("Grid Size");
        gridSizeField.value = lastGridRadialSize;
        gridSizeField.RegisterValueChangedCallback(f => SceneView.RepaintAll());
        cGridSettingsGroup.Add(gridSizeField);

        var pGridSettingsGroup = new VisualElement();
        root.Add(pGridSettingsGroup);

        Selection.selectionChanged += SelectionChanged;
        SceneView.duringSceneGui += SceneGUI;
    }
    private void OnDisable()
    {
        lastGridRadialSize = gridSizeField.value;

        // gridSizeField.UnregisterValueChangedCallback();
        Selection.selectionChanged -= SelectionChanged;
        SceneView.duringSceneGui -= SceneGUI;
    }

    void SelectionChanged()
    {
        snapBtn.SetEnabled(Selection.count > 0);
    }

    void SnapSelection()
    {
        if (gridSizeField.value <= 0)
        {
            Debug.LogWarning("Snapping size cannot be 0 or less!");
            return;
        }
        // Debug.Log("Snapping!");
        var selectedTransforms = Selection.transforms;
        Undo.RecordObjects(selectedTransforms, "Snap to Grid");
        foreach (var st in selectedTransforms)
        {
            st.position = SnapAxisCartesian(st.position);
        }
    }
    Vector3 SnapAxisCartesian(Vector3 a)
    {
        Vector3 snapped = a;
        float snapSize = gridSizeField.value;
        snapped.x = Mathf.Round(a.x / snapSize) * snapSize;
        snapped.y = Mathf.Round(a.y / snapSize) * snapSize;
        snapped.z = Mathf.Round(a.z / snapSize) * snapSize;
        return snapped;
    }

    void SceneGUI(SceneView sceneView)
    {
        if (gridSizeField.value <= 0)
        {
            return;
        }
        var selectedTransforms = Selection.transforms;
        int numLines = 3;//Mathf.RoundToInt(1 / gridSizeField.value) + 1;
        float distBetweenLines = gridSizeField.value;//(rangeDist) / numLines * 2;
        float rangeDist = (numLines-1) * distBetweenLines;
        bool showYAxis = false;
        foreach (var st in selectedTransforms)
        {
            Vector3 center = SnapAxisCartesian(st.position);
            for (int axis = 0; axis < 3; axis++)
            {
                Vector3 lineDir = axis == 0 ? Vector3.forward : axis == 1 ? Vector3.right : Vector3.up;
                Vector3 sideDir = axis == 0 ? Vector3.up : axis == 1 ? Vector3.up : Vector3.right;
                Vector3 sideDir2 = axis == 0 ? Vector3.right : axis == 1 ? Vector3.forward : Vector3.forward;
                if (!showYAxis && (lineDir.y == 1))
                {
                    break;
                }

                for (int a1 = 0; a1 < numLines; a1++)
                {
                    Vector3 a1Offset = sideDir * (a1 - numLines / 2) * distBetweenLines;
                    if (!showYAxis)
                    {
                        a1Offset = Vector3.zero;
                    }
                    for (int a2 = 0; a2 < numLines; a2++)
                    {
                        Vector3 a2Offset = sideDir2 * (a2 - numLines / 2) * distBetweenLines;
                        Vector3 offset = a1Offset + a2Offset;
                        Handles.DrawLine(center - lineDir * rangeDist + offset, center + lineDir * rangeDist + offset);
                    }
                    if (!showYAxis)
                    {
                        break;
                    }
                }
            }
        }
    }

}