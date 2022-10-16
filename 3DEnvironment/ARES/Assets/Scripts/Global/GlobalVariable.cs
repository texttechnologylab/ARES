using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GlobalVariable : MonoBehaviour
{
    public static RelationJson.Relations relations = new RelationJson.Relations();
    public static AttributeJson.Attributes attributes = new AttributeJson.Attributes();
    public static SubScene.Scenes subScenes = new SubScene.Scenes();
    public static int selectedRelation = -1;
    public static Dictionary<string, ShapeNetModel> ShapeNetObjects = new Dictionary<string, ShapeNetModel>();
    public static bool allShapeNetObjectsProcessed = false;
    public static Dictionary<string, string> CachedObjectPathMap = new Dictionary<string, string>();
    public static ExtractJsonScene.Scenes scenesForVisualization;
    public static string inputText;
    public static List<Entity> entitiesScene = new List<Entity>();
    public static List<Relation> relationsScene = new List<Relation>();
    public static bool sceneObjectsLoaded = false;
    public static bool collisionOccured = false;
    public static bool physicsActivated = false;
    public static float allowedCollisionRangeFix = 0.1f;
    public static float currMaxDistancePP = 0f;
    public static float currMaxDistanceNN = 0f;
    public static float currMaxDistancePN = 0f;
    public static float currMaxDistanceNP = 0f;
}
