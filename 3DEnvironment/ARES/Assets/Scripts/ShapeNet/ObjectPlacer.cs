using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

using UnityEditor;
/*
@source: StolperwegeVR
*/
public class ObjectPlacer : MonoBehaviour
{
    private static string path = @"D:\Data\Corpora\ShapeNet\ShapeNet Sem\v0\models\";
    private static string m_path = @"D:\Data\Corpora\ShapeNet\ShapeNet Sem\v0\models\";

    public static GameObject Finde_Object_by_Id(string id)
    {
        string objpath = path + id + ".obj";
        string mtlpath = path + id + ".mtl";

        if (!File.Exists(objpath))
        {
            Debug.Log(objpath + " obj doesn't exist.");
        }
        else
        {

            if (!File.Exists(mtlpath))
            {
                Debug.Log(mtlpath + " mtl doesn't exist.");
            }

            Dummiesman.OBJLoader loader = new Dummiesman.OBJLoader();
            GameObject obj = loader.Load(objpath, mtlpath);

            return obj;
        }
        return null;
    }

    public static Object GetSprite(string fileName)
    {
        fileName = fileName.Replace("D:\\Uni\\Bachelorarbeit\\main\\ARES\\3DEnvironment\\ARES\\Assets\\Resources\\", "");
        fileName = fileName.Replace(".obj", "");

        Object sprite = Resources.Load<Object>(fileName);
        
        return sprite;
    }

    public static GameObject LoadObject(string objpath, string mtlpath)
    {   
        if (!File.Exists(objpath))
        {
            Debug.Log(objpath + " obj doesn't exist.");
        }
        else
        {

            if (!File.Exists(mtlpath))
            {
                Debug.Log(mtlpath + " mtl doesn't exist.");
            }

            Dummiesman.OBJLoader loader = new Dummiesman.OBJLoader();
            GameObject obj = loader.Load(objpath);
            return obj;
        }
        return null;
    }

    public static GameObject Reorientate_Obj(GameObject obj, Vector3 up, Vector3 front, float scale)
    {
        obj.transform.localScale = new Vector3(scale, scale, scale);

        Quaternion rotation_up = Quaternion.FromToRotation(up, Vector3.up);
        obj.transform.rotation = rotation_up;

        Quaternion rotation_front = Quaternion.FromToRotation(front, -obj.transform.forward);
        obj.transform.rotation *= rotation_front;

        Renderer obj_renderer = obj.GetComponentInChildren<Renderer>();
        Vector3 render_position_min = -obj_renderer.bounds.min;
        Vector3 render_position_med = -obj_renderer.bounds.center;
        obj.transform.localPosition = new Vector3(render_position_med.x, render_position_min.y, render_position_med.z);


        GameObject oriented_obj = new GameObject(obj.name+"_center");
        obj.transform.SetParent(oriented_obj.transform,true);
        return oriented_obj;
 
    }


}
