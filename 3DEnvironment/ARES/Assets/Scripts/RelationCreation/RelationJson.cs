using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;
using TMPro;

public class RelationJson : MonoBehaviour
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
        public Vec3D position;
        public Vec3D scale;
        public Vec3D rotation;
        public float spread;

        public void setPos(Vector3 vec3)
        {
            Vec3D vec3d = new Vec3D();
            vec3d.setVector3(vec3);
            position = vec3d;
        }
        public void setSca(Vector3 vec3)
        {
            Vec3D vec3d = new Vec3D();
            vec3d.setVector3(vec3);
            scale = vec3d;
        }
        public void setRot(Vector3 vec3)
        {
            Vec3D vec3d = new Vec3D();
            vec3d.setVector3(vec3);
            rotation = vec3d;
        }
    }
    [System.Serializable]
    public class Relation
    {
        public string label;
        public string[] names;
        public Entity head;
        public Entity tail;
        public bool useScale;
        public bool useRotation;
        public bool useSpread;
    }
    [System.Serializable]
    public class Relations
    {
        public Relation[] result;

        public void removeRelationAtIndex(int index)
        {
            Relation[] newResult = new Relation[result.Length-1];
            int count = 0;
            for (int relation_nr = 0; relation_nr < result.Length; relation_nr++)
            {
                if (relation_nr != index)
                {
                    newResult[count] = result[relation_nr];
                    count++;
                }
            }
            result = newResult;
        }
    }

    public Transform headCube;
    public Transform tailCube;
    public TMP_Text headCubeText;
    public TMP_Text tailCubeText;
    
    public static void saveJsonFile()
    {
        string outputString = JsonUtility.ToJson(GlobalVariable.relations);
        string relationsPath = "/Resources/Data/RelationDefinitions/currRelationList.json";
        File.WriteAllText(Application.dataPath + relationsPath, outputString);
    }

    void Update()
    {
        string headText = "Head:" + "\n";
        string tailText = "Tail:" + "\n";

        headText += headCube.position.ToString() + "\n";
        tailText += tailCube.position.ToString() + "\n";

        headText += headCube.localScale.ToString() + "\n";
        tailText += tailCube.localScale.ToString() + "\n";

        headText += headCube.eulerAngles.ToString() + "\n";
        tailText += tailCube.eulerAngles.ToString() + "\n";

        headCubeText.text = headText;
        tailCubeText.text = tailText;
    }
}
