using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SceneItems : MonoBehaviour, IDropHandler
{
    public GameObject canvasEntityInfo;
    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null)
        {
            if (eventData.pointerDrag.transform.parent != this.gameObject.transform && eventData.pointerDrag.name != "RelationCircle")
            {
                eventData.pointerDrag.transform.parent = this.gameObject.transform;
                canvasEntityInfo.SetActive(true);
                DragDropParameters.currentEntity = eventData.pointerDrag;
            }
        }
    }
}
