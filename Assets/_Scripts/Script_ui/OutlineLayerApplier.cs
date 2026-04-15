using System.Collections.Generic;
using UnityEngine;

public class OutlineLayerApplier : MonoBehaviour
{
    [Header("Target Layer")]
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
            Debug.LogError("OutlineLayerApplier: outlineMaterial이 비어 있음");
            return;
        }

        int targetLayer = LayerMask.NameToLayer(targetLayerName);
        if (targetLayer < 0)
        {
            Debug.LogError($"OutlineLayerApplier: 레이어 '{targetLayerName}' 없음");
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

            CreateOutlineForRenderer(rend);
        }
    }

    [ContextMenu("Clear Outlines")]
    public void ClearGeneratedOutlines()
    {
        foreach (GameObject go in createdOutlines)
        {
            if (go != null)
            {
                DestroyImmediate(go);
            }
        }

        createdOutlines.Clear();
    }

    private void CreateOutlineForRenderer(Renderer sourceRenderer)
    {
        if (sourceRenderer == null)
            return;

        Transform source = sourceRenderer.transform;

        // 중복 방지
        Transform existing = source.Find("__Outline");
        if (existing != null)
        {
            createdOutlines.Add(existing.gameObject);
            return;
        }

        GameObject outlineObj = new GameObject("__Outline");
        outlineObj.transform.SetParent(source, false);
        outlineObj.transform.localPosition = Vector3.zero;
        outlineObj.transform.localRotation = Quaternion.identity;
        outlineObj.transform.localScale = Vector3.one;
        outlineObj.layer = source.gameObject.layer;

        Material runtimeMat = new Material(outlineMaterial);
        runtimeMat.SetColor("_OutlineColor", outlineColor);
        runtimeMat.SetFloat("_OutlineWidth", outlineWidth);

        if (sourceRenderer is MeshRenderer meshRenderer)
        {
            MeshFilter srcFilter = source.GetComponent<MeshFilter>();
            if (srcFilter == null || srcFilter.sharedMesh == null)
            {
                DestroyImmediate(outlineObj);
                return;
            }

            MeshFilter dstFilter = outlineObj.AddComponent<MeshFilter>();
            dstFilter.sharedMesh = srcFilter.sharedMesh;

            MeshRenderer dstRenderer = outlineObj.AddComponent<MeshRenderer>();
            dstRenderer.sharedMaterial = runtimeMat;
            dstRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dstRenderer.receiveShadows = false;
            dstRenderer.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            dstRenderer.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
        }
        else if (sourceRenderer is SkinnedMeshRenderer skinned)
        {
            if (skinned.sharedMesh == null)
            {
                DestroyImmediate(outlineObj);
                return;
            }

            SkinnedMeshRenderer dst = outlineObj.AddComponent<SkinnedMeshRenderer>();
            dst.sharedMesh = skinned.sharedMesh;
            dst.rootBone = skinned.rootBone;
            dst.bones = skinned.bones;
            dst.sharedMaterial = runtimeMat;
            dst.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
            dst.receiveShadows = false;
            dst.lightProbeUsage = UnityEngine.Rendering.LightProbeUsage.Off;
            dst.reflectionProbeUsage = UnityEngine.Rendering.ReflectionProbeUsage.Off;
            dst.updateWhenOffscreen = true;
        }
        else
        {
            DestroyImmediate(outlineObj);
            return;
        }

        Collider col = outlineObj.GetComponent<Collider>();
        if (col != null)
            DestroyImmediate(col);

        createdOutlines.Add(outlineObj);
    }
}