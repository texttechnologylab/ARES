using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class DragAndDropHandler : MonoBehaviour
{
    public GameObject canvasEntityInfo;
    public GameObject sceneItems;
    public GameObject entityPrefab;
    public GameObject linePrefab;
    private ShapeNetHandler snh;
    private bool thumbnailSearchRun = false;
    public GameObject relationCavnas;
    public GameObject entityCanvas;

    public static int getEntityObjectIndex(int tokenIndex)
    {
        for(int index = 0; index < DragDropParameters.entityList.Count; index++)
        {
            GameObject entityObject = DragDropParameters.entityList[index];
            ExtractJsonScene.Entity entity = entityObject.GetComponent<DragAndDrop>().entity;
            if (entity.index == tokenIndex)
            {
                return index;
            }
        }
        return -1;
    }

    // Start is called before the first frame update
    void Start()
    {
        //reset variables
        DragDropParameters.entityList = new List<GameObject>();
        DragDropParameters.droppedInSceneBox = false;
        DragDropParameters.currentEntity = null;
        DragDropParameters.currScene = null;
        DragDropParameters.newEntityImageSearched = true;

        snh = gameObject.AddComponent<ShapeNetHandler>();
        if (GlobalVariable.scenesForVisualization != null){
            foreach (ExtractJsonScene.Scene scene in GlobalVariable.scenesForVisualization.result)
            {
                DragDropParameters.currScene = scene;
                foreach(ExtractJsonScene.Entity entity in scene.entities)
                {
                    GameObject entityObject = Instantiate(entityPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                    entityObject.transform.parent = sceneItems.transform;
                    entityObject.transform.localPosition = Vector3.zero;
                    entityObject.transform.localScale = new Vector3(1,1,1);
                    DragDropParameters.entityList.Add(entityObject);
                    entityObject.GetComponent<DragAndDrop>().entity = entity;
                    entityObject.GetComponent<DragAndDrop>().relationCavnas = relationCavnas;
                    entityObject.GetComponent<DragAndDrop>().entityCanvas = entityCanvas;
                    entityObject.GetComponent<DragAndDrop>().isNewEntity = false;

                    Transform nameTransform = entityObject.transform.Find("Name");
                    if (nameTransform != null)
                    {
                        Transform textTransform = nameTransform.Find("Text (TMP)");
                        if (textTransform != null)
                        {
                            textTransform.gameObject.GetComponent<TextMeshProUGUI>().text = entity.lemma;
                        }
                    }
                    OptionScreenHandler.addButtonActionToEntity(entityObject, canvasEntityInfo);
                }
                foreach(ExtractJsonScene.Relation relation in scene.relations)
                {
                    int headIndex = getEntityObjectIndex(relation.head);
                    int tailIndex = getEntityObjectIndex(relation.tail);
                    if (headIndex != -1 && tailIndex != -1)
                    {
                        GameObject head = DragDropParameters.entityList[headIndex];
                        GameObject tail = DragDropParameters.entityList[tailIndex];
                        GameObject line = Instantiate(linePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                        RelationLine rl = line.GetComponent<RelationLine>();
                        rl.head = head;
                        rl.tail = tail;
                        rl.relation = relation;
                        DragDropParameters.relationList.Add(line);
                    }
                }
            }
        }
    }

    void Update()
    {
        if(GlobalVariable.allShapeNetObjectsProcessed)
        {
            if (thumbnailSearchRun == false)
            {
                foreach (GameObject entityGameObject in DragDropParameters.entityList)
                {
                    bool imageAttached = entityGameObject.GetComponent<DragAndDrop>().imageAttached;
                    if (imageAttached == false)
                    {
                        ExtractJsonScene.Entity entity = entityGameObject.GetComponent<DragAndDrop>().entity;
                        string id = entity.id != null  ? entity.id : snh.searchShapeNetObject(entity);
                        if (id != null)
                        {
                            entity.id = id;
                            snh.GetThumbnail(id);
                        }
                    }
                }
                thumbnailSearchRun = true;
            }
            else if (!DragDropParameters.newEntityImageSearched)
            {
                bool imageAttached = DragDropParameters.currentEntity.GetComponent<DragAndDrop>().imageAttached;
                if (imageAttached == false)
                {
                    ExtractJsonScene.Entity entity = DragDropParameters.currentEntity.GetComponent<DragAndDrop>().entity;
                    string id = entity.id != null ? entity.id : snh.searchShapeNetObject(entity);
                    if (id != null)
                    {
                        entity.id = id;
                        snh.GetThumbnail(id);
                    }
                }
                DragDropParameters.newEntityImageSearched = true;
            }
            else
            {
                foreach (GameObject entityGameObject in DragDropParameters.entityList)
                {
                    bool imageAttached = entityGameObject.GetComponent<DragAndDrop>().imageAttached;
                    if (imageAttached == false)
                    {
                        ExtractJsonScene.Entity entity = entityGameObject.GetComponent<DragAndDrop>().entity;
                        string id = entity.id != null ? entity.id : snh.searchShapeNetObject(entity);
                        if (id == null)
                        {
                            entityGameObject.GetComponent<DragAndDrop>().imageAttached = true;
                        }
                        else
                        {
                            ShapeNetObject sObj = GlobalVariable.ShapeNetObjects[id];
                            if (sObj.Thumbnail != null)
                            {
                                Transform imageTransform = entityGameObject.transform.Find("Image");
                                if (imageTransform != null)
                                {
                                    Texture2D texture = sObj.Thumbnail;
                                    if (texture != null)
                                    {
                                        imageTransform.gameObject.GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                                    }
                                }
                                entityGameObject.GetComponent<DragAndDrop>().imageAttached = true;
                            }
                        }
                    }
                }
            }
        }
    }


}
