using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class SnappingWindow : EditorWindow
{
    Toggle usePolarToggle;
    FloatField gridSizeField;
    FloatField angularSizeField;
    FloatField radialSizeField;
    VisualElement cartesianGroup;
    VisualElement polarGroup;
    Button snapBtn;

    SerializedObject snapWindowSO;
    [SerializeField]
    float gridcSize = 1;
    [SerializeField]
    float gridAngularSize = 1;
    [SerializeField]
    float gridRadialSize = 1;
    [SerializeField]
    bool usePolar = false;

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
        snapWindowSO = new SerializedObject(this);
        var gridSizeProperty = snapWindowSO.FindProperty("gridcSize");
        var gridAngSizeProperty = snapWindowSO.FindProperty("gridAngularSize");
        var gridRadialSizeProperty = snapWindowSO.FindProperty("gridRadialSize");
        var usePolarProperty = snapWindowSO.FindProperty("usePolar");

        usePolarToggle = new Toggle("Use Polar coordinates");
        usePolarToggle.BindProperty(usePolarProperty);
        usePolarToggle.RegisterValueChangedCallback(b => PolarChange(b.newValue));
        root.Add(usePolarToggle);

        // cartesian
        cartesianGroup = new VisualElement();
        root.Add(cartesianGroup);

        gridSizeField = new FloatField("Grid Size");
        gridSizeField.BindProperty(gridSizeProperty);
        gridSizeField.RegisterValueChangedCallback(f => SceneView.RepaintAll());
        cartesianGroup.Add(gridSizeField);

        // polar
        polarGroup = new VisualElement();
        root.Add(polarGroup);

        angularSizeField = new FloatField("Angular Size");
        angularSizeField.BindProperty(gridAngSizeProperty);
        angularSizeField.RegisterValueChangedCallback(f => SceneView.RepaintAll());
        polarGroup.Add(angularSizeField);

        radialSizeField = new FloatField("Radial Size");
        radialSizeField.BindProperty(gridRadialSizeProperty);
        radialSizeField.RegisterValueChangedCallback(f => SceneView.RepaintAll());
        polarGroup.Add(radialSizeField);


        snapBtn = new Button(SnapSelection);
        snapBtn.text = "Snap Selected Objects";
        snapBtn.tooltip = "Snaps selected objects to nearest grid point";

        root.Add(snapBtn);
        PolarChange(usePolar);

        Selection.selectionChanged += SelectionChanged;
        SceneView.duringSceneGui += SceneGUI;
    }
    private void OnDisable()
    {
        // lastGridRadialSize = gridSizeField.value;

        // gridSizeField.UnregisterValueChangedCallback();
        Selection.selectionChanged -= SelectionChanged;
        SceneView.duringSceneGui -= SceneGUI;
    }
    void PolarChange(bool enabled)
    {
        polarGroup.SetEnabled(enabled);
        cartesianGroup.SetEnabled(!enabled);
        SceneView.RepaintAll();
    }
    void SelectionChanged()
    {
        snapBtn.SetEnabled(Selection.count > 0);
    }

    void SnapSelection()
    {
        if (!usePolar)
        {
            if (gridSizeField.value <= 0)
            {
                Debug.LogWarning("Snapping size cannot be 0 or less!");
                return;
            }
            // Debug.Log("Snapping!");
            var selectedTransforms = Selection.transforms;
            // var so = new SerializedObject(selectedTransforms);
            Undo.RecordObjects(selectedTransforms, "Snap positions");
            foreach (var st in selectedTransforms)
            {
                // so.FindProperty("m_LocalPosition")
                st.position = SnapAxisCartesian(st.position);
            }
            // so.ApplyModifiedProperties();
        } else
        {
            // polar
            if (angularSizeField.value <= 0 || radialSizeField.value <= 0)
            {
                Debug.LogWarning("radial Snapping size cannot be 0 or less!");
                return;
            }
            // Debug.Log("Snapping!");
            var selectedTransforms = Selection.transforms;
            Undo.RecordObjects(selectedTransforms, "Snap positions polar");
            foreach (var st in selectedTransforms)
            {
                st.position = SnapAxisPolar(st.position);
            }
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
    Vector3 SnapAxisPolar(Vector3 a)
    {
        Vector3 snapped = a;
        float angularSize = angularSizeField.value;
        float radialSize = radialSizeField.value;
        // snapped.x = Mathf.Round(a.x / angularSize) * angularSize;
        // snapped.y = Mathf.Round(a.y / angularSize) * angularSize;
        // snapped.z = Mathf.Round(a.z / angularSize) * angularSize;
        return snapped;
    }

    void SceneGUI(SceneView sceneView)
    {
        if (!usePolar)
        {
            if (gridSizeField.value <= 0)
            {
                return;
            }
            var selectedTransforms = Selection.transforms;
            int numLines = 3;//Mathf.RoundToInt(1 / gridSizeField.value) + 1;
            float distBetweenLines = gridSizeField.value;//(rangeDist) / numLines * 2;
            float rangeDist = (numLines - 1) * distBetweenLines;
            bool showYAxis = false;
            foreach (var st in selectedTransforms)
            {
                Vector3 center = SnapAxisCartesian(st.position);
                // Handles.DrawingScope
                Handles.DrawLine(center, st.position);
                Handles.DrawSolidDisc(center, Vector3.up, 0.1f * distBetweenLines);
                // Gizmos.color = Color.white;
                // Gizmos.DrawSphere(center, 0.1f);
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
        } else
        {
            // use polar coordinates

            var selectedTransforms = Selection.transforms;
            int numLines = 4;
            bool showYAxis = false;
            foreach (var st in selectedTransforms)
            {

            }
        }
    }

}