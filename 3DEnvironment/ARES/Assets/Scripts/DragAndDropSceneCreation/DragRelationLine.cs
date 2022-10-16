using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using TMPro;
using UnityEngine.UI;

public class DragRelationLine : MonoBehaviour, IBeginDragHandler, IEndDragHandler, IDragHandler, IDropHandler
{
    private RectTransform rectTransform;
    private CanvasGroup canvasGroup;
    [SerializeField] private Canvas canvas;
    private Vector3 originalPos;

    private void Awake() 
    {
        if (canvas == null)
        {
            canvas = GameObject.Find("Canvas").GetComponent<Canvas>();
        }
        rectTransform = GetComponent<RectTransform>();
        canvasGroup = GetComponent<CanvasGroup>();
        originalPos = this.gameObject.transform.localPosition;
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        this.gameObject.transform.parent.GetComponent<DragAndDrop>().circleIsDragged = true;
        this.gameObject.transform.parent.SetSiblingIndex(0);
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        this.gameObject.transform.parent.GetComponent<DragAndDrop>().circleIsDragged = false;
        this.gameObject.transform.localPosition = originalPos;
    }

    public void OnDrop(PointerEventData eventData)
    {
        if (eventData.pointerDrag != null && !GameObject.ReferenceEquals(eventData.pointerDrag, this.gameObject))
        {
            if (eventData.pointerDrag.name == "RelationCircle")
            {
                GameObject currentRelationHead = eventData.pointerDrag.transform.parent.gameObject;
                GameObject currentRelationTail = this.gameObject.transform.parent.gameObject;
                ExtractJsonScene.Entity headEntity = currentRelationHead.GetComponent<DragAndDrop>().entity;
                ExtractJsonScene.Entity tailEntity = currentRelationTail.GetComponent<DragAndDrop>().entity;
                if (headEntity != null && tailEntity != null)
                {
                    DragDropParameters.currentRelationHead = currentRelationHead;
                    DragDropParameters.currentRelationTail = currentRelationTail;
                    GameObject relationCavnas = this.gameObject.transform.parent.GetComponent<DragAndDrop>().relationCavnas;
                    relationCavnas.SetActive(true);
                    relationCavnas.transform.Find("RelationInfo").Find("HeadInfo").gameObject.GetComponent<TMP_Text>().text = headEntity.index.ToString() + ": " + headEntity.lemma;
                    relationCavnas.transform.Find("RelationInfo").Find("TailInfo").gameObject.GetComponent<TMP_Text>().text = tailEntity.index.ToString() + ": " + tailEntity.lemma;
                    relationCavnas.transform.Find("RelationInfo").Find("TypeInputField").gameObject.GetComponent<InputField>().text ="";
                }
            }
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }
}
