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
        Load();

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
        Save();
        // lastGridRadialSize = gridSizeField.value;

        // gridSizeField.UnregisterValueChangedCallback();
        Selection.selectionChanged -= SelectionChanged;
        SceneView.duringSceneGui -= SceneGUI;
    }
    void Save()
    {
        EditorPrefs.SetFloat("SNAPTOOL_gridcSize", gridcSize);
        EditorPrefs.SetFloat("SNAPTOOL_gridAngularSize", gridAngularSize);
        EditorPrefs.SetFloat("SNAPTOOL_gridRadialSize", gridRadialSize);
        EditorPrefs.SetBool("SNAPTOOL_usePolar", usePolar);
    }
    void Load()
    {
        gridcSize = EditorPrefs.GetFloat("SNAPTOOL_gridcSize", gridcSize);
        gridAngularSize = EditorPrefs.GetFloat("SNAPTOOL_gridAngularSize", gridAngularSize);
        gridRadialSize = EditorPrefs.GetFloat("SNAPTOOL_gridRadialSize", gridRadialSize);
        usePolar = EditorPrefs.GetBool("SNAPTOOL_usePolar", usePolar);
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
    Vector3 polarToVec3(float angle, float radius = 1)
    {
        return new Vector3(Mathf.Cos(angle * Mathf.Deg2Rad), 0, Mathf.Sin(angle * Mathf.Deg2Rad)) * radius;
    }
    void Vec3ToPolar(Vector3 a, out float radius, out float angle)
    {
        Vector3 b = a;
        b.Normalize();
        angle = Mathf.Atan2(b.z, b.x) * Mathf.Rad2Deg;
        radius = new Vector2(a.x, a.z).magnitude;
    }
    void GetPolarSnapped(Vector3 a, out float radius, out float angle)
    {
        Vector3 b = a;
        b.Normalize();
        float ang = Mathf.Atan2(b.z, b.x) * Mathf.Rad2Deg;
        float snapAng = Mathf.Round(ang / gridAngularSize) * gridAngularSize;
        float magn = new Vector2(a.x, a.z).magnitude;
        float snappedMag = Mathf.Round(magn / gridRadialSize) * gridRadialSize;
        angle = snapAng;
        radius = snappedMag;
    }
    Vector3 SnapAxisPolar(Vector3 a)
    {
        Vector3 snapped = a;
        snapped.y = a.y;

        GetPolarSnapped(a, out var snapRadius, out var snapAngle);
        float angX = snapRadius * Mathf.Cos(snapAngle * Mathf.Deg2Rad);
        float angY = snapRadius * Mathf.Sin(snapAngle * Mathf.Deg2Rad);
        snapped.x = angX;
        snapped.z = angY;
        return snapped;
    }

    void SceneGUI(SceneView sceneView)
    {
        if (Event.current.type != EventType.Repaint)
            return;

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
            // todo support any plane, not just xz
            // todo support any transform as polar center, not just the origin
            // todo snap in scene view while holding ctrl?
            // todo snap rotation as well?
            if (radialSizeField.value <= 0)
            {
                return;
            }
            var selectedTransforms = Selection.transforms;
            int numLines = 3;
            float distBetweenLines = gridRadialSize;//(rangeDist) / numLines * 2;
            float rangeDist = distBetweenLines * (numLines - 1);
            bool showYAxis = false;
            Vector3 polarCenter = Vector3.zero;
            foreach (var st in selectedTransforms)
            {
                polarCenter.y = st.position.y;
                Vector3 center = SnapAxisPolar(st.position);
                center.y = polarCenter.y;
                Handles.DrawLine(center, st.position);
                Handles.DrawSolidDisc(center, Vector3.up, gridRadialSize * 0.05f);
                Vec3ToPolar(center, out var crad, out var cang);
                for (int i = 0; i < numLines; i++)
                {
                    float lineOffMult = i - numLines / 2;
                    if (center == Vector3.zero && i == 0)
                    {
                        continue;
                    }
                    float angOffset = lineOffMult * gridAngularSize;
                    GetPolarSnapped(st.position, out float snapRad, out float snapAng);

                    var lineCenter = SnapAxisPolar(polarToVec3(cang + angOffset, crad));
                    lineCenter.y = polarCenter.y;
                    var toCenter = lineCenter - polarCenter;
                    if (toCenter != Vector3.zero)
                    {
                        var toDir = toCenter.normalized;
                        Handles.DrawLine(lineCenter - toDir * rangeDist, lineCenter + toDir * rangeDist);
                    } else
                    {
                        lineCenter = SnapAxisPolar(polarToVec3(snapAng + angOffset, snapRad + gridRadialSize));
                        lineCenter.y = polarCenter.y;
                        toCenter = lineCenter - polarCenter;
                        if (toCenter != Vector3.zero)
                        {
                            var toDir = toCenter.normalized;
                            float nDist = gridRadialSize / 2;
                            Handles.DrawLine(lineCenter - toDir * (rangeDist + nDist), lineCenter + toDir * nDist);
                        }
                    }

                    float radOffset = lineOffMult * gridRadialSize;
                    float angDist = gridAngularSize * 2;
                    float startAng = (snapAng - angDist);
                    // Debug.Log(snapAng);
                    Vector3 startAnglePos = polarToVec3(startAng);

                    Handles.DrawWireArc(polarCenter, Vector3.up, startAnglePos, -angDist * 2, snapRad + radOffset);
                }
            }
        }
    }

}