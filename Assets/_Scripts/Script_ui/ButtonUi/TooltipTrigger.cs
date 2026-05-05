using UnityEngine;

public class TooltipTrigger : MonoBehaviour
{
    public GameObject tooltip;

    void Start()
    {
        if (tooltip != null)
            tooltip.SetActive(false);
    }

    void OnMouseEnter()
    {
        if (tooltip != null)
            tooltip.SetActive(true);
    }

    void OnMouseExit()
    {
        if (tooltip != null)
            tooltip.SetActive(false);
    }
}