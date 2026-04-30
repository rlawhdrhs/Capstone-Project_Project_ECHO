using System.Collections.Generic;
using UnityEngine;

public class OutlineLayerApplier : MonoBehaviour
{
    [Header("Target Layer (아웃라인 적용 대상)")]
    public string targetLayerName = "Outline";

    [Header("Outline")]
    public Material outlineMaterial;
    [Range(0.001f, 0.1f)]
    public float outlineWidth = 0.03f;
    public Color outlineColor = new Color(1f, 0.2f, 0.1f, 1f);

    [Header("Options")]
    public bool applyOnStart = true;
    public bool clearOldOutlineChildren = true;

    private readonly List<GameObject> createdOutlines = new();

    void Start()
    {
        if (applyOnStart)
        {
            ApplyOutlines();
        }
    }

    [ContextMenu("Apply Outlines")]
    public void ApplyOutlines()
    {
        if (outlineMaterial == null)
        {
            Debug.LogError("outlineMaterial 비어있음");
            return;
        }

        int targetLayer = LayerMask.NameToLayer(targetLayerName);
        if (targetLayer < 0)
        {
            Debug.LogError($"Layer '{targetLayerName}' 없음");
            return;
        }

        if (clearOldOutlineChildren)
        {
            ClearGeneratedOutlines();
        }

        Renderer[] renderers = FindObjectsOfType<Renderer>(true);

        foreach (Renderer rend in renderers)
        {
            if (rend.gameObject.layer != targetLayer)
                continue;

            if (rend is ParticleSystemRenderer)
                continue;

            CreateOutline(rend);
        }
    }

    [ContextMenu("Clear Outlines")]
    public void ClearGeneratedOutlines()
    {
        foreach (GameObject go in createdOutlines)
        {
            if (go != null)
                DestroyImmediate(go);
        }

        createdOutlines.Clear();
    }

    private void CreateOutline(Renderer sourceRenderer)
    {
        Transform source = sourceRenderer.transform;

        // 중복 생성 방지
        if (source.Find("__Outline") != null)
            return;

        GameObject outlineObj = new GameObject("__Outline");
        outlineObj.transform.SetParent(source, false);

        //: Outline Layer로 설정
        int outlineLayer = LayerMask.NameToLayer("Outline");
        outlineObj.layer = outlineLayer;

        // Material 생성
        Material runtimeMat = new Material(outlineMaterial);
        runtimeMat.SetColor("_OutlineColor", outlineColor);
        runtimeMat.SetFloat("_OutlineWidth", outlineWidth);

        if (sourceRenderer is MeshRenderer)
        {
            MeshFilter src = source.GetComponent<MeshFilter>();
            if (src == null) return;

            MeshFilter dst = outlineObj.AddComponent<MeshFilter>();
            dst.sharedMesh = src.sharedMesh;

            MeshRenderer mr = outlineObj.AddComponent<MeshRenderer>();
            mr.sharedMaterial = runtimeMat;
        }
        else if (sourceRenderer is SkinnedMeshRenderer skinned)
        {
            SkinnedMeshRenderer dst = outlineObj.AddComponent<SkinnedMeshRenderer>();
            dst.sharedMesh = skinned.sharedMesh;
            dst.bones = skinned.bones;
            dst.rootBone = skinned.rootBone;
            dst.sharedMaterial = runtimeMat;
        }

        createdOutlines.Add(outlineObj);
    }
}