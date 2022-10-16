using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectInteraction : MonoBehaviour
{
    public Transform userPointer;
    private Transform scene;
    private Transform model;

    void OnMouseDown() 
    {
        model = this.transform.parent.parent;
        scene = model.parent;

        if (Input.GetKey(KeyCode.LeftShift))
        {
            GetComponent<Rigidbody>().useGravity = false;
            this.transform.position = userPointer.position;
            model.parent = userPointer.transform;
        }
        else
        {
            if (this.gameObject.tag == "inScene")
            {
                foreach (Entity entity in GlobalVariable.entitiesScene)
                {   
                    if (GameObject.ReferenceEquals(entity.model, this.gameObject.transform.parent.gameObject))
                    {
                        GameObject user = GameObject.Find("User");
                        CustomSceneCreator csc = user.GetComponent<CustomSceneCreator>();
                        csc.enterInfo(entity);
                        break;
                    }
                }
            }
        }
    }
    void OnMouseUp()
    {
        GetComponent<Rigidbody>().useGravity = true;
        model.parent = scene;
    }

    private void showObjectInfo()
    {
        GameObject userCanvas = GameObject.Find("UserCanvas");
    }
}
