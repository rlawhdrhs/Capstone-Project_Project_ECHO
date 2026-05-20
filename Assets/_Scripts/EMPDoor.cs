using UnityEngine;

public class EMPDoor : MonoBehaviour
{
    private Animator _animator;
    private bool _isOpened = false;

    void Awake()
    {
        _animator = GetComponent<Animator>();
        
        if (_animator == null)
        {
            Debug.LogError($"{gameObject.name}에 Animator가 없습니다! 애니메이션을 추가해주세요.");
        }
    }

    public void OpenDoor()
    {
        if (_isOpened) return; 

        _isOpened = true;
        
        _animator.SetTrigger("Open");

        Debug.Log($"<color=blue>{gameObject.name} 문이 EMP 충격으로 개방되었습니다!</color>");
    }
}