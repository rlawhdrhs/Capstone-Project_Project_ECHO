using UnityEngine;

public class EMPDoor : MonoBehaviour
{
    // 폭탄이 터질 때 호출될 함수
    public void OpenDoor()
    {
        Debug.Log($"⚡ EMP 감지! {gameObject.name} 시스템 해킹 완료.");
        
        gameObject.SetActive(false); 
    }
}