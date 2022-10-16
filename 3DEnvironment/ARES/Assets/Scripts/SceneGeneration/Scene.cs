using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Scene : MonoBehaviour
{
    public GameObject sceneBucket;
    public List<Entity> entities = new List<Entity>();
    public float sceneErrorScore = 0;
    public bool inScene = false;

    public Scene(int sceneNr)
    {
        sceneBucket = new GameObject("sceneObj" + sceneNr.ToString());
    }

    public void AddEntity(Entity entity)
    {
        entities.Add(entity);
        entity.model.transform.parent.parent = sceneBucket.transform;
    }

    public List<int> getIndexList()
    {
        List<int> result = new List<int>();
        foreach (Entity entity in entities)
        {
            result.Add(entity.index);
        }
        return result;
    }

    public int getEntityWithIndexAmount(int searchIndex)
    {
        int result = 0;
        foreach (int index in getIndexList())
        {
            if (index == searchIndex)
            {
                result++;
            }
        }
        return result;
    }

    public bool containsEntityInstance(Entity searchEntity)
    {
        foreach (Entity entity in entities)
        {
            if (GameObject.ReferenceEquals(searchEntity.model, entity.model))
            {
                return true;
            }
        }
        return false;
    }

    public void setAllGhostObjectPos()
    {
        foreach (Entity ent in entities)
        {
            ent.resetGhostModelPos();
        }
    }
}
