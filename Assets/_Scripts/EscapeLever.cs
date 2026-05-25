using UnityEngine;
using UnityEngine.Events;

public class EscapeLever : MonoBehaviour
{
    private HingeJoint _hingeJoint;
    private Rigidbody _rb;
    private bool _isEscaped = false;

    [Header("Angle Settings")]
    public float targetAngle = 120f;
    public bool invertDirection = false;
    public UnityEvent onLeverPulled;

    private Transform _pullingHand;
    private Transform _leverBase;
    private float _startHandAngle;
    private float _startLeverAngle;

    // ★ X축 고정을 위한 초기 각도 저장 변수
    private float _initialLocalY;
    private float _initialLocalZ;

    void Start()
    {
        _hingeJoint = GetComponent<HingeJoint>();
        _rb = GetComponent<Rigidbody>();
        _leverBase = transform.parent != null ? transform.parent : transform;

        // ★ 시작할 때의 고유한 Y, Z 회전값을 백업해 둡니다.
        Vector3 currentEuler = transform.localEulerAngles;
        _initialLocalY = currentEuler.y;
        _initialLocalZ = currentEuler.z;
    }

    private Vector3 GetLeverPivotInParentSpace()
    {
        if (_hingeJoint != null)
        {
            return transform.localPosition + _hingeJoint.anchor;
        }
        return transform.localPosition;
    }

    public void StartPull(Transform hand)
    {
        if (_isEscaped) return;

        _pullingHand = hand;
        _rb.isKinematic = true;

        Vector3 handPosInParent = _leverBase.InverseTransformPoint(_pullingHand.position);
        Vector3 pivotInParent = GetLeverPivotInParentSpace();
        Vector3 directionToHand = handPosInParent - pivotInParent;

        // X축 회전이므로 Y, Z 평면의 변화량만 정밀하게 추출합니다.
        _startHandAngle = Mathf.Atan2(directionToHand.y, directionToHand.z) * Mathf.Rad2Deg;
        _startLeverAngle = _hingeJoint.angle;
    }

    public void EndPull()
    {
        _pullingHand = null;
        if (!_isEscaped)
        {
            _rb.isKinematic = false;
        }
    }

    void Update()
    {
        if (_pullingHand != null && !_isEscaped)
        {
            Vector3 handPosInParent = _leverBase.InverseTransformPoint(_pullingHand.position);
            Vector3 pivotInParent = GetLeverPivotInParentSpace();
            Vector3 directionToHand = handPosInParent - pivotInParent;

            float currentHandAngle = Mathf.Atan2(directionToHand.y, directionToHand.z) * Mathf.Rad2Deg;
            float deltaAngle = currentHandAngle - _startHandAngle;

            if (invertDirection)
            {
                deltaAngle = -deltaAngle;
            }

            float targetJointAngle = _startLeverAngle + deltaAngle;

            if (_hingeJoint.useLimits)
            {
                targetJointAngle = Mathf.Clamp(targetJointAngle, _hingeJoint.limits.min, _hingeJoint.limits.max);
            }

            // ★ 핵심: Y와 Z는 시작할 때 각도로 꽉 묶어두고, X축만 부드럽게 대입합니다.
            transform.localEulerAngles = new Vector3(targetJointAngle, _initialLocalY, _initialLocalZ);
        }

        if (_isEscaped) return;

        float currentAngle = Mathf.Abs(_hingeJoint.angle);

        if (currentAngle >= targetAngle)
        {
            _isEscaped = true;
            _rb.isKinematic = true;
            _pullingHand = null;

            onLeverPulled.Invoke();
            Debug.Log($"<color=yellow>[레버] {targetAngle}도 도달 완료! 탈출 이벤트 작동.</color>");
        }
    }
}