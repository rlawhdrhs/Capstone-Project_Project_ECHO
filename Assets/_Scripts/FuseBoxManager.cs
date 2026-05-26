using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Interactors;

public class FuseBoxManager : MonoBehaviour
{
    [Header("소켓 설정")]
    public XRSocketInteractor socket1;
    public XRSocketInteractor socket2;
    public XRSocketInteractor socket3;

    [Header("전구 오브젝트")]
    public MeshRenderer light1;
    public MeshRenderer light2;
    public MeshRenderer light3;

    [Header("불빛 색상")]
    public Material offMaterial;   // 까만불
    public Material redMaterial;   // 빨간불
    public Material greenMaterial; // 초록불

    void Update()
    {
        // 1. 각각의 소켓에 퓨즈가 꽂혀있는지 확인 (hasSelection이 true면 꽂힌 상태)
        bool isFuse1In = socket1.hasSelection;
        bool isFuse2In = socket2.hasSelection;
        bool isFuse3In = socket3.hasSelection;

        // 2. 3개가 전부 다 꽂혔는지 확인
        bool allFusesIn = isFuse1In && isFuse2In && isFuse3In;

        if (allFusesIn)
        {
            light1.material = greenMaterial;
            light2.material = greenMaterial;
            light3.material = greenMaterial;
            
        }
        else
        {
            light1.material = isFuse1In ? redMaterial : offMaterial;
            light2.material = isFuse2In ? redMaterial : offMaterial;
            light3.material = isFuse3In ? redMaterial : offMaterial;
        }
    }
}