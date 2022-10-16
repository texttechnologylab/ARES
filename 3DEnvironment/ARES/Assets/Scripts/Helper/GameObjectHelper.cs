using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class GameObjectHelper : MonoBehaviour
{
    public class GameObjectBoundries
    {
        public float x_min;
        public float x_max;
        public float y_min;
        public float y_max;
        public float z_min;
        public float z_max;

        public GameObjectBoundries(float x_min, float x_max, float y_min, float y_max, float z_min, float z_max)
        {
            this.x_min = x_min;
            this.x_max = x_max;
            this.y_min = y_min;
            this.y_max = y_max;
            this.z_min = z_min;
            this.z_max = z_max;
        }

        public void modelMovedByVector(Vector3 addVector)
        {
            this.x_min += addVector.x;
            this.x_max += addVector.x;
            this.y_min += addVector.y;
            this.y_max += addVector.y;
            this.z_min += addVector.z;
            this.z_max += addVector.z;
        }
    }

    public static Transform[] getAllSubTransforms(GameObject gameObject)
    {
        Transform[] allTransforms = gameObject.GetComponents<Transform>();
        List<GameObject> allChildren = getAllChildGameObjects(gameObject);
        Transform[] subTransforms = getAllSubTransformsHelper(allChildren);
        allTransforms = allTransforms.Concat(subTransforms).ToArray();
        return allTransforms;
    }

    private static Transform[] getAllSubTransformsHelper(List<GameObject> gameObjects)
    {
        Transform[] allTransforms = new Transform[0];
        foreach (GameObject gameObject in gameObjects)
        {
            Transform[] childTransforms = gameObject.GetComponents<Transform>();
            allTransforms = allTransforms.Concat(childTransforms).ToArray();

            List<GameObject> allChildren = getAllChildGameObjects(gameObject);;
            Transform[] subTransforms = getAllSubTransformsHelper(allChildren);
            allTransforms = allTransforms.Concat(subTransforms).ToArray();
        }
        return allTransforms;
    }

    public static List<GameObject> getAllChildGameObjects(GameObject gameObject)
    {
        List<GameObject> childs = new List<GameObject>();
        for (int i = 0; i < gameObject.transform.childCount; i++)
        {
            GameObject child = gameObject.transform.GetChild(i).gameObject;
            childs.Add(child);
        }
        return childs;
    }

    public static GameObjectBoundries getGameObjectBoundries(GameObject gameObject)
    {
        Transform[] allChildren = getAllSubTransforms(gameObject);

        float x_min = float.PositiveInfinity;
        float x_max = float.NegativeInfinity;
        float y_min = float.PositiveInfinity;
        float y_max = float.NegativeInfinity;
        float z_min = float.PositiveInfinity;
        float z_max = float.NegativeInfinity;

        foreach (Transform child in allChildren)
        {
            Renderer childRenderer = child.gameObject.GetComponent<Renderer>();
            if (childRenderer != null && childRenderer.enabled){
                Bounds childBounds = childRenderer.bounds;
                x_min = Mathf.Min(x_min, childBounds.min.x);
                x_max = Mathf.Max(x_max, childBounds.max.x);
                y_min = Mathf.Min(y_min, childBounds.min.y);
                y_max = Mathf.Max(y_max, childBounds.max.y);
                z_min = Mathf.Min(z_min, childBounds.min.z);
                z_max = Mathf.Max(z_max, childBounds.max.z);
            }
        }
        return new GameObjectBoundries(x_min, x_max, y_min, y_max, z_min, z_max);
    }

    public static Vector3 centerObjectOnVec(GameObject gameObject, Vector3 center)
    {
        Vector3 origPos = gameObject.transform.position;
        GameObjectBoundries goBoundries = getGameObjectBoundries(gameObject);
        float goMidX = goBoundries.x_min + (goBoundries.x_max - goBoundries.x_min)/2;
        float centeredX =  origPos.x + center.x - goMidX;
        float goMidY = goBoundries.y_min + (goBoundries.y_max - goBoundries.y_min)/2;
        float centeredY =  origPos.y + center.y - goMidY;
        float goMidZ = goBoundries.z_min + (goBoundries.z_max - goBoundries.z_min)/2;
        float centeredZ =  origPos.z + center.z - goMidZ;
        return new Vector3(centeredX, centeredY, centeredZ);
    }

    public static Vector3 centerObjectOnGroundPoint(GameObject gameObject, Vector3 center)
    {
        Vector3 origPos = gameObject.transform.position;
        Vector3 centeredVec = centerObjectOnVec(gameObject, center);
        GameObjectBoundries goBoundries = getGameObjectBoundries(gameObject);
        float y = origPos.y + center.y - goBoundries.y_min;
        return new Vector3(centeredVec.x, y, centeredVec.z);
    }

    public static bool objectsOverlappingOnHeight(GameObject anchor, GameObject runner, Vector3 addVector)
    {
        GameObjectBoundries anchorBoundries = getGameObjectBoundries(anchor);
        GameObjectBoundries runnerBoundries = getGameObjectBoundries(runner);
        runnerBoundries.modelMovedByVector(addVector);
        return  anchorBoundries.x_min < runnerBoundries.x_max &&
                anchorBoundries.x_max > runnerBoundries.x_min &&
                anchorBoundries.z_max > runnerBoundries.z_min &&
                anchorBoundries.z_min < runnerBoundries.z_max;
    }
}
