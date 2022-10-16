using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentAnimation : MonoBehaviour
{
    public Transform headCube;
    public Transform tailCube;
    private LineRenderer lineRenderer;
    private float distance;
    private float counter;
    private float lineDrawSpeed = 6f;

    private Rect headRect = new Rect(0, 0, 300, 100);
    private Rect tailRect = new Rect(0, 0, 300, 100);
    private Rect distanceInbetweenRect = new Rect(0, 0, 300, 100);
    private GUIStyle headerStyle;

    private Vector3 getDistanceInbetweetCubes()
    {
        Vector3 distanceInbetweenCenter = tailCube.position - headCube.position;
        Vector3 headEdge = headCube.position;
        Vector3 tailEdge = tailCube.position;
        headEdge.x += distanceInbetweenCenter.x >= 0 ? headCube.localScale.x*.5f : -headCube.localScale.x*.5f;
        headEdge.y += distanceInbetweenCenter.y >= 0 ? headCube.localScale.y*.5f : -headCube.localScale.y*.5f;
        headEdge.z += distanceInbetweenCenter.z >= 0 ? headCube.localScale.z*.5f : -headCube.localScale.z*.5f;
        tailEdge.x += distanceInbetweenCenter.x <= 0 ? tailCube.localScale.x*.5f : -tailCube.localScale.x*.5f;
        tailEdge.y += distanceInbetweenCenter.y <= 0 ? tailCube.localScale.y*.5f : -tailCube.localScale.y*.5f;
        tailEdge.z += distanceInbetweenCenter.z <= 0 ? tailCube.localScale.z*.5f : -tailCube.localScale.z*.5f;
        Vector3 distanceInbetween = tailEdge - headEdge;
        return distanceInbetween;
    }

    // Update is called once per frame
    void Update()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.SetPosition(0, headCube.position);
        lineRenderer.SetWidth(.05f, .05f);
        distance = Vector3.Distance(headCube.position, tailCube.position);

        counter += .1f / lineDrawSpeed;
        float x = Mathf.Lerp(0, distance, counter);

        Vector3 pointHead = headCube.position;
        Vector3 pointTail = tailCube.position;

        Vector3 pointAlongLine = x * Vector3.Normalize(pointTail - pointHead) + pointHead;
        lineRenderer.SetPosition(1, pointAlongLine);
    }

     void OnGUI()
    {
        headerStyle = new GUIStyle(GUI.skin.label);
        headerStyle.alignment = TextAnchor.MiddleCenter;
        headerStyle.normal.textColor = Color.green;

        Vector3 headPoint = Camera.main.WorldToScreenPoint(headCube.position);
        headRect.x = headPoint.x - headRect.width/2;
        headRect.y = Screen.height - headPoint.y - headRect.height;

        Vector3 tailPoint = Camera.main.WorldToScreenPoint(tailCube.position);
        tailRect.x = tailPoint.x - tailRect.width/2;
        tailRect.y = Screen.height - tailPoint.y - tailRect.height;

        Vector3 middle = (headCube.position + tailCube.position) / 2;
        Vector3 middlePoint = Camera.main.WorldToScreenPoint(middle);
        distanceInbetweenRect.x = middlePoint.x - distanceInbetweenRect.width/2;
        distanceInbetweenRect.y = Screen.height - middlePoint.y - distanceInbetweenRect.height;

        GUI.Label(headRect, headCube.position.ToString(), headerStyle);
        GUI.Label(tailRect, tailCube.position.ToString(), headerStyle);
        GUI.Label(distanceInbetweenRect, getDistanceInbetweetCubes().ToString(), headerStyle);
    }
}
