using UnityEngine;

public class MissionLightActivator : MonoBehaviour
{
    [Header("감지할 Layer")]
    public LayerMask missionLightLayer;

    [Header("클릭 가능한 큐브")]
    public GameObject targetCube;

    [Header("사운드 매니저 연동 설정")]
    public SoundType lightActivationSoundType;

    private bool activated = false;

    public DataMissionSpawner DataMissionSpawner;

    void Start()
    {
        TurnOffMissionLights();
    }

    /*void Update()
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
    }*/

    public void OnPlayButtonClicked()
    {
        ActivateMissionLights();
        NetworkGameManager.Instance.Rpc_CompletePowerRestore();
    }

    // 불 켜기
    public void ActivateMissionLights()
    {
        if (activated) return;

        activated = true;

        if (NetworkGameManager.Instance != null)
        {
            NetworkGameManager.Instance.RPC_PlayGlobalSound(transform.position, 5.0f, lightActivationSoundType);
        }
        else if (SoundManager.Instance != null)
        {
            SoundManager.Instance.EmitSound(transform.position, 5.0f, lightActivationSoundType);
        }

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

        if (NetworkGameManager.Instance != null && NetworkGameManager.Instance.Object.IsValid)
        {
            NetworkGameManager.Instance.Rpc_CompletePowerRestore();
        }
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