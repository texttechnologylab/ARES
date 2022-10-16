using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class SubScene : MonoBehaviour
{
    [System.Serializable]
    public class Vec3D
    {
        public float x;
        public float y;
        public float z;

        public Vector3 getVector3()
        {
            return new Vector3(x, y, z);
        }

        public void setVector3(Vector3 vec3)
        {
            x = vec3.x;
            y = vec3.y;
            z = vec3.z;
        }
    }

        
    [System.Serializable]
    public class Entity
    {
        public string lemma;
        public string id;
        public bool isAnchor;
        public Vec3D position;
        public Vec3D rotation;
        public Vec3D scale;
    }
    [System.Serializable]
    public class Relation
    {
        public int head;
        public int tail;
        public string type;
    }

    [System.Serializable]
    public class Scene
    {
        public Entity[] entities;
        public Relation[] relations;
    }

    [System.Serializable]
    public class Scenes
    {
        public Scene[] result;
    }

    public static void getSubScenesJson()
    {
        if (GlobalVariable.subScenes.result == null)
        {
            string subScenePath = "Data/SubSceneDefinitions/currSubSceneList";
            TextAsset textJSON = Resources.Load<TextAsset>(subScenePath);
            GlobalVariable.subScenes = JsonUtility.FromJson<Scenes>(textJSON.text);
        }
    }

    public static void saveJsonFile()
    {
        string outputString = JsonUtility.ToJson(GlobalVariable.subScenes);
        string subScenePath = "/Resources/Data/SubSceneDefinitions/currSubSceneList.json";
        File.WriteAllText(Application.dataPath + subScenePath, outputString);
    }
}
