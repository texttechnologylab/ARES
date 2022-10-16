using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DragDropParameters : MonoBehaviour
{
    public static List<GameObject> entityList = new List<GameObject>();
    public static List<GameObject> relationList = new List<GameObject>();
    public static bool droppedInSceneBox = false;
    public static GameObject currentEntity;
    public static GameObject currentRelationHead;
    public static GameObject currentRelationTail;
    public static ExtractJsonScene.Scene currScene;
    public static bool newEntityImageSearched = true;
}
