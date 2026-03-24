using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Tilemaps;
using UnityEditor;
using UnityEditor.SceneManagement;

/// <summary>
/// One-click scene setup tool.
/// Menu: FactoryFlow > Setup Scene
/// Wires all components, creates Canvas UI, and assigns references.
/// </summary>
public static class SceneSetup
{
    [MenuItem("FactoryFlow/Setup Scene")]
    public static void SetupScene()
    {
        // --- GameManager object ---
        GameObject gmObj = GameObject.Find("GameManager");
        if (gmObj == null)
        {
            gmObj = new GameObject("GameManager");
            Debug.Log("[SceneSetup] Created GameManager object.");
        }

        // Remove old scripts if present
        RemoveComponent<GridTester>(gmObj);
        RemoveComponent<DebugUI>(gmObj);

        // Ensure core scripts
        GameManager gm = EnsureComponent<GameManager>(gmObj);
        GridSystem gs = EnsureComponent<GridSystem>(gmObj);
        ResourceManager rm = EnsureComponent<ResourceManager>(gmObj);
        ResourceFactory rf = EnsureComponent<ResourceFactory>(gmObj);
        MoneyManager mm = EnsureComponent<MoneyManager>(gmObj);
        TilemapReader tr = EnsureComponent<TilemapReader>(gmObj);
        BuildingPlacer bp = EnsureComponent<BuildingPlacer>(gmObj);
        EnsureComponent<ConveyorPlacer>(gmObj);

        // --- Wire GameManager references ---
        SerializedObject gmSO = new SerializedObject(gm);
        gmSO.FindProperty("_gridSystem").objectReferenceValue = gs;
        gmSO.FindProperty("_resourceManager").objectReferenceValue = rm;
        gmSO.FindProperty("_resourceFactory").objectReferenceValue = rf;
        gmSO.FindProperty("_moneyManager").objectReferenceValue = mm;
        gmSO.FindProperty("_tilemapReader").objectReferenceValue = tr;
        gmSO.FindProperty("_buildingPlacer").objectReferenceValue = bp;
        gmSO.ApplyModifiedProperties();
        Debug.Log("[SceneSetup] Wired GameManager references.");

        // --- Wire ResourceFactory prefabs ---
        SerializedObject rfSO = new SerializedObject(rf);
        AssignPrefab(rfSO, "_ironOrePrefab", "Assets/Sprites/Iron.prefab", "Iron");
        AssignPrefab(rfSO, "_copperOrePrefab", "Assets/Sprites/Copper.prefab", "Copper");
        AssignPrefab(rfSO, "_coalPrefab", "Assets/Sprites/Coal.prefab", "Coal");
        AssignPrefab(rfSO, "_stonePrefab", "Assets/Sprites/Stone.prefab", "Stone");
        rfSO.ApplyModifiedProperties();
        Debug.Log("[SceneSetup] Wired ResourceFactory prefabs.");

        // --- Wire TilemapReader ---
        SerializedObject trSO = new SerializedObject(tr);

        // Find the Ground tilemap
        GameObject groundObj = GameObject.Find("Ground");
        if (groundObj != null)
        {
            Tilemap tilemap = groundObj.GetComponent<Tilemap>();
            if (tilemap != null)
            {
                trSO.FindProperty("_groundTilemap").objectReferenceValue = tilemap;
                Debug.Log("[SceneSetup] Assigned Ground tilemap.");
            }
        }

        // Assign tile assets
        AssignTileAsset(trSO, "_ironOreTile", "Assets/Sprites/IronOreTile.asset", "IronOre");
        AssignTileAsset(trSO, "_copperOreTile", "Assets/Sprites/CopperOreTIle.asset", "CopperOre");
        AssignTileAsset(trSO, "_coalTile", "Assets/Sprites/CoalTile.asset", "Coal");
        AssignTileAsset(trSO, "_stoneTile", "Assets/Sprites/StoneTIle.asset", "Stone");
        trSO.ApplyModifiedProperties();
        Debug.Log("[SceneSetup] Wired TilemapReader references.");

        // --- Create Canvas UI ---
        SetupCanvasUI();

        // Mark scene dirty
        EditorSceneManager.MarkSceneDirty(gmObj.scene);
        Debug.Log("[SceneSetup] === Setup Complete! Press Ctrl+S to save the scene. ===");
    }

    private static void SetupCanvasUI()
    {
        // Find or create Canvas
        Canvas canvas = Object.FindFirstObjectByType<Canvas>();
        GameObject canvasObj;

        if (canvas == null)
        {
            canvasObj = new GameObject("GameCanvas");
            canvas = canvasObj.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 100;

            CanvasScaler scaler = canvasObj.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            canvasObj.AddComponent<GraphicRaycaster>();
            Debug.Log("[SceneSetup] Created Canvas.");
        }
        else
        {
            canvasObj = canvas.gameObject;
        }

        // --- Mode Indicator (top-left) ---
        CreateTextElement(canvasObj, "ModeIndicator",
            new Vector2(10, -10), new Vector2(500, 40),
            TextAnchor.UpperLeft, 22, Color.white,
            new Vector2(0, 1), new Vector2(0, 1));

        // Ensure ModeIndicator script
        ModeIndicator mi = canvasObj.GetComponentInChildren<ModeIndicator>();
        if (mi == null)
        {
            GameObject miObj = GameObject.Find("ModeIndicator");
            if (miObj != null)
            {
                mi = miObj.AddComponent<ModeIndicator>();
                SerializedObject miSO = new SerializedObject(mi);
                miSO.FindProperty("_modeText").objectReferenceValue = miObj.GetComponent<Text>();
                miSO.ApplyModifiedProperties();
            }
        }

        // --- Money Display (top-right) ---
        CreateTextElement(canvasObj, "MoneyDisplay",
            new Vector2(-10, -10), new Vector2(300, 40),
            TextAnchor.UpperRight, 28, new Color(0.9f, 0.8f, 0.3f),
            new Vector2(1, 1), new Vector2(1, 1));

        MoneyDisplay md = canvasObj.GetComponentInChildren<MoneyDisplay>();
        if (md == null)
        {
            GameObject mdObj = GameObject.Find("MoneyDisplay");
            if (mdObj != null)
            {
                md = mdObj.AddComponent<MoneyDisplay>();
                SerializedObject mdSO = new SerializedObject(md);
                mdSO.FindProperty("_moneyText").objectReferenceValue = mdObj.GetComponent<Text>();
                mdSO.ApplyModifiedProperties();
            }
        }

        // --- Toolbar (bottom-center) ---
        SetupToolbar(canvasObj);

        Debug.Log("[SceneSetup] Canvas UI setup complete.");
    }

    private static void SetupToolbar(GameObject canvasObj)
    {
        // Find or create toolbar container
        Transform toolbarTransform = canvasObj.transform.Find("Toolbar");
        GameObject toolbarObj;

        if (toolbarTransform == null)
        {
            toolbarObj = new GameObject("Toolbar");
            toolbarObj.transform.SetParent(canvasObj.transform, false);

            RectTransform rt = toolbarObj.AddComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.5f, 0);
            rt.anchorMax = new Vector2(0.5f, 0);
            rt.pivot = new Vector2(0.5f, 0);
            rt.anchoredPosition = new Vector2(0, 10);
            rt.sizeDelta = new Vector2(700, 70);

            // Background
            Image bg = toolbarObj.AddComponent<Image>();
            bg.color = new Color(0.1f, 0.1f, 0.12f, 0.85f);

            // Layout
            HorizontalLayoutGroup hlg = toolbarObj.AddComponent<HorizontalLayoutGroup>();
            hlg.spacing = 4;
            hlg.padding = new RectOffset(5, 5, 5, 5);
            hlg.childAlignment = TextAnchor.MiddleCenter;
            hlg.childForceExpandWidth = false;
            hlg.childForceExpandHeight = false;
        }
        else
        {
            toolbarObj = toolbarTransform.gameObject;
        }

        // Create 10 slots
        string[] names = { "1:Miner", "2:Conveyor", "3:Splitter", "4:Merger", "5:Station",
                           "6:", "7:", "8:", "9:", "0:" };
        Color[] colors = {
            new Color(0.6f, 0.4f, 0.2f),
            new Color(0.4f, 0.4f, 0.45f),
            new Color(0.2f, 0.4f, 0.7f),
            new Color(0.2f, 0.6f, 0.3f),
            new Color(0.7f, 0.5f, 0.1f),
            Color.clear, Color.clear, Color.clear, Color.clear, Color.clear
        };

        // Only create slots if they don't exist
        if (toolbarObj.transform.childCount < 10)
        {
            // Clear existing
            while (toolbarObj.transform.childCount > 0)
                Object.DestroyImmediate(toolbarObj.transform.GetChild(0).gameObject);

            for (int i = 0; i < 10; i++)
            {
                GameObject slot = new GameObject($"Slot_{i}");
                slot.transform.SetParent(toolbarObj.transform, false);

                RectTransform srt = slot.AddComponent<RectTransform>();
                srt.sizeDelta = new Vector2(64, 60);

                Image slotBg = slot.AddComponent<Image>();
                if (i < 5)
                    slotBg.color = new Color(0.2f, 0.2f, 0.25f, 0.8f);
                else
                    slotBg.color = new Color(0.15f, 0.15f, 0.18f, 0.5f);

                // Icon area
                if (i < 5)
                {
                    GameObject icon = new GameObject("Icon");
                    icon.transform.SetParent(slot.transform, false);
                    RectTransform irt = icon.AddComponent<RectTransform>();
                    irt.anchorMin = Vector2.zero;
                    irt.anchorMax = Vector2.one;
                    irt.offsetMin = new Vector2(4, 14);
                    irt.offsetMax = new Vector2(-4, -4);

                    Image iconImg = icon.AddComponent<Image>();
                    iconImg.color = colors[i];
                }

                // Label
                GameObject label = new GameObject("Label");
                label.transform.SetParent(slot.transform, false);
                RectTransform lrt = label.AddComponent<RectTransform>();
                lrt.anchorMin = new Vector2(0, 0);
                lrt.anchorMax = new Vector2(1, 0);
                lrt.pivot = new Vector2(0.5f, 0);
                lrt.offsetMin = Vector2.zero;
                lrt.offsetMax = new Vector2(0, 14);

                Text labelText = label.AddComponent<Text>();
                labelText.text = names[i];
                labelText.fontSize = 9;
                labelText.alignment = TextAnchor.MiddleCenter;
                labelText.color = Color.white;
                labelText.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");

                // Button for click
                Button btn = slot.AddComponent<Button>();
                int slotIndex = i;
                // Note: button click will be wired via ToolbarUI at runtime
            }
        }

        // Add ToolbarUI script
        ToolbarUI toolbar = toolbarObj.GetComponent<ToolbarUI>();
        if (toolbar == null)
        {
            toolbar = toolbarObj.AddComponent<ToolbarUI>();
        }

        // Wire slot backgrounds
        SerializedObject tbSO = new SerializedObject(toolbar);
        SerializedProperty bgProp = tbSO.FindProperty("_slotBackgrounds");
        bgProp.arraySize = 10;
        for (int i = 0; i < 10; i++)
        {
            if (i < toolbarObj.transform.childCount)
            {
                Image img = toolbarObj.transform.GetChild(i).GetComponent<Image>();
                bgProp.GetArrayElementAtIndex(i).objectReferenceValue = img;
            }
        }
        tbSO.ApplyModifiedProperties();

        Debug.Log("[SceneSetup] Toolbar created with 10 slots.");
    }

    private static void CreateTextElement(GameObject parent, string name,
        Vector2 position, Vector2 size, TextAnchor alignment, int fontSize,
        Color color, Vector2 anchorMin, Vector2 anchorMax)
    {
        Transform existing = parent.transform.Find(name);
        if (existing != null) return;

        GameObject textObj = new GameObject(name);
        textObj.transform.SetParent(parent.transform, false);

        RectTransform rt = textObj.AddComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.pivot = anchorMin;
        rt.anchoredPosition = position;
        rt.sizeDelta = size;

        Text text = textObj.AddComponent<Text>();
        text.text = name;
        text.fontSize = fontSize;
        text.alignment = alignment;
        text.color = color;
        text.font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
    }

    private static T EnsureComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component == null)
        {
            component = obj.AddComponent<T>();
            Debug.Log($"[SceneSetup] Added {typeof(T).Name}.");
        }
        return component;
    }

    private static void RemoveComponent<T>(GameObject obj) where T : Component
    {
        T component = obj.GetComponent<T>();
        if (component != null)
        {
            Object.DestroyImmediate(component);
            Debug.Log($"[SceneSetup] Removed {typeof(T).Name}.");
        }
    }

    private static void AssignPrefab(SerializedObject so, string propertyName, string assetPath, string label)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null) return;
        GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(assetPath);
        if (prefab != null)
        {
            prop.objectReferenceValue = prefab;
            Debug.Log($"[SceneSetup] Assigned {label} prefab from {assetPath}.");
        }
        else
        {
            Debug.LogWarning($"[SceneSetup] Could not find prefab at {assetPath}.");
        }
    }

    private static void AssignTileAsset(SerializedObject so, string propertyName, string assetPath, string label)
    {
        SerializedProperty prop = so.FindProperty(propertyName);
        if (prop == null) return;
        TileBase tile = AssetDatabase.LoadAssetAtPath<TileBase>(assetPath);
        if (tile != null)
        {
            prop.objectReferenceValue = tile;
            Debug.Log($"[SceneSetup] Assigned {label} tile from {assetPath}.");
        }
        else
        {
            Debug.LogWarning($"[SceneSetup] Could not find tile at {assetPath}.");
        }
    }

    [MenuItem("FactoryFlow/Remove Missing Scripts")]
    public static void RemoveMissingScripts()
    {
        GameObject[] allObjects = GameObject.FindObjectsByType<GameObject>(FindObjectsSortMode.None);
        int removedCount = 0;

        foreach (GameObject go in allObjects)
        {
            int count = GameObjectUtility.GetMonoBehavioursWithMissingScriptCount(go);
            if (count > 0)
            {
                GameObjectUtility.RemoveMonoBehavioursWithMissingScript(go);
                removedCount += count;
                Debug.Log($"[SceneSetup] Removed {count} missing script(s) from '{go.name}'.");
            }
        }

        if (removedCount == 0)
        {
            Debug.Log("[SceneSetup] No missing scripts found.");
        }
        else
        {
            EditorSceneManager.MarkSceneDirty(
                UnityEngine.SceneManagement.SceneManager.GetActiveScene());
            Debug.Log($"[SceneSetup] Removed {removedCount} missing script(s) total. Press Ctrl+S to save.");
        }
    }
}
