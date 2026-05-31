using System.Collections.Generic;
using Fusion;
using UnityEngine;

public class NetworkSensorManager : NetworkBehaviour
{
    public static NetworkSensorManager Instance;

    public List<NetworkSoundSensor> sensors = new List<NetworkSoundSensor>();
    public NetworkSoundSensor currentControlledSensor;
    public Camera chaserCamera; // 추격자의 메인 카메라

    private void Awake()
    {
        Instance = this;
    }

    public void RegisterChaserCamera(Camera localCam)
    {
        chaserCamera = localCam;
    }

    public void RegisterSensor(NetworkSoundSensor sensor)
    {
        if (!sensors.Contains(sensor))
        {
            sensors.Add(sensor);
        }
    }

    public void SwitchToSensorById(int id)
    {
        if (!Runner.IsClient) return;

        NetworkSoundSensor targetSensor = null;
        foreach (var sensor in sensors)
        {
            if (sensor.sensorId == id) targetSensor = sensor;
        }
        if (targetSensor == null) return;

        NetworkSoundSensor prevSensor = currentControlledSensor;

        VRRigSynchronizer humanRig = null;

        if (NetworkManager.Instance != null && NetworkManager.Instance.ChaserObject != null)
        {
            humanRig = NetworkManager.Instance.ChaserObject.GetComponent<VRRigSynchronizer>();
        }

        if (humanRig == null)
        {
            VRRigSynchronizer[] allRigs = FindObjectsOfType<VRRigSynchronizer>();
            foreach (var rig in allRigs)
            {
                if (!rig.isSensorRobot && rig.Object.HasInputAuthority)
                {
                    humanRig = rig;
                    break;
                }
            }
        }

        if (humanRig != null)
        {
            humanRig.localFreeze = true; // 로컬 즉시 정지

            CharacterController humanCC = humanRig.GetComponent<CharacterController>();
            if (humanCC != null) humanCC.enabled = false; // 물리 즉시 정지

            //humanRig.RPC_SetFrozenState(true, humanRig.transform.position); 
            Debug.Log("추격자 아바타 정상 박제 완료!");
        }
        else
        {
            Debug.LogError("🚨 추격자 아바타를 찾지 못했습니다! 아바타가 로봇으로 같이 텔레포트 할 수 있습니다.");
        }

        if (currentControlledSensor != null) currentControlledSensor.RPC_ReleaseControl();
        currentControlledSensor = targetSensor;

        Debug.Log($"[클라이언트] 서버에게 {targetSensor.gameObject.name} 조종권을 요청합니다. 내 PlayerRef: {Runner.LocalPlayer}");

        currentControlledSensor.RPC_RequestControl(Runner.LocalPlayer);

        if (LocalVRRig.Instance != null && targetSensor.cameraPoint != null)
        {
            Transform vrRig = LocalVRRig.Instance.transform;
            Transform hardwareHead = LocalVRRig.Instance.hardwareHead;

            CharacterController xrCC = vrRig.GetComponent<CharacterController>();
            if (xrCC != null) xrCC.enabled = false;

            Physics.SyncTransforms();

            vrRig.rotation = targetSensor.cameraPoint.rotation;

            Vector3 eyeOffset = hardwareHead.position - vrRig.position;
            vrRig.position -= eyeOffset;

            Physics.SyncTransforms();

            if (xrCC != null) xrCC.enabled = true;

            // 로봇에 내 영혼(IK) 연결
            VRRigSynchronizer robotRig = targetSensor.GetComponent<VRRigSynchronizer>();
            if (robotRig != null)
            {
                robotRig.localFreeze = false;
                targetSensor.RPC_RequestControl(Runner.LocalPlayer);

                //robotRig.RPC_SetFrozenState(false, robotRig.transform.position);

                LocalVRRig.Instance.avatarRoot = targetSensor.transform;
                LocalVRRig.Instance.avatarHead = robotRig.avatarHead;
                LocalVRRig.Instance.avatarLeftHand = robotRig.avatarLeftHand;
                LocalVRRig.Instance.avatarRightHand = robotRig.avatarRightHand;
                LocalVRRig.Instance.animator = robotRig.animator;

            }

            if (prevSensor != null && prevSensor != targetSensor)
            {
                VRRigSynchronizer prevRig = prevSensor.GetComponent<VRRigSynchronizer>();
                if (prevRig != null)
                {
                    prevRig.localFreeze = true;
                    //prevRig.RPC_SetFrozenState(true, prevRig.transform.position);
                }
            }
        }
    }
}