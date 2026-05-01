using UnityEngine;

public class MissionInteractable : MonoBehaviour
{
    [Header("Linked Mission")]
    [SerializeField] private MissionBase linkedMission;

    public void Interact()
    {
        if (MissionManager.Instance == null)
        {
            Debug.LogWarning("[Mission] MissionManager is missing.");
            return;
        }

        if (linkedMission == null)
        {
            Debug.LogWarning("[Mission] Linked mission is not assigned.");
            return;
        }

        if (!MissionManager.Instance.IsCurrentMission(linkedMission))
        {
            Debug.Log("[Mission] This is not the current mission.");
            return;
        }

        if (!linkedMission.CanInteract())
        {
            Debug.Log("[Mission] Interaction is not available.");
            return;
        }

        OnInteractSuccess();
    }

    protected virtual void OnInteractSuccess()
    {
        Debug.Log($"[Mission] Interacted with {gameObject.name}");
        MissionManager.Instance.CompleteCurrentMission();
    }
}