using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class PropCannon : EditorWindow
{

    public float brushRadius = 10;
    public int spawnAmountMin = 5;
    // public int spawnAmountMax = 8;

    [MenuItem("ToolTest/PropCannon")]
    private static void ShowWindow()
    {
        var window = GetWindow<PropCannon>();
        window.titleContent = new GUIContent("Prop Cannon");
    }
    private void OnEnable()
    {
        LoadEditorData();

        // editor window 
        var root = rootVisualElement;

        var radField = new FloatField("Brush Radius");
        radField.bindingPath = "brushRadius";
        radField.RegisterValueChangedCallback(f => SceneView.RepaintAll());
        root.Add(radField);
        var minSpawnField = new FloatField("spawnAmountMin");
        minSpawnField.bindingPath = "spawnAmountMin";
        minSpawnField.RegisterValueChangedCallback(f => SceneView.RepaintAll());
        root.Add(minSpawnField);




        var so = new SerializedObject(this);
        root.Bind(so);

        // events
        SceneView.duringSceneGui += SceneGUI;
    }
    private void OnDisable()
    {
        SaveEditorData();

        SceneView.duringSceneGui -= SceneGUI;
    }
    void SaveEditorData()
    {

    }
    void LoadEditorData()
    {

    }
    void SceneGUI(SceneView sceneView)
    {
        if (Event.current.type == EventType.Repaint)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Transform cam = sceneView.camera.transform;

            Ray camRay = new Ray(cam.position, cam.forward);
            // Ray camRay = sceneView.camera.ScreenPointToRay(Event.current.mousePosition);
            Handles.color = Color.black;
            if (Physics.Raycast(camRay, out var hit))
            {
                Handles.DrawAAPolyLine(4, hit.point, hit.point + hit.normal);
                Handles.DrawWireDisc(hit.point, hit.normal, brushRadius);
            }
            Handles.color = Color.white;
        }
    }
    void GenerateRandomPoints()
    {
        int numPoints = spawnAmountMin;
        // random positions in brush
        Vector3[] positions = new Vector3[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            positions[i] = Random.insideUnitCircle * brushRadius;
        }
        // snap to terrain
        for (int i = 0; i < numPoints; i++)
        {
            var start = positions[i];
            // if (Physics.Raycast(start, start))
        }
    }
}