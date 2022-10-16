using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class DragAndDropMenu : MonoBehaviour
{
    public GameObject resourcesPanel;
    public GameObject scenePanel;
    public GameObject resourcesButton;
    public GameObject sceneButton;

    private Color buttonDefaultColor;
    private List<GameObject> entityListButtons = new List<GameObject>();
    private List<GameObject> relationListButtons = new List<GameObject>();
    public GameObject entityContentComponent;
    public GameObject relationsContentComponent;
    public GameObject listButtonPrefab;

    public GameObject entityRemoveButton;
    public GameObject relationRemoveButton;
    private GameObject curSelectedEntity;
    private GameObject curSelectedRelation;

    public void backToSceneGeneration()
    {
        SceneManager.LoadScene("GenerateSceneMenu");
    }

    public void changeToResources()
    {
        resourcesButton.GetComponent<Button>().interactable = false;
        sceneButton.GetComponent<Button>().interactable = true;
        resourcesPanel.SetActive(true);
        scenePanel.SetActive(false);
    }

    public void changeToScene()
    {
        resourcesButton.GetComponent<Button>().interactable = true;
        sceneButton.GetComponent<Button>().interactable = false;
        resourcesPanel.SetActive(false);
        scenePanel.SetActive(true);

        displaySceneComponents();
    }

    public void displaySceneComponents()
    {
        ColorUtility.TryParseHtmlString("#383838", out buttonDefaultColor);
        // entity components
        while (entityListButtons.Count != 0)
        {
            GameObject button = entityListButtons[0];
            entityListButtons.RemoveAt(0);
            Destroy(button);
        }
        for (int entity_nr = 0; entity_nr < DragDropParameters.entityList.Count; entity_nr++)
        {
            ExtractJsonScene.Entity entity = DragDropParameters.entityList[entity_nr].GetComponent<DragAndDrop>().entity;
            GameObject listButton =  Instantiate(listButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            listButton.GetComponentInChildren<TMP_Text>().text = new string(' ', 2) + entity.index.ToString() + ": " + entity.lemma;
            listButton.transform.parent = entityContentComponent.transform;
            listButton.transform.localScale = new Vector3(1f, 1f, 1f);
            Vector3 newPos = listButton.transform.localPosition;
            newPos.z = 0;
            listButton.transform.localPosition = newPos;
            entityListButtons.Add(listButton);
            buttonEventEntity(listButton, DragDropParameters.entityList[entity_nr]);
        }
        // relation components
        while (relationListButtons.Count != 0)
        {
            GameObject button = relationListButtons[0];
            relationListButtons.RemoveAt(0);
            Destroy(button);
        }
        for (int relation_nr = 0; relation_nr < DragDropParameters.relationList.Count; relation_nr++)
        {
            ExtractJsonScene.Relation relation = DragDropParameters.relationList[relation_nr].GetComponent<RelationLine>().relation;
            GameObject listButton =  Instantiate(listButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            listButton.GetComponentInChildren<TMP_Text>().text = new string(' ', 2) + relation.head + ' ' + relation.type + ' ' + relation.tail;
            listButton.transform.parent = relationsContentComponent.transform;
            listButton.transform.localScale = new Vector3(1f, 1f, 1f);
            Vector3 newPos = listButton.transform.localPosition;
            newPos.z = 0;
            listButton.transform.localPosition = newPos;
            relationListButtons.Add(listButton);
            buttonEventRelation(listButton, DragDropParameters.relationList[relation_nr]);
        }
    }

    private void onClickListButtonEntity(GameObject curButton, GameObject entityObject)
    {
        ColorUtility.TryParseHtmlString("#383838", out buttonDefaultColor);

        for (int button_nr = 0; button_nr < entityListButtons.Count; button_nr++)
        {
            if (GameObject.ReferenceEquals(curButton, entityListButtons[button_nr]))
            {
                entityListButtons[button_nr].GetComponent<Image>().color = Color.black;
                entityRemoveButton.GetComponent<Button>().interactable = true;
                curSelectedEntity = entityObject;
            }
            else
            {
                entityListButtons[button_nr].GetComponent<Image>().color = buttonDefaultColor;
            }
        }
    }

    private void buttonEventEntity(GameObject curButton, GameObject entityObject)
    {
        curButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate {onClickListButtonEntity(curButton, entityObject); });
    }

    public void removeEntity()
    {
        removeRelatedRelations(curSelectedEntity);
        ExtractJsonScene.Entity selectedEntity = curSelectedEntity.GetComponent<DragAndDrop>().entity;
        ExtractJsonScene.Entity[] entities = new ExtractJsonScene.Entity[DragDropParameters.currScene.entities.Length-1];
        int index = 0;
        foreach(ExtractJsonScene.Entity entity in DragDropParameters.currScene.entities)
        {
            if (!selectedEntity.Equals(entity))
            {
                entities[index] = entity;
                index++;
            }
        }
        DragDropParameters.currScene.entities = entities;
        for(int indx = 0; indx < DragDropParameters.entityList.Count; indx++)
        {
            if(GameObject.ReferenceEquals(curSelectedEntity, DragDropParameters.entityList[indx]))
            {
                DragDropParameters.entityList.RemoveAt(indx);
            }
        }
        Destroy(curSelectedEntity);
        displaySceneComponents();
        entityRemoveButton.GetComponent<Button>().interactable = false;
        relationRemoveButton.GetComponent<Button>().interactable = false;
        curSelectedEntity = null;
        curSelectedRelation = null;
    }

    private void onClickListButtonRelation(GameObject curButton, GameObject relationObject)
    {
        ColorUtility.TryParseHtmlString("#383838", out buttonDefaultColor);
        for (int button_nr = 0; button_nr < relationListButtons.Count; button_nr++)
        {
            if (GameObject.ReferenceEquals(curButton, relationListButtons[button_nr]))
            {
                relationListButtons[button_nr].GetComponent<Image>().color = Color.black;
                relationRemoveButton.GetComponent<Button>().interactable = true;
                curSelectedRelation = relationObject;
            }
            else
            {
                relationListButtons[button_nr].GetComponent<Image>().color = buttonDefaultColor;
            }
        }
    }

    private void buttonEventRelation(GameObject curButton, GameObject relationObject)
    {
        curButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate {onClickListButtonRelation(curButton, relationObject); });
    }

    public void removeRelation()
    {
        ExtractJsonScene.Relation selectedRelation = curSelectedRelation.GetComponent<RelationLine>().relation;
        ExtractJsonScene.Relation[] relations = new ExtractJsonScene.Relation[DragDropParameters.currScene.relations.Length-1];
        int index = 0;
        foreach(ExtractJsonScene.Relation relation in DragDropParameters.currScene.relations)
        {
            if (!selectedRelation.Equals(relation))
            {
                relations[index] = relation;
                index++;
            }
        }
        DragDropParameters.currScene.relations = relations;
        for(int indx = 0; indx < DragDropParameters.relationList.Count; indx++)
        {
            if(GameObject.ReferenceEquals(curSelectedRelation, DragDropParameters.relationList[indx]))
            {
                DragDropParameters.relationList.RemoveAt(indx);
            }
        }
        Destroy(curSelectedRelation);
        displaySceneComponents();
        entityRemoveButton.GetComponent<Button>().interactable = false;
        relationRemoveButton.GetComponent<Button>().interactable = false;
        curSelectedEntity = null;
        curSelectedRelation = null;
    }

    public void removeRelatedRelations(GameObject entityObject)
    {
        bool relationFound = true;
        while (relationFound)
        {
            relationFound = false;
            foreach(GameObject relation in DragDropParameters.relationList)
            {
                RelationLine rl = relation.GetComponent<RelationLine>();
                if (GameObject.ReferenceEquals(rl.head, entityObject) || GameObject.ReferenceEquals(rl.tail, entityObject))
                {
                    ExtractJsonScene.Relation selectedRelation = relation.GetComponent<RelationLine>().relation;
                    ExtractJsonScene.Relation[] relations = new ExtractJsonScene.Relation[DragDropParameters.currScene.relations.Length-1];
                    int index = 0;
                    foreach(ExtractJsonScene.Relation listRelation in DragDropParameters.currScene.relations)
                    {
                        if (!selectedRelation.Equals(listRelation))
                        {
                            relations[index] = listRelation;
                            index++;
                        }
                    }
                    DragDropParameters.currScene.relations = relations;
                    
                    for(int indx = 0; indx < DragDropParameters.relationList.Count; indx++)
                    {
                        if(GameObject.ReferenceEquals(relation, DragDropParameters.relationList[indx]))
                        {
                            DragDropParameters.relationList.RemoveAt(indx);
                        }
                    }
                    Destroy(relation);
                    relationFound = true;
                    break;
                }
            }
        }
    }
}
