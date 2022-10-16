using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Entity : MonoBehaviour
{
    public GameObject model;
    public GameObject ghostModel;
    public ShapeNetModel modelInfo;
    public string entityName;
    public int index;
    public int copyNr;

    public Entity(ShapeNetModel info, int idx, string name, int number)
    {
        modelInfo = info;
        index = idx;
        entityName = name;
        copyNr = number;
    }

    public void resetGhostModelPos()
    {
        ghostModel.transform.position = model.transform.position;
        ghostModel.transform.rotation = model.transform.rotation;

        GameObject modelMeshObj = GameObjectHelper.getAllChildGameObjects(model)[0];
        GameObject ghostMeshObj = GameObjectHelper.getAllChildGameObjects(ghostModel)[0];

        ghostMeshObj.transform.position = modelMeshObj.transform.position;
        ghostMeshObj.transform.rotation = modelMeshObj.transform.rotation;

        Rigidbody rb = ghostModel.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }
       
    }
}
