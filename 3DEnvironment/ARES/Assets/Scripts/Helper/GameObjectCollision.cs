using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameObjectCollision : MonoBehaviour
{
    void OnCollisionStay(Collision collisionInfo)
    {
        GameObject gameObject = collisionInfo.gameObject;
        if (gameObject.tag != "Ground")
        {
            Rigidbody rb1 = collisionInfo.gameObject.GetComponent<Rigidbody>();
            rb1.velocity = Vector3.zero;
            rb1.angularVelocity = Vector3.zero; 
            Rigidbody rb2 = gameObject.GetComponent<Rigidbody>();
            rb2.velocity = Vector3.zero;
            rb2.angularVelocity = Vector3.zero; 
            if (collisionInfo.gameObject.tag == "inScene"
                && gameObject.tag == "inScene")
            {
                bool validCollisionOccured = true;
                //allow collision range
                if (collisionOccuredInAllowedRange(this.gameObject, gameObject))
                {
                    validCollisionOccured = false;
                }

                if (validCollisionOccured)
                {
                    GlobalVariable.collisionOccured = true;
                }   
            }
        }
    }

    private bool collisionOccuredInAllowedRange(GameObject gameObject1, GameObject gameObject2)
    {
        GameObjectHelper.GameObjectBoundries gameObject1Boundries = GameObjectHelper.getGameObjectBoundries(gameObject1);
        GameObjectHelper.GameObjectBoundries gameObject2Boundries = GameObjectHelper.getGameObjectBoundries(gameObject2);
        if (gameObject1.transform.position.y > gameObject2.transform.position.y)
        {
            return Mathf.Abs(gameObject1Boundries.y_min - gameObject2Boundries.y_max) < GlobalVariable.allowedCollisionRangeFix;
        }
        else
        {
            return Mathf.Abs(gameObject2Boundries.y_min - gameObject1Boundries.y_max) < GlobalVariable.allowedCollisionRangeFix;
        }
    }
}
