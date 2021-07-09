using UnityEngine;
using UnityEditor;
using UnityEngine.UIElements;
using UnityEditor.UIElements;

public class PropCannon : EditorWindow
{
    private const string startToggleText = "start placing props";
    private const string finishToggleText = "finish placing props";

    [Min(0)]
    public float brushRadius = 10;
    public int spawnAmountMin = 5;
    public int spawnAmountMax = 30;
    public LayerMask layerMask = Physics.DefaultRaycastLayers;
    // public int spawnAmountMax = 8;

    bool isPlacing = false;
    float upDist = 5f;
    float downDist = 5f;
    Vector2[] randPoints;
    float[] randRots;
    Matrix4x4[] placedTransforms;


    Label numPointsLabel;

    SerializedObject serializedObject;

    [MenuItem("ToolTest/PropCannon")]
    private static void ShowWindow()
    {
        var window = GetWindow<PropCannon>();
        window.titleContent = new GUIContent("Prop Cannon");
    }
    private void OnEnable()
    {
        LoadEditorData();

        // editor window ui
        var root = rootVisualElement;

        var radField = new FloatField("Brush Radius");
        radField.bindingPath = nameof(brushRadius);
        radField.RegisterValueChangedCallback(f => {
            // brushRadius = 0;// Mathf.Max(f.newValue, 0.01f);
            // var brprop = serializedObject.FindProperty(nameof(brushRadius));
            // brprop.floatValue = Mathf.Max(0.01f, brprop.floatValue);
            // serializedObject.ApplyModifiedProperties();
            SceneView.RepaintAll();
        });
        root.Add(radField);
        // root.Add(new InspectorElement());
        var minSpawnField = new IntegerField(nameof(spawnAmountMin));
        minSpawnField.bindingPath = nameof(spawnAmountMin);
        minSpawnField.RegisterValueChangedCallback(f => SceneView.RepaintAll());
        root.Add(minSpawnField);
        var maxSpawnField = new IntegerField(nameof(spawnAmountMax));
        maxSpawnField.bindingPath = nameof(spawnAmountMax);
        maxSpawnField.RegisterValueChangedCallback(f => { SceneView.RepaintAll(); GenerateRandomPoints(); });
        root.Add(maxSpawnField);
        var layerMaskField = new LayerMaskField(nameof(layerMask));
        layerMaskField.bindingPath = nameof(layerMask);
        layerMaskField.RegisterValueChangedCallback(f => { SceneView.RepaintAll(); GenerateRandomPoints(); });
        root.Add(layerMaskField);

        numPointsLabel = new Label("num points: 0");
        root.Add(numPointsLabel);
        // var numPointsLabelVal = new Label(randPoints.Length);
        // root.Add(numPointsLabelVal);
        var randbtn = new Button();
        randbtn.text = "new random points";
        randbtn.clicked += () => { GenerateRandomPoints(); };
        root.Add(randbtn);
        var btn = new Button();
        btn.text = "save test";
        btn.clicked += () => { SaveEditorData(); };
        root.Add(btn);
        var toggleBtn = new Button();
        toggleBtn.text = isPlacing ? finishToggleText : startToggleText;
        toggleBtn.clicked += () => {
            isPlacing = !isPlacing;
            toggleBtn.text = isPlacing ? finishToggleText : startToggleText;
        };
        root.Add(toggleBtn);


        serializedObject = new SerializedObject(this);
        root.Bind(serializedObject);


        GenerateRandomPoints();

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
        var me = this;
        // var me = new SerializedObject(this).targetObject;

        // string v = JsonUtility.ToJson(me);
        // string key = GetType().Name.ToUpper();
        // Debug.Log(key+" "+v);
        // PropCannon objectToOverwrite = ScriptableObject.CreateInstance<PropCannon>();
        // JsonUtility.FromJsonOverwrite(v, objectToOverwrite);
        // Debug.Log(JsonUtility.ToJson(objectToOverwrite));
        // still need to set individually


    }
    void LoadEditorData()
    {

    }
    void SceneGUI(SceneView sceneView)
    {
        if (Event.current.type == EventType.MouseMove)
        {
            sceneView.Repaint();
        }
        if (!isPlacing)
        {
            return;
        }
        if (Event.current.type == EventType.MouseDown && Event.current.button == 0)
        {
            if (SpawnProps())
            {
                Event.current.Use();
            }
        }
        if (Event.current.type == EventType.ScrollWheel && Event.current.control)
        {
            // change brush size with scroll wheel
            serializedObject.Update();
            var brprop = serializedObject.FindProperty(nameof(brushRadius));

            float scrollAmount = Event.current.delta.y;
            scrollAmount = -Mathf.Sign(scrollAmount);
            brprop.floatValue *= 1 + scrollAmount * 0.2f;
            brprop.floatValue = Mathf.Max(0.01f, brprop.floatValue);
            serializedObject.ApplyModifiedProperties();

            Repaint();
            sceneView.Repaint();
            Event.current.Use();
        }
        if (Event.current.type == EventType.Repaint)
        {
            Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
            Transform cam = sceneView.camera.transform;
            Ray mouseRay = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

            Ray camRay = new Ray(cam.position, cam.forward);
            // Ray camRay = sceneView.camera.ScreenPointToRay(Event.current.mousePosition);
            if (Physics.Raycast(mouseRay, out var hit, 1000, layerMask))
            {
                Vector3 hitNormal = hit.normal;
                Vector3 hitTangent = Vector3.Cross(hitNormal, cam.up).normalized;
                Vector3 hitBitangent = -Vector3.Cross(hit.normal, hitTangent);

                // calculate and draw points
                Vector3[] points = SnapPointsToTerrain(hit.point, hitNormal, hitTangent, hitBitangent);
                Handles.color = Color.black;
                foreach (var point in points)
                {
                    if (point != Vector3.negativeInfinity)
                        Handles.SphereHandleCap(-1, point, Quaternion.identity, 0.2f, EventType.Repaint);
                    // Handles.DrawAAPolyLine(4, point, point + hitNormal);
                }

                // draw tangent space axis
                Handles.color = Color.red;
                Handles.DrawAAPolyLine(4, hit.point, hit.point + hitTangent);
                Handles.color = Color.green;
                Handles.DrawAAPolyLine(4, hit.point, hit.point + hit.normal);
                Handles.color = Color.blue;
                Handles.DrawAAPolyLine(4, hit.point, hit.point + hitBitangent);

                // draw circle
                // Handles.zTest = UnityEngine.Rendering.CompareFunction.Always;
                Handles.color = Color.black;
                // Handles.DrawWireDisc(hit.point, hit.normal, brushRadius);
                // Handles.color = Color.white;
                // Handles.zTest = UnityEngine.Rendering.CompareFunction.LessEqual;
                const int circleDetail = 64;
                Vector3[] circlePoints = new Vector3[circleDetail];
                for (int i = 0; i < circleDetail; i++)
                {
                    float ang = (float)i / ((float)circleDetail - 1f) * Mathf.PI * 2;
                    Vector2 circumference = new Vector2(Mathf.Cos(ang), Mathf.Sin(ang));
                    Vector3 tangetPos = hit.point + (hitTangent * circumference.x + hitBitangent * circumference.y) * brushRadius;
                    Vector3 point = tangetPos + hitNormal * upDist;
                    Ray ray = new Ray(point, -hitNormal);
                    if (Physics.Raycast(ray, out var cirHit, upDist + downDist, layerMask))
                    {
                        circlePoints[i] = cirHit.point + cirHit.normal * 0.02f;
                    } else
                    {
                        circlePoints[i] = tangetPos;
                    }
                }
                Handles.DrawAAPolyLine(2, circlePoints);
            } else
            {
                // mouse not over surface
                if (placedTransforms.Length != 0)
                {
                    placedTransforms = new Matrix4x4[0];
                }
            }
        }
    }
    void GenerateRandomPoints()
    {
        int numPoints = Random.Range(spawnAmountMin, spawnAmountMax);
        // random positions in brush
        randPoints = new Vector2[numPoints];
        randRots = new float[numPoints];
        for (int i = 0; i < numPoints; i++)
        {
            randPoints[i] = Random.insideUnitCircle;
            randRots[i] = Random.Range(0, 360);
        }
        numPointsLabel.text = " num points: " + randPoints.Length;
        SceneView.RepaintAll();
        Repaint();
    }

    Vector3[] SnapPointsToTerrain(Vector3 point, Vector3 normal, Vector3 xaxis, Vector3 yaxis)
    {
        if (randPoints.Length == 0)
        {
            GenerateRandomPoints();
        }
        int numPoints = randPoints.Length;
        Vector3[] positions = new Vector3[numPoints];
        placedTransforms = new Matrix4x4[numPoints];
        // snap to terrain
        for (int i = 0; i < numPoints; i++)
        {
            Vector3 tangetPos = point + (xaxis * randPoints[i].x + yaxis * randPoints[i].y) * brushRadius;
            var startPos = tangetPos + normal * upDist;
            if (Physics.Raycast(startPos, -normal, out var hit, upDist + downDist, layerMask))
            {
                // Handles.color = Color.red;
                positions[i] = hit.point;
                // Handles.DrawAAPolyLine(4, startPos, hit.point);
                // random rotation
                Quaternion rot = Quaternion.LookRotation(yaxis, hit.normal).normalized;
                rot *= Quaternion.Euler(randRots[i] * Vector3.up).normalized;
                placedTransforms[i] = Matrix4x4.TRS(positions[i], rot, Vector3.one);
            } else
            {
                // Handles.color = Color.white;
                // positions[i] = tangetPos;
                positions[i] = Vector3.negativeInfinity;
                // Handles.DrawAAPolyLine(4, startPos, startPos -normal * (upDist + downDist));
                placedTransforms[i] = Matrix4x4.zero;
            }
        }
        return positions;
    }
    bool PreviewSpawnProps()
    {
        return true;
    }
    // returns false if points are all invalid
    bool SpawnProps()
    {
        if (placedTransforms.Length == 0)
        {
            return false;
        }
        foreach (var placedTransform in placedTransforms)
        {
            if (placedTransform.ValidTRS() && placedTransform != Matrix4x4.zero)
            {
                GameObject gameObject = ObjectFactory.CreatePrimitive(PrimitiveType.Cube);
                // GameObject gameObject = ObjectFactory.CreateGameObject("prop");

                gameObject.transform.localScale = Vector3.Scale(placedTransform.lossyScale, new Vector3(1, 2, 1));
                gameObject.transform.rotation = placedTransform.rotation;
                gameObject.transform.position = placedTransform.GetColumn(3);
                gameObject.transform.position += Vector3.up * 0.9f;
                if (Selection.activeGameObject)
                {
                    // gameObject.transform.SetParent(Selection.activeGameObject.transform);
                    // problem when undoing?
                }
            }
        }
        GenerateRandomPoints();
        return true;
    }
}