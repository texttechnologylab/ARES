using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using System.Threading;
using System.Threading.Tasks;
using System.IO;
using ICSharpCode.SharpZipLib.Zip;
using LitJson;
using System.Net;


public class ShapeNetHandler : MonoBehaviour
{
    public enum Usage {DragAndDrop, SceneGeneration};
    public Usage usedFor;
    public GameObject defaultEntity;

    public const string WS = "http://shapenet.texttechnologylab.org/";
    private const string CACHE_DIR = "\\Documents\\text2scene\\";
    public const string LOADED_OBJECTS = WS + "loadedobjects";
    public const string GET_OBJECT_ID = WS + "get?id=";
    private const string CACHED_OBJECT_DIR = CACHE_DIR + "objects\\";
    private const string CACHED_OBJECT_FILES = CACHED_OBJECT_DIR + "models\\";
    private const string CACHED_TEXTURE_DIR = CACHE_DIR + "textures\\";
    private const string CACHED_TEXTURE_FILES = CACHED_TEXTURE_DIR + "textureFiles\\";


    public const string OBJECT_THUMBNAILS = WS + "thumbnails";
    public const string OBJECT_THUMBNAIL_INFO = WS + "thumbnailsInfos";
    private const string CACHED_OBJECT_THUMBNAILS = CACHED_OBJECT_DIR + "thumbnails\\";
    private const string ThumbnailInfosOld = "thumbnailLastUpdate.txt";
    private const string ThumbnailInfos = "thumbnailInfos.json"; 
    private const string ThumbnailZip = "thumbnails.zip";
    private bool _objectThumbnailsActualized;
    private string _objectThumbnailError;
    private long JSONtimestamp;
    private long JSONSize;
    private long StoredTimestamp;
    private long UnzippedSize;
    private long StoredSize;
    private StreamReader streamReader;
    private JsonData storedInfos;
    private long ActualSize;
    private FileInfo[] ActualFiles;
    private byte[] bytes;


    private string _path;
    public string UserFolder { get; private set; }


    private UnityWebRequest request;
    private string _objectListError;
    private FileStream fileStream;
    private DirectoryInfo dir;
    private Thread _unzippingThread;
    public string InitStatus;

    private JsonData data;
    private JsonData objectList;

    public Dictionary<string, string> CachedTexturePathMap { get; private set; }

    private ShapeNetModel shapeNetModel;

    private GameObject GhostObject;

    private ShapeNetModel _shapeNetObject;
    BoxCollider _collider;
    private Dictionary<string, GameObject[]> _generatedObjects = new Dictionary<string, GameObject[]>();

    public OnObjectLoaded Event { get; private set; }
    public OnObjectLoaded onLoadedTest;
    /*
    @source: StolperwegeVR
    */
    public delegate void OnObjectLoaded(string filePath);

    // scene generation @author: Patrick Masny
    
    /*
    @source: StolperwegeVR
    */
    private void InitializeCache()
    {
        InitStatus = "Initializing cached model map...";
        _path = UserFolder + CACHED_OBJECT_FILES;
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);
        GlobalVariable.CachedObjectPathMap = new Dictionary<string, string>();
        string[] paths = Directory.GetDirectories(_path);
        for (int i = 0; i < paths.Length; i++)
        {
            dir = new DirectoryInfo(paths[i]);
            GlobalVariable.CachedObjectPathMap.Add(dir.Name, dir.FullName);
        }

        InitStatus = "Initializing cached texture map...";
        _path = UserFolder + CACHED_TEXTURE_FILES;
        if (!Directory.Exists(_path))
            Directory.CreateDirectory(_path);
        CachedTexturePathMap = new Dictionary<string, string>();
        paths = Directory.GetDirectories(_path);
        for (int i = 0; i < paths.Length; i++)
        {
            dir = new DirectoryInfo(paths[i]);
            CachedTexturePathMap.Add(dir.Name, dir.FullName);
        }
    }
    /*
    @author: Patrick Masny
    */
    public static async Task fillModelDictAsnyc(JsonData objectList)
    {
        await Task.Run( () => {
            for (int i = 0; i < objectList.Count; i++)
            {
                ShapeNetModel shapeNetModel = new ShapeNetModel(objectList[i]);
                GlobalVariable.ShapeNetObjects.Add(shapeNetModel.ID, shapeNetModel);
            }
            GlobalVariable.allShapeNetObjectsProcessed = true;
            
        });
    }
    /*
    @source: StolperwegeVR
    @further work: Patrick Masny -> Async Processing
    */
    private IEnumerator LoadModelList()
    {
        request = UnityWebRequest.Get(LOADED_OBJECTS);
        yield return request.SendWebRequest();
        if (request.isNetworkError || request.isHttpError)
            _objectListError = request.error;
        else
        {
            data = JsonMapper.ToObject(request.downloadHandler.text);

            if (!data.Keys.Contains("success") || !bool.Parse(data["success"].ToString()) ||
                !data.Keys.Contains("ShapeNetObj"))
            {
                _objectListError = "Downloading object list failed.";
                yield break;
            }
            objectList = data["ShapeNetObj"];
            Task task = Task.Run(() => fillModelDictAsnyc(objectList));
        }
    }
    /*
    @source: StolperwegeVR
    */
    private void UnzipFile(string filePath, string targetDir)
    {
        FastZip zip = new FastZip();
        zip.ExtractZip(filePath, targetDir, null);        
    }

    /*
    @source: StolperwegeVR
    */
    public IEnumerator GetModel(string id, OnObjectLoaded onLoaded)
    {
        _path = UserFolder + CACHED_OBJECT_FILES;
        if (GlobalVariable.CachedObjectPathMap.ContainsKey(id))
        {
            onLoaded(GlobalVariable.CachedObjectPathMap[id]);
            Debug.Log("Searched model was cached.");
            yield break;

        }

        Debug.Log("Searched model was not cached, downloading it...");
        request = UnityWebRequest.Get(GET_OBJECT_ID + id);
        yield return request.SendWebRequest();

        if (request.isNetworkError)
            Debug.Log(request.error);
        else
        {
            if (!Directory.Exists(_path + id))
                Directory.CreateDirectory(_path + id);
            string zipFile = _path + id + "\\" + id + ".zip";
            fileStream = new FileStream(zipFile, FileMode.Create); 
            fileStream.Write(request.downloadHandler.data, 0, request.downloadHandler.data.Length);
            fileStream.Close();
            _unzippingThread = new Thread(() => { UnzipFile(zipFile, _path + id); });
            _unzippingThread.Start();
            while (_unzippingThread.IsAlive)
                yield return null;
            GlobalVariable.CachedObjectPathMap.Add(id, _path + id);
            File.Delete(zipFile);
            onLoaded(GlobalVariable.CachedObjectPathMap[id]);
        }
    }

    /*
    @source: StolperwegeVR
    */
    public void Awake()
    {       
        StartCoroutine(Initialize());
    }
    /*
    @source: StolperwegeVR
    */
    private IEnumerator Initialize()
    {
        UserFolder = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        _objectThumbnailsActualized = false;
        _objectThumbnailError = null;
        
        if (!_objectThumbnailsActualized)
        {
            InitStatus = "Loading Shapenet-Model-Thumbnails...";
            yield return StartCoroutine(CheckThumbnails(CACHED_OBJECT_DIR, CACHED_OBJECT_THUMBNAILS, CACHED_OBJECT_FILES, OBJECT_THUMBNAIL_INFO, OBJECT_THUMBNAILS, "Object"));
            if (_objectThumbnailError == null) InitStatus = "ShapeNet object-thumbnails actualized.";
            else InitStatus = "ShapeNet object-thumbnails cannot be actualized: " + _objectThumbnailError;
            _objectThumbnailsActualized = _objectThumbnailError == null;
        }

        yield return "Test Test Test";
    }

    public void LoadModel(int entityIndex)
    {
        Entity entity = GlobalVariable.entitiesScene[entityIndex];
        if (entity.modelInfo != null){
            string id = entity.modelInfo.ID;
            GhostObject = null;        
            _shapeNetObject = GlobalVariable.ShapeNetObjects[id];
            StartCoroutine(GetModel(_shapeNetObject.ID, (path) =>
            {
                GameObject _object = ObjectPlacer.LoadObject(path + "\\" + _shapeNetObject.ID + ".obj", path + "\\" + _shapeNetObject.ID + ".mtl");
                GhostObject = ObjectPlacer.Reorientate_Obj(_object, _shapeNetObject.Up, _shapeNetObject.Front, _shapeNetObject.Unit);
                entity.model = _object;
                currEntityFetchLocked = false;
                GhostObject.AddComponent<MeshCollider>();
            }));
        }
        else
        {
            //unknown entity
            GameObject objectParent = Instantiate(defaultEntity);
            GameObject _object = objectParent.transform.Find("entity").gameObject;
            entity.model = _object;
            currEntityFetchLocked = false;
            _object.AddComponent<MeshCollider>();
        }
    }

    public void GetThumbnail(string id)
    {
        ShapeNetObject sObj = GlobalVariable.ShapeNetObjects[id];
        StartCoroutine(LoadThumbnail(sObj));
    }

    string url;
    private IEnumerator LoadThumbnail(ShapeNetObject sObj)
    {
        url = "file://" + UserFolder;
        if (sObj is ShapeNetModel) url += CACHED_OBJECT_THUMBNAILS + sObj.ID + "-7.png";
        UnityWebRequest request = UnityWebRequestTexture.GetTexture(url);
        yield return request.SendWebRequest();
        if (request.error == null)
        {
            sObj.Thumbnail = DownloadHandlerTexture.GetContent(request); 
        }      
    }

    private IEnumerator CheckThumbnails(string cacheFolder, string thumbnailCacheFolder, string dataCacheFolder, string infoURL, string thumbnailURL, string thumbnailType)
    {
        _path = UserFolder + thumbnailCacheFolder;

            if (!Directory.Exists(_path)){
            Directory.CreateDirectory(_path);

            _unzippingThread = new Thread(() => 
            { 
                using (var client = new WebClient())
                {
                    client.DownloadFile(thumbnailURL, _path + ThumbnailZip);
                }
                UnzipFile(_path + ThumbnailZip, _path); 
            });
            _unzippingThread.Start();
            while (_unzippingThread.IsAlive)
                yield return null;
            File.Delete(_path + ThumbnailZip);
            ActualSize = 0;
            ActualFiles = new DirectoryInfo(UserFolder + thumbnailCacheFolder).GetFiles();
            for (int i = 0; i < ActualFiles.Length; i++)
                ActualSize += ActualFiles[i].Length;
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(LoadModelList());
    }
    //
    private bool testbool = true;
    //
    // Update is called once per frame
    void Update()
    {
        if(usedFor == Usage.SceneGeneration)
        {
            createEntitiesWithRelations();
            instEntities();
        }
    }
    /*
    @author: Patrick Masny
    */
    private int currEntityIndex = 0;
    private bool currEntityFetchLocked = false;
    public void instEntities()
    {
        if (entitiesSceneFound)
        {
            if (currEntityIndex < GlobalVariable.entitiesScene.Count && !currEntityFetchLocked)
            {
                currEntityFetchLocked = true;
                LoadModel(currEntityIndex);
                currEntityIndex++;
            }
            else if (currEntityIndex == GlobalVariable.entitiesScene.Count && !currEntityFetchLocked)
            {
                GlobalVariable.sceneObjectsLoaded = true;
                currEntityIndex++;
            }
        }
    }
    /*
    @author: Patrick Masny
    */
    public bool entitiesSceneFound = false;
    public void createEntitiesWithRelations()
    {
        if (GlobalVariable.allShapeNetObjectsProcessed && !entitiesSceneFound)
        {
            foreach (ExtractJsonScene.Scene scene in GlobalVariable.scenesForVisualization.result)
            {
                DragDropParameters.currScene = scene;
                //entities
                foreach (ExtractJsonScene.Entity entity in scene.entities)
                {
                    string id = entity.id != null ? entity.id : searchShapeNetObject(entity);
                    if (id != null)
                    {
                        ShapeNetModel modelInfo = GlobalVariable.ShapeNetObjects[id];
                        for (int nr = 0; nr < entity.amount; nr++)
                        {
                            int copy_nr = entity.amount > 1 ? nr : -1;
                            GlobalVariable.entitiesScene.Add(new Entity(modelInfo, entity.index, entity.lemma, nr));
                        }
                    }
                    else
                    {
                        GlobalVariable.entitiesScene.Add(new Entity(null, entity.index, entity.lemma, -1));
                    }
                }
                //relations
                foreach (ExtractJsonScene.Relation relation in scene.relations)
                {
                    foreach (Entity head in GlobalVariable.entitiesScene)
                    {
                        if (relation.head == head.index){
                            foreach (Entity tail in GlobalVariable.entitiesScene)
                            {
                                if (relation.tail == tail.index)
                                {
                                    bool found = false;
                                    foreach (RelationJson.Relation def in GlobalVariable.relations.result)
                                    {
                                        if (Array.Exists(def.names, element => element == relation.type)){
                                            GlobalVariable.relationsScene.Add(new Relation(head, tail, relation.type, def));
                                            found = true;
                                            break;
                                        }
                                    }
                                    if (!found)
                                    {
                                        GlobalVariable.relationsScene.Add(new Relation(head, tail, relation.type, null));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            entitiesSceneFound = true;
        }
    }
    /*
    @author: Patrick Masny
    */
    public string searchShapeNetObject(ExtractJsonScene.Entity entity)
    {
        int maxMatch = 0;
        string maxId = null;
        foreach (string id in GlobalVariable.ShapeNetObjects.Keys)
        {
            ShapeNetModel modelInfo = GlobalVariable.ShapeNetObjects[id];
            int curMatch = 0;
            if (modelInfo.ContainsTag(entity.lemma))
            {
                curMatch++;
                List<string> attributes = DragDropParameters.currScene.getAllAttributesForIndex(entity.index);
                foreach (string attribute in attributes)
                {
                    if (modelInfo.ContainsTag(attribute))
                    {
                        curMatch++;
                    }
                }
                if (maxMatch < curMatch)
                {
                    maxMatch = curMatch;
                    maxId = id;
                }
            }
        }
        return maxId;
    }
}