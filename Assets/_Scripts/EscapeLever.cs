using UnityEngine;
using UnityEngine.Events;

public class EscapeLever : MonoBehaviour
{
    private HingeJoint _hingeJoint;
    private bool _isEscaped = false;

    public float targetAngle = 40f; 

    public UnityEvent onLeverPulled; 

    void Start()
    {
        _hingeJoint = GetComponent<HingeJoint>();
    }

    void Update()
    {
        if (_isEscaped) return;

        float currentAngle = Mathf.Abs(_hingeJoint.angle);

        if (currentAngle >= targetAngle)
        {
            _isEscaped = true;
            
            // 연결된 탈출 함수 실행
            onLeverPulled.Invoke(); 
        }
    }
}