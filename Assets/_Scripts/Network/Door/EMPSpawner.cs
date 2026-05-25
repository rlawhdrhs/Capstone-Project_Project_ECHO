using Fusion;
using UnityEngine;

public class EMPSpawner : NetworkBehaviour
{
    [Header("폭탄 설정")]
    public GameObject bombPrefab;

    private Transform _rightHandTransform;
    private GameObject _currentHeldBomb; // 🔴 현재 손에 쥐고 있는 폭탄을 기억할 변수

    [Networked] private NetworkBool WasBPressedLastFrame { get; set; }

    public override void Spawned()
    {
        if (Object.HasInputAuthority)
        {
            GameObject handObj = GameObject.Find("LefthandController");
            if (handObj == null) handObj = GameObject.Find("LeftHandController");

            if (handObj != null)
            {
                _rightHandTransform = handObj.transform;
                Debug.Log($"<color=cyan>[스포너 성공] 오른손 컨트롤러 매칭 완료! ({handObj.name})</color>");
            }
            else
            {
                Debug.LogError("<color=red>[스포너 에러] 오른손 컨트롤러를 찾지 못했습니다!</color>");
            }
        }
    }

    public override void FixedUpdateNetwork()
    {
        if (GetInput(out NetworkInputData data))
        {
            if (data.rightButtonB && !WasBPressedLastFrame)
            {
                if (Object.HasInputAuthority && Runner.IsForward)
                {
                    ToggleBombInHand();
                }
            }
            WasBPressedLastFrame = data.rightButtonB;
        }
    }

    private void ToggleBombInHand()
    {
        if (_rightHandTransform == null)
        {
            Debug.LogError("<color=red>[스폰 실패] 오른손 컨트롤러가 지정되지 않았습니다.</color>");
            return;
        }

        if (bombPrefab == null) return;

        // 🔴 [핵심 로직] 이미 생성된 폭탄이 존재하고, 그 폭탄의 부모가 여전히 '내 오른손'인지 검사합니다.
        if (_currentHeldBomb != null && _currentHeldBomb.transform.parent == _rightHandTransform)
        {
            // 손에 들려있는 상태에서 B를 한 번 더 눌렀으므로 생성 해제(제거)
            Destroy(_currentHeldBomb);
            _currentHeldBomb = null;

            Debug.Log("<color=orange>[시스템] 손에 있던 폭탄을 해제(제거)했습니다.</color>");
        }
        else
        {
            // 손에 폭탄이 없거나, 기존 폭탄을 이미 벽에 붙여서 손을 떠난 상태라면 새로 생성
            _currentHeldBomb = Instantiate(bombPrefab, _rightHandTransform.position, _rightHandTransform.rotation);

            _currentHeldBomb.transform.SetParent(_rightHandTransform);
            _currentHeldBomb.transform.localPosition = Vector3.zero;
            _currentHeldBomb.transform.localRotation = Quaternion.identity;

            Debug.Log("<color=lime>[시스템] 새로운 폭탄이 오른손에 생성되었습니다!</color>");
        }
    }
}