using UnityEngine;
using UnityEngine.EventSystems;

public class TechnologyNodeButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private TechnologyNodeData _nodeData;

    public void Initialize(TechnologyNodeData nodeData)
    {
        _nodeData = nodeData;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        EventManager.TriggerEvent("HoveredTechnologyNode", _nodeData);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        EventManager.TriggerEvent("UnhoveredTechnologyNode");
    }
}