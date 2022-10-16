using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class CustomSceneCreator : MonoBehaviour
{
    public TMP_Text header;
    public InputField indexInput;
    public InputField lemmaInput;
    public InputField copyNrInput;
    public InputField idInput;
    public InputField attributesInput;
    public Button addButton;
    public Button removeButton;

    public GameObject entityInfo;
    public GameObject selEnt;
    public GameObject entitiesContent;

    public GameObject buttonPrefab;


    private Entity currEntity;
    private List<Entity> selectedEntities = new List<Entity>();
    private List<GameObject> listButtons = new List<GameObject>();

    public void enterInfo(Entity entity)
    {
        entityInfo.SetActive(true);
        Cursor.lockState = CursorLockMode.None;
        header.text = entity.entityName + " " + (entity.index + (entity.copyNr == -1 ? "" : '-' + entity.copyNr.ToString()));
        indexInput.text = entity.index.ToString();
        lemmaInput.text = entity.entityName;
        copyNrInput.text = entity.copyNr.ToString();
        idInput.text = entity.modelInfo.ID;
        attributesInput.text = string.Join(",", DragDropParameters.currScene.getAllAttributesForIndex(entity.index));
        currEntity = entity;
        viewList();
    }

    public void addEntity()
    {
        if (searchIndexInList() == -1)
        {
            selectedEntities.Add(currEntity);
            viewList();
            entityInfo.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void removeEntity()
    {
        int foundIndex = searchIndexInList();
        if (foundIndex != -1)
        {
            selectedEntities.RemoveAt(foundIndex);
            viewList();
            entityInfo.SetActive(false);
            Cursor.lockState = CursorLockMode.Locked;
        }
    }

    public void cancelEntity()
    {
        entityInfo.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void saveSubScene()
    {
        entityInfo.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;
        createSubScene();
        SubScene.saveJsonFile();
        selectedEntities = new List<Entity>();
        viewList();
    }

    public void createSubScene()
    {
        SubScene.getSubScenesJson();
        SubScene.Scene scene = new SubScene.Scene();
        bool anchorTaken = false;
        Vector3 origAnchorPos = Vector3.zero;
        List<SubScene.Entity> entities = new List<SubScene.Entity>();
        foreach (Entity entity in selectedEntities)
        {
            SubScene.Entity sse = new SubScene.Entity();

            sse.position = new SubScene.Vec3D();
            sse.rotation = new SubScene.Vec3D();
            sse.scale = new SubScene.Vec3D();

            sse.lemma = entity.entityName;
            sse.id = entity.modelInfo.ID;
            if (!anchorTaken)
            {
                anchorTaken = true;
                sse.isAnchor = true;
                origAnchorPos = entity.model.transform.position;
                sse.position.setVector3(Vector3.zero);
            }
            else
            {
                sse.isAnchor = false;
                sse.position.setVector3(entity.model.transform.position - origAnchorPos);
            }
            sse.rotation.setVector3(entity.model.transform.rotation.eulerAngles);
            sse.scale.setVector3(entity.model.transform.localScale);
            entities.Add(sse);
        }

        List<SubScene.Relation> relations = new List<SubScene.Relation>();
        foreach (Relation relation in GlobalVariable.relationsScene)
        {
            for (int headNr = 0; headNr < selectedEntities.Count; headNr++)
            {
                Entity head = selectedEntities[headNr];
                if (head.index == relation.head.index)
                {
                    for (int tailNr = 0; tailNr < selectedEntities.Count; tailNr++)
                    {
                        Entity tail = selectedEntities[tailNr];
                        if (tail.index == relation.tail.index)
                        {
                            SubScene.Relation ssr = new SubScene.Relation();
                            ssr.head = headNr;
                            ssr.tail = tailNr;
                            ssr.type = relation.relationType;
                            relations.Add(ssr);
                        }
                    }
                }
            }
        }

        scene.entities = entities.ToArray();
        scene.relations = relations.ToArray();
        int totalSceneNumber = GlobalVariable.subScenes != null ? GlobalVariable.subScenes.result.Length + 1 : 1;
        SubScene.Scene[] scenes = new SubScene.Scene[totalSceneNumber];
        for (int scene_nr = 0; scene_nr < totalSceneNumber - 1; scene_nr++)
        {
            scenes[scene_nr] = GlobalVariable.subScenes.result[scene_nr];
        }
        scenes[totalSceneNumber - 1] = scene;
        GlobalVariable.subScenes = new SubScene.Scenes();
        GlobalVariable.subScenes.result = scenes;
    }

    public int searchIndexInList()
    {
        for (int idx = 0; idx < selectedEntities.Count; idx++)
        {
            if (selectedEntities[idx].index == currEntity.index && selectedEntities[idx].copyNr == currEntity.copyNr)
            {
                return idx;
            }
        }
        return -1;
    }

    public void viewList()
    {
        if (searchIndexInList() == -1)
        {
            addButton.interactable = true;
            removeButton.interactable = false;
        }
        else
        {
            removeButton.interactable = true;
            addButton.interactable = false;
        }

        while (listButtons.Count != 0)
        {
            GameObject button = listButtons[0];
            Destroy(button);
            listButtons.RemoveAt(0);
        }
        if (selectedEntities.Count > 0)
        {
            selEnt.SetActive(true);

            for (int ent_nr = 0; ent_nr < selectedEntities.Count; ent_nr++)
            {
                Entity entity = selectedEntities[ent_nr];
                GameObject listButton =  Instantiate(buttonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
                listButton.GetComponentInChildren<TMP_Text>().text = new string(' ', 2) + ent_nr.ToString() + ". " + entity.entityName + " " + (entity.index + (entity.copyNr == -1 ? "" : '-' + entity.copyNr.ToString()));
                listButton.transform.parent = entitiesContent.transform;
                listButton.transform.localScale = new Vector3(1f, 1f, 1f);
                listButtons.Add(listButton);
            }
        }
        else
        {
            selEnt.SetActive(false);
        }
    }
}
