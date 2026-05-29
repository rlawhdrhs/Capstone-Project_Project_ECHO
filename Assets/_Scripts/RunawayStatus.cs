using UnityEngine;
using System.Collections;

public class RunawayStatus : MonoBehaviour
{
    [Header("이동 제어기")]
    [Tooltip("생존자의 조이스틱 이동을 담당하는 스크립트를 끌어다 넣으세요.")]
    public Behaviour moveProvider; 

    public void ApplyStun(float duration)
    {
        // 혹시 이미 기절해 있는 상태에서 또 전기 충격을 맞으면, 
        // 타이머가 꼬이지 않게 이전 기절 코루틴을 취소하고 처음부터 다시 셉니다.
        StopAllCoroutines();
        StartCoroutine(StunRoutine(duration));
    }

    private IEnumerator StunRoutine(float duration)
    {
        if (moveProvider != null)
        {
            Debug.Log($"😵 [생존자] 으악! 감전되었습니다! {duration}초 동안 이동 불가!");

            if (IntruderStatusUIManager.Instance != null)
            {
                IntruderStatusUIManager.Instance.SetStunStatus(true);
            }

            // 조이스틱 이동 기능을 담당하는 컴포넌트를 강제로 꺼버림 (발 묶기)
            moveProvider.enabled = false; 
            
            // 설정된 기절 시간(초)만큼 대기
            yield return new WaitForSeconds(duration); 
            
            // 대기 시간이 끝나면 이동 기능을 다시 켜줌
            moveProvider.enabled = true;

            if (IntruderStatusUIManager.Instance != null)
            {
                IntruderStatusUIManager.Instance.SetStunStatus(false);
            }

            Debug.Log("🏃 [생존자] 기절이 풀렸습니다! 다시 도망칩니다!");
        }
        else
        {
            // 스크립트 연결을 깜빡하셨을 때 콘솔창에 띄워주는 친절한 경고문
            Debug.LogWarning("⚠️ [생존자] 감전 신호는 받았는데, 'Move Provider'가 연결되어 있지 않아 발을 묶을 수 없습니다!");
        }
    }
}