using System.IO;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

/// <summary>只更新室内标题与一件科研摆件；不重建场景或修改门动画。</summary>
public static class LabPersonalization
{
    private const string AssetRoot = "Assets/ThirdParty/KenneySpaceKit/";
    private const string Title = "welcom to LJL‘s Lab’";

    [MenuItem("Door Lab/8. Apply requested lab detail %#&l")]
    public static void Apply()
    {
        if (EditorApplication.isPlaying) return;
        foreach (var label in Object.FindObjectsOfType<TextMesh>())
            if (label.text == "VIRTUAL REALITY" || label.text == Title)
            {
                label.text = Title;
                label.gameObject.name = Title;
                EditorUtility.SetDirty(label);
            }
        AddDish(GameObject.Find("Environment").transform);
        EditorSceneManager.MarkSceneDirty(UnityEngine.SceneManagement.SceneManager.GetActiveScene());
        DoorLabBuilder.SaveAll();
        Debug.Log("LAB_PERSONALIZATION_COMPLETE");
    }

    public static void AddDish(Transform parent)
    {
        // 幂等执行：已有摆件时保留其位置，避免重复添加。
        if (parent.Find("ResearchSatelliteDish") != null) return;
        var model = AssetDatabase.LoadAssetAtPath<GameObject>(AssetRoot + "satelliteDish_detailed.obj");
        if (model == null) { Debug.LogError("Satellite dish asset was not imported."); return; }
        var instance = (GameObject)PrefabUtility.InstantiatePrefab(model);
        instance.name = "ResearchSatelliteDish";
        instance.transform.SetParent(parent, false);
        instance.transform.rotation = Quaternion.Euler(0, 155, 0);
        var renderers = instance.GetComponentsInChildren<Renderer>();
        var bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        instance.transform.localScale *= 1.35f / Mathf.Max(.01f, bounds.size.y);
        bounds = renderers[0].bounds;
        foreach (var renderer in renderers) bounds.Encapsulate(renderer.bounds);
        // 放在右侧工作台前部，底座贴合桌面，保留显示器和中央展台。
        instance.transform.position += new Vector3(3.45f, 1.105f, 4.45f) - new Vector3(bounds.center.x, bounds.min.y, bounds.center.z);

        Directory.CreateDirectory(AssetRoot + "Materials");
        foreach (var renderer in renderers)
        {
            var materials = renderer.sharedMaterials;
            for (int i = 0; i < materials.Length; i++)
            {
                string name = materials[i] != null ? materials[i].name : "metal";
                string path = AssetRoot + "Materials/" + name + ".mat";
                var material = AssetDatabase.LoadAssetAtPath<Material>(path);
                if (material == null)
                {
                    material = new Material(Shader.Find("Standard"));
                    // 保留 Kenney 原模型 MTL 中的色板，不依赖外部纹理。
                    material.color = name.Contains("Red") ? new Color(1,.6285f,.2028f) :
                        name == "dark" ? new Color(.2745f,.298f,.3412f) :
                        name.Contains("Dark") ? new Color(.675f,.710f,.7736f) : new Color(.8431f,.8706f,.9098f);
                    material.SetFloat("_Metallic", .45f);
                    material.SetFloat("_Glossiness", .4f);
                    AssetDatabase.CreateAsset(material, path);
                }
                materials[i] = material;
            }
            renderer.sharedMaterials = materials;
        }
    }

    [MenuItem("Door Lab/9. Inspect lab interior %#&i")]
    public static void InspectInterior()
    {
        if (!EditorApplication.isPlaying) return;
        var player = Object.FindObjectOfType<PlayerController>();
        player.Teleport(new Vector3(.4f,.08f,1f), 8);
        player.LockCursor(false);
        // 等待一帧，捕获实际 Game 摄像机画面作为素材检查证据。
        EditorApplication.delayCall += () => ScreenCapture.CaptureScreenshot("Docs/Verification/08-lab-interior.png");
    }
}
