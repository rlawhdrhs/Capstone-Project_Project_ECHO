using UnityEngine;
using UnityEditor;
using System;
using System.Reflection;

#if UNITY_EDITOR
public class AcousticProxyGenerator : EditorWindow
{
    [MenuItem("Tools/Acoustic Proxy Generator")]
    public static void ShowWindow()
    {
        GetWindow<AcousticProxyGenerator>("Audio Proxy Gen");
    }

    private void OnGUI()
    {
        GUILayout.Label("1단계: 맵 그룹 선택 후 큐브 생성", EditorStyles.boldLabel);
        if (GUILayout.Button("선택한 그룹의 모든 자식 매쉬 기반 큐브 생성", GUILayout.Height(35)))
        {
            GenerateProxies();
        }

        GUILayout.Space(15);

        GUILayout.Label("2단계: 생성된 1,000개 큐브 한 번에 굽기", EditorStyles.boldLabel);
        GUI.backgroundColor = new Color(0.3f, 1f, 0.4f); // 버튼 색상을 초록색으로 변경
        if (GUILayout.Button("생성된 모든 큐브 일괄 베이크 (Batch Bake)", GUILayout.Height(45)))
        {
            BatchBakeAllProxies();
        }
        GUI.backgroundColor = Color.white;
    }

    private void GenerateProxies()
    {
        GameObject[] selectedObjects = Selection.gameObjects;
        if (selectedObjects.Length == 0)
        {
            Debug.LogWarning("[ProxyGen] 선택된 오브젝트가 없습니다.");
            return;
        }

        Type acousticComponentType = FindTypeInAssemblies("MetaXRAcousticGeometry");
        if (acousticComponentType == null) return;

        GameObject groupRoot = GameObject.Find("Acoustic_Proxies");
        if (groupRoot == null) groupRoot = new GameObject("Acoustic_Proxies");

        int count = 0;
        foreach (GameObject selectedParent in selectedObjects)
        {
            MeshFilter[] allMeshFilters = selectedParent.GetComponentsInChildren<MeshFilter>(true);

            foreach (MeshFilter meshFilter in allMeshFilters)
            {
                GameObject target = meshFilter.gameObject;
                if (meshFilter.sharedMesh == null || target.name.StartsWith("AudioProxy_")) continue;

                GameObject proxyCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
                proxyCube.name = "AudioProxy_" + target.name;
                proxyCube.transform.SetParent(groupRoot.transform);

                Bounds meshBounds = meshFilter.sharedMesh.bounds;
                Vector3 worldCenter = target.transform.TransformPoint(meshBounds.center);
                proxyCube.transform.position = worldCenter;
                proxyCube.transform.rotation = target.transform.rotation;

                Vector3 targetLossyScale = target.transform.lossyScale;
                proxyCube.transform.localScale = new Vector3(
                    meshBounds.size.x * targetLossyScale.x,
                    meshBounds.size.y * targetLossyScale.y,
                    meshBounds.size.z * targetLossyScale.z
                );

                MeshRenderer meshRender = proxyCube.GetComponent<MeshRenderer>();
                if (meshRender != null) meshRender.enabled = false;

                Collider col = proxyCube.GetComponent<Collider>();
                if (col != null) col.isTrigger = true;

                if (proxyCube.GetComponent(acousticComponentType) == null)
                {
                    proxyCube.AddComponent(acousticComponentType);
                }

                count++;
            }
        }
        Debug.Log($"<color=lime>[ProxyGen] 총 {count}개의 프록시 큐브 생성 완료!</color>");
    }

    // ⭐ [새로 추가] 멀티 편집 제한을 우회하여 1,000개 큐브의 'Bake Mesh'를 자동 실행하는 함수
    private void BatchBakeAllProxies()
    {
        GameObject groupRoot = GameObject.Find("Acoustic_Proxies");
        if (groupRoot == null)
        {
            Debug.LogError("[ProxyGen] Acoustic_Proxies 그룹을 찾을 수 없습니다. 1단계를 먼저 진행하세요.");
            return;
        }

        Type acousticComponentType = FindTypeInAssemblies("MetaXRAcousticGeometry");
        if (acousticComponentType == null) return;

        // 그룹 하위에 있는 모든 MetaXRAcousticGeometry 컴포넌트를 싹 수집합니다.
        Component[] acousticComponents = groupRoot.GetComponentsInChildren(acousticComponentType, true);

        if (acousticComponents.Length == 0)
        {
            Debug.LogWarning("[ProxyGen] 베이크할 컴포넌트가 없습니다.");
            return;
        }

        // Meta SDK 내부에 숨겨진 진짜 굽기 메서드(WriteFile)를 리플렉션으로 추출합니다.
        MethodInfo writeFileMethod = acousticComponentType.GetMethod("WriteFile", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);

        if (writeFileMethod == null)
        {
            Debug.LogError("[ProxyGen] Meta SDK의 WriteFile 메서드를 찾을 수 없습니다. SDK 버전을 확인하세요.");
            return;
        }

        int successCount = 0;
        float progress = 0f;

        // 1,000개의 큐브를 돌며 수동 클릭을 코드로 대신합니다.
        for (int i = 0; i < acousticComponents.Length; i++)
        {
            Component comp = acousticComponents[i];

            // 유니티 상단에 로딩 바 표시 (렉 걸려 멈춘 것처럼 보이는 현상 방지)
            progress = (float)i / acousticComponents.Length;
            EditorUtility.DisplayProgressBar("Acoustic Batch Bake", $"{comp.gameObject.name} 굽는 중... ({i}/{acousticComponents.Length})", progress);

            try
            {
                // 각 큐브 오브젝트 인스펙터의 'Bake Mesh' 버튼 함수를 강제로 실행시킵니다.
                writeFileMethod.Invoke(comp, null);
                successCount++;
            }
            catch (Exception e)
            {
                Debug.LogError($"[ProxyGen Error] {comp.gameObject.name} 베이크 실패: {e.Message}");
            }
        }

        // 작업 완료 후 로딩 바 제거 및 로그 출력
        EditorUtility.ClearProgressBar();
        Debug.Log($"<color=cyan><b>[ProxyGen] 멀티 편집 제한을 우회하여 총 {successCount}개의 큐브 지오메트리를 일괄 베이크 완료했습니다!</b></color>");
    }

    private Type FindTypeInAssemblies(string typeName)
    {
        foreach (var assembly in AppDomain.CurrentDomain.GetAssemblies())
        {
            foreach (var type in assembly.GetTypes())
            {
                if (type.Name == typeName) return type;
            }
        }
        return null;
    }
}
#endif