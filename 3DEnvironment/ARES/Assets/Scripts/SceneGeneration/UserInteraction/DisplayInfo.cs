using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DisplayInfo : MonoBehaviour
{
    public GameObject userCam;
    public GameObject userPointer;

    public TMP_Text foundObject;

    // Update is called once per frame
    void Update()
    {
        RaycastHit rHit;
        Ray landingRay = new Ray(userCam.transform.position,userPointer.transform.position-userCam.transform.position);

        foundObject.text = "";
        if (Physics.Raycast(landingRay, out rHit, 5))
        {
            if (rHit.collider.tag == "inScene")
            {
                foreach (Entity entity in GlobalVariable.entitiesScene)
                {
                    if (GameObject.ReferenceEquals(entity.model, rHit.collider.transform.parent.gameObject))
                    {
                        foundObject.text = entity.entityName + " " + (entity.index + (entity.copyNr == -1 ? "" : '-' + entity.copyNr.ToString()));
                        break;
                    }
                }
                
            }
        }
    }
}
