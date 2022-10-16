using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Relation : MonoBehaviour
{
    public Entity head;
    public Entity tail;
    public string relationType;
    public RelationJson.Relation definition;
    public bool headTailSwitched = false;

    public Relation(Entity head, Entity tail, string relationType, RelationJson.Relation definition)
    {
        this.head = head;
        this.tail = tail;
        this.relationType = relationType;
        this.definition = definition;
        switchHeadTail();
    }

    private void switchHeadTail()
    {
        if (headCubeOnTailCube())
        {
            Entity head_tmp = head;
            head = tail;
            tail = head_tmp;
            headTailSwitched = true;
        }
    }

    private Vector3 getGapDistanceInbetweetCubes()
    {
        Vector3 anchorPos = definition.head.position.getVector3();
        Vector3 anchorSca = definition.head.scale.getVector3();
        Vector3 runnerPos = definition.tail.position.getVector3();
        Vector3 runnerSca = definition.tail.scale.getVector3();
        Vector3 distanceInbetweenCenter = runnerPos - anchorPos;
        Vector3 headEdge = anchorPos;
        Vector3 tailEdge = runnerPos;
        headEdge.x += distanceInbetweenCenter.x >= 0 ? anchorSca.x*.5f : -anchorSca.x*.5f;
        headEdge.y += distanceInbetweenCenter.y >= 0 ? anchorSca.y*.5f : -anchorSca.y*.5f;
        headEdge.z += distanceInbetweenCenter.z >= 0 ? anchorSca.z*.5f : -anchorSca.z*.5f;
        tailEdge.x += distanceInbetweenCenter.x <= 0 ? runnerSca.x*.5f : -runnerSca.x*.5f;
        tailEdge.y += distanceInbetweenCenter.y <= 0 ? runnerSca.y*.5f : -runnerSca.y*.5f;
        tailEdge.z += distanceInbetweenCenter.z <= 0 ? runnerSca.z*.5f : -runnerSca.z*.5f;
        Vector3 distanceInbetween = tailEdge - headEdge;
        return distanceInbetween;
    }

    private bool cubesAreStacked()
    {
        Vector3 anchorPos = definition.head.position.getVector3();
        Vector3 anchorSca = definition.head.scale.getVector3();
        Vector3 runnerPos = definition.tail.position.getVector3();
        Vector3 runnerSca = definition.tail.scale.getVector3();
        float anchorMinX = anchorPos.x - anchorSca.x*.5f;
        float anchorMaxX = anchorPos.x + anchorSca.x*.5f;
        float anchorMinZ = anchorPos.z - anchorSca.z*.5f;
        float anchorMaxZ = anchorPos.z + anchorSca.z*.5f;
        float runnerMinX = runnerPos.x - runnerSca.x*.5f;
        float runnerMaxX = runnerPos.x + runnerSca.x*.5f;
        float runnerMinZ = runnerPos.z - runnerSca.z*.5f;
        float runnerMaxZ = runnerPos.z + runnerSca.z*.5f;
        return  anchorMinX < runnerMaxX &&
                anchorMaxX > runnerMinX &&
                anchorMaxZ > runnerMinZ &&
                runnerMinZ < runnerMaxZ;
    }

    private bool headCubeOnTailCube()
    {
        return cubesAreStacked() && definition.head.position.getVector3().y > definition.tail.position.getVector3().y;
    }

    public bool isInRelationAndIsStacked(GameObject ghostObject1, GameObject ghostObject2)
    {
        GameObject ghost1 = ghostObject1.transform.parent.gameObject;
        GameObject ghost2 = ghostObject2.transform.parent.gameObject;
        return cubesAreStacked() && 
                ((GameObject.ReferenceEquals(head.ghostModel, ghost1) && GameObject.ReferenceEquals(tail.ghostModel, ghost2)) ||
                ((GameObject.ReferenceEquals(head.ghostModel, ghost2) && GameObject.ReferenceEquals(tail.ghostModel, ghost1))));
    }

    private Vector3 getCenterDistanceInbetweenCubes()
    {
        Vector3 anchorPos = definition.head.position.getVector3();
        Vector3 runnerPos = definition.tail.position.getVector3();
        return runnerPos - anchorPos;
    }

    private Vector3 onTopOfMoveDistances()
    {
        GameObjectHelper.GameObjectBoundries anchorBoundries = GameObjectHelper.getGameObjectBoundries(head.model);

        Vector3 centerDistance = getCenterDistanceInbetweenCubes();
        Vector3 anchorPos = definition.head.position.getVector3();
        Vector3 anchorSca = definition.head.scale.getVector3();
        float anchorRadiusX = anchorSca.x*.5f;
        float anchorRadiusZ = anchorSca.z*.5f;

        float xMoveBy = (anchorBoundries.x_max-anchorBoundries.x_min)/2 * centerDistance.x/anchorRadiusX;
        float zMoveBy = (anchorBoundries.z_max-anchorBoundries.z_min)/2 * centerDistance.z/anchorRadiusZ;
        return new Vector3(xMoveBy, 0, zMoveBy);
    }

    public float calcErrorValue(Vector3 runnerPos)
    {
        
        float errorValue = 0;
        Transform anchor = head.model.transform;

        Vector3 runnerInitPos = getInitialTailPosition(false, false);
        //errorScoreGroundMovement
        float normal = calcErrorScoreGroundMovement(runnerPos, runnerInitPos);
        //errorScoreRotationMovement
        Vector3 runnerInitPosNegativeNegative = getInitialTailPosition(true, true);
        float negativeNegative = calcErrorScoreGroundMovement(runnerPos, runnerInitPosNegativeNegative);
        Vector3 runnerInitPosNegativePositive = getInitialTailPosition(true, false);
        float negativePositive = calcErrorScoreGroundMovement(runnerPos, runnerInitPosNegativePositive);
        Vector3 runnerInitPosPositiveNegative = getInitialTailPosition(false, true);
        float positiveNegative = calcErrorScoreGroundMovement(runnerPos, runnerInitPosPositiveNegative);
        //errorScoreGroundMovement & errorScoreRotationMovement
        float additionalErrorScoreForRotaion = definition.useRotation ? Settings.errorScoreRotationMovement : 0;
        float errorScoreXZ = Mathf.Min( normal, 
                                        negativeNegative + additionalErrorScoreForRotaion, 
                                        negativePositive + additionalErrorScoreForRotaion,
                                        positiveNegative + additionalErrorScoreForRotaion);
        errorValue += errorScoreXZ;
        //errorScorePositiveHeightMovement
        float errorScorePositiveY = errorScorePositiveHeightMovement(runnerPos, runnerInitPos);
        errorValue += errorScorePositiveY;
        //errorScoreNegativeHeightMovement
        float errorScoreNegativeY = calcErrorScoreNegativeHeightMovement(runnerPos, runnerInitPos);
        errorValue += errorScoreNegativeY;
        return errorValue;
    }
    public float calcErrorScoreGroundMovement(Vector3 runnerPos, Vector3 runnerInitPos)
    {
        float xDif = runnerInitPos.x - runnerPos.x;
        float zDif = runnerInitPos.z - runnerPos.z;
        float allowedSpread = definition.useSpread ? (definition.head.spread + definition.tail.spread)/2 : 0;
        float lengthDif = Mathf.Sqrt(Mathf.Pow(xDif, 2) + Mathf.Pow(zDif, 2));
        return  Mathf.Pow(
                Mathf.Max(0, lengthDif 
                - allowedSpread)
                / 0.1f * Settings.errorScoreGroundMovement
                , 2);
    }

    public float errorScorePositiveHeightMovement(Vector3 runnerPos, Vector3 runnerInitPos)
    {
        return runnerPos.y > runnerInitPos.y ? Mathf.Pow(runnerPos.y - runnerInitPos.y, 2) * Settings.errorScorePositiveHeightMovement : 0;
    }

    public float calcErrorScoreNegativeHeightMovement(Vector3 runnerPos, Vector3 runnerInitPos)
    {
        return runnerPos.y < runnerInitPos.y ? Settings.errorScoreNegativeHeightMovement : 0;
    }

    // Move tail in relation to the head Entity
    public Vector3 getInitialTailPosition(bool invertPosX, bool invertPosZ)
    {
        GameObject anchorModel = head.model;
        GameObject runnterModel = tail.model;
        float ground_y = 0;
        float x;
        float y;
        float z;
        Vector3 anchorVec3 = anchorModel.transform.position;
        Vector3 runnerVec3 = runnterModel.transform.position;
        GameObjectHelper.GameObjectBoundries anchorBoundries = GameObjectHelper.getGameObjectBoundries(anchorModel);
        GameObjectHelper.GameObjectBoundries runnerBoundries = GameObjectHelper.getGameObjectBoundries(runnterModel);
        Vector3 gapDistance = getGapDistanceInbetweetCubes();
        Vector3 centerDistance = getCenterDistanceInbetweenCubes();
        
        int modX = invertPosX ? -1 : 1;
        int modZ = invertPosZ ? -1 : 1;
        gapDistance.x *= modX;
        centerDistance.x *= modX;
        gapDistance.z *= modZ;
        centerDistance.z *= modZ;

        if (cubesAreStacked())
        {
            Vector3 additionalMoveDistance = onTopOfMoveDistances();
            float xAnchorMid = anchorBoundries.x_min + (anchorBoundries.x_max - anchorBoundries.x_min)/2;
            float xRunnerMid = runnerBoundries.x_min + (runnerBoundries.x_max - runnerBoundries.x_min)/2;
            x = runnerVec3.x - (xRunnerMid - xAnchorMid) + additionalMoveDistance.x;
            y = anchorBoundries.y_max + (runnerVec3.y - runnerBoundries.y_min);
            float zAnchorMid = anchorBoundries.z_min + (anchorBoundries.z_max - anchorBoundries.z_min)/2;
            float zRunnerMid = runnerBoundries.z_min + (runnerBoundries.z_max - runnerBoundries.z_min)/2;
            z = runnerVec3.z - (zRunnerMid - zAnchorMid) + additionalMoveDistance.z;
        }
        else
        {
            if (centerDistance.x > 0 )
            {
                x = (anchorBoundries.x_max - anchorVec3.x) 
                    + (runnerVec3.x - runnerBoundries.x_min) 
                    + gapDistance.x
                    + anchorVec3.x;
            }
            else if (centerDistance.x < 0)
            {
                x = - (runnerBoundries.x_max - runnerVec3.x) 
                    - (anchorVec3.x - anchorBoundries.x_min) 
                    - modX * gapDistance.x
                    + anchorVec3.x;
            }
            else
            {
                x = anchorVec3.x;
            }
            if (centerDistance.z > 0)
            {
                z = (anchorBoundries.z_max - anchorVec3.z) 
                    + (runnerVec3.z - runnerBoundries.z_min) 
                    + gapDistance.z
                    + anchorVec3.z;
            }
            else if (centerDistance.z < 0)
            {
                z = - (runnerBoundries.z_max - runnerVec3.z) 
                    - (anchorVec3.z - anchorBoundries.z_min) 
                    - gapDistance.z
                    + anchorVec3.z;
            }
            else
            {
                z = anchorVec3.z;
            }
            y = ground_y + (runnerVec3.y - runnerBoundries.y_min);
        }
        return new Vector3(x,y,z);
    }
}
