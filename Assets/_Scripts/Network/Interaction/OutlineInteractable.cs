using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;
using UnityEngine.XR.Interaction.Toolkit.Interactables;

public class OutlineInteractable : MonoBehaviour
{
    [Header("Outline Settings")]
    [SerializeField] private Material outlineMaterial; // URP 아웃라인 머티리얼

    [Header("UI Indicator Settings")]
    [SerializeField] private GameObject hoverIndicatorUI; // 월드 스페이스 캔버스 등록

    private MeshRenderer meshRenderer;
    private XRBaseInteractable interactable;

    private void Awake()
    {
        // 1. 상시 아웃라인 적용
        meshRenderer = GetComponentInChildren<MeshRenderer>();
        if (meshRenderer != null)
        {
            Material[] originalMaterials = meshRenderer.materials;
            Material[] outlineMaterials = new Material[originalMaterials.Length + 1];
            for (int i = 0; i < originalMaterials.Length; i++)
            {
                outlineMaterials[i] = originalMaterials[i];
            }
            outlineMaterials[outlineMaterials.Length - 1] = outlineMaterial;
            meshRenderer.materials = outlineMaterials;
        }

        // 2. XR 상호작용 컴포넌트 가져오기
        interactable = GetComponent<XRBaseInteractable>();
        if (interactable == null)
        {
            Debug.LogError($"[{gameObject.name}] XR Interactable 컴포넌트가 필요합니다.");
            enabled = false;
            return;
        }

        // 처음에 동그라미 UI는 비활성화
        if (hoverIndicatorUI != null)
        {
            hoverIndicatorUI.SetActive(false);
        }
    }

    private void OnEnable()
    {
        // 호버 및 셀렉트(그랩) 이벤트 모두 구독
        interactable.hoverEntered.AddListener(OnHoverEnter);
        interactable.hoverExited.AddListener(OnHoverExit);
        interactable.selectEntered.AddListener(OnSelectEnter); // 잡았을 때
        interactable.selectExited.AddListener(OnSelectExit);   // 놓았을 때
    }

    private void OnDisable()
    {
        // 이벤트 구독 해제
        interactable.hoverEntered.RemoveListener(OnHoverEnter);
        interactable.hoverExited.RemoveListener(OnHoverExit);
        interactable.selectEntered.RemoveListener(OnSelectEnter);
        interactable.selectExited.RemoveListener(OnSelectExit);
    }

    private void Update()
    {
        // 동그라미 UI가 활성화되어 있을 때만 카메라를 바라보도록 처리
        if (hoverIndicatorUI != null && hoverIndicatorUI.activeSelf && Camera.main != null)
        {
            hoverIndicatorUI.transform.LookAt(
                hoverIndicatorUI.transform.position + Camera.main.transform.rotation * Vector3.forward,
                Camera.main.transform.rotation * Vector3.up
            );
        }
    }

    // 레이가 물체에 닿았을 때 (Hover 진입)
    private void OnHoverEnter(HoverEnterEventArgs args)
    {
        // 이미 물건을 잡고 있는 상태가 아닐 때만 동그라미를 켭니다.
        if (hoverIndicatorUI != null && !interactable.isSelected)
        {
            hoverIndicatorUI.SetActive(true);
        }
    }

    // 레이가 물체에서 벗어났을 때 (Hover 이탈)
    private void OnHoverExit(HoverExitEventArgs args)
    {
        if (hoverIndicatorUI != null)
        {
            hoverIndicatorUI.SetActive(false);
        }
    }

    // 버튼을 눌러 물체를 잡았을 때 (Select 진입)
    private void OnSelectEnter(SelectEnterEventArgs args)
    {
        // 잡는 순간 동그라미 UI를 끕니다.
        if (hoverIndicatorUI != null)
        {
            hoverIndicatorUI.SetActive(false);
        }
    }

    // 잡고 있던 물체를 놓았을 때 (Select 이탈)
    private void OnSelectExit(SelectExitEventArgs args)
    {
        // 물건을 놓았을 때, 여전히 레이가 이 물체를 가리키고 있다면(Hover 상태라면) 동그라미를 다시 켜줍니다.
        if (hoverIndicatorUI != null && interactable.isHovered)
        {
            hoverIndicatorUI.SetActive(true);
        }
    }
}