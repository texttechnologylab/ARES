using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RelationLine : MonoBehaviour
{
    public ExtractJsonScene.Relation relation;
    public GameObject head;
    public GameObject tail;
    public LineRenderer lineRenderer;
    private float distance;
    private float counter;
    private float lineDrawSpeed = 6f;
    private Rect distanceInbetweenRect = new Rect(0, 0, 300, 100);

    // Start is called before the first frame update
    void Start()
    {
        lineRenderer = this.gameObject.GetComponent<LineRenderer>();
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 pointHead = head.transform.position;
        pointHead.z -= 1;
        Vector3 pointTail = tail.transform.position;
        pointTail.z -= 1;
        lineRenderer.SetPosition(0, pointHead);
        lineRenderer.SetWidth(.5f, .05f);
        distance = Vector3.Distance(pointHead, pointTail);

        counter += .1f / lineDrawSpeed;
        float x = Mathf.Lerp(0, distance, counter);

        Vector3 pointAlongLine = x * Vector3.Normalize(pointTail - pointHead) + pointHead;
        lineRenderer.SetPosition(1, pointAlongLine);
    }

    void OnGUI()
    {
        DragAndDrop dad = head.GetComponent<DragAndDrop>();
        if (relation != null && dad != null)
        {
            if (!dad.entityCanvas.active && !dad.relationCavnas.active)
            {
                Vector3 middle = (head.transform.position + tail.transform.position) / 2;
                Vector3 middlePoint = Camera.main.WorldToScreenPoint(middle);
                distanceInbetweenRect.x = middlePoint.x - distanceInbetweenRect.width/2;
                distanceInbetweenRect.y = Screen.height - middlePoint.y - distanceInbetweenRect.height;

                GUIStyle headerStyle = new GUIStyle(GUI.skin.label);
                headerStyle.alignment = TextAnchor.MiddleCenter;
                headerStyle.normal.textColor = Color.white;
                GUI.Label(distanceInbetweenRect, relation.type, headerStyle);
            }
        }
    }
}
