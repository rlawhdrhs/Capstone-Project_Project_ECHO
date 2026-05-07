using UnityEngine;

public class MissionLightActivator : MonoBehaviour
{
    [Header("감지할 Layer")]
    public LayerMask missionLightLayer;

    [Header("클릭 가능한 큐브")]
    public GameObject targetCube;

    private bool activated = false;

    void Start()
    {
        TurnOffMissionLights();
    }

    void Update()
    {
        // 마우스 왼쪽 클릭
        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            // 클릭한 오브젝트 검사
            if (Physics.Raycast(ray, out hit))
            {
                // 클릭한 게 targetCube인지 확인
                if (hit.collider.gameObject == targetCube)
                {
                    ActivateMissionLights();
                }
            }
        }
    }

    // 불 켜기
    public void ActivateMissionLights()
    {
        if (activated) return;

        activated = true;

        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            // MissionLight Layer 검사
            if (((1 << obj.layer) & missionLightLayer) != 0)
            {
                Light lightComponent = obj.GetComponent<Light>();

                if (lightComponent != null)
                {
                    lightComponent.enabled = true;
                }
            }
        }

        Debug.Log("Mission Light 활성화");
    }

    // 시작 시 전부 끄기
    void TurnOffMissionLights()
    {
        GameObject[] allObjects = FindObjectsOfType<GameObject>();

        foreach (GameObject obj in allObjects)
        {
            if (((1 << obj.layer) & missionLightLayer) != 0)
            {
                Light lightComponent = obj.GetComponent<Light>();

                if (lightComponent != null)
                {
                    lightComponent.enabled = false;
                }
            }
        }
    }
}