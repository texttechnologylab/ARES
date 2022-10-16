using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class EntityInfoMenu : MonoBehaviour
{
    public GameObject infoPanel;
    public GameObject resourcesPanel;
    public GameObject canvasEntityInfo;
    public GameObject indexInput;
    public GameObject lemmaInput;
    public GameObject amountInput;
    public GameObject idInput;
    public GameObject attributesInput;

    public GameObject entityPrefab;

    public GameObject relationCavnas;
    public GameObject entityCanvas;

    public GameObject errorText;

    public void saveEntity()
    {
        if (isValidEntity())
        {
            if (DragDropParameters.currentEntity.GetComponent<DragAndDrop>().isNewEntity)
            {
                Transform nameTransform = DragDropParameters.currentEntity.transform.Find("Name");
                if (nameTransform != null)
                {
                    Transform textTransform = nameTransform.Find("Text (TMP)");
                    if (textTransform != null)
                    {
                        textTransform.gameObject.GetComponent<TextMeshProUGUI>().text = getLemma();
                    }
                }
                ExtractJsonScene.Entity newEntity = new ExtractJsonScene.Entity();
                newEntity.index = getIndex();
                newEntity.lemma = getLemma();
                newEntity.amount = getAmount();
                newEntity.id = getId();
                DragDropParameters.currScene.deleteAllAttributesForIndex(getIndex());
                DragDropParameters.currScene.addNewAttributesForIndex(getAttributes(), getIndex());
                ExtractJsonScene.Entity[] entities = new ExtractJsonScene.Entity[DragDropParameters.currScene.entities.Length + 1];
                for (int idx = 0; idx < DragDropParameters.currScene.entities.Length; idx++)
                {
                    entities[idx] = DragDropParameters.currScene.entities[idx];
                }
                entities[entities.Length-1] = newEntity;
                DragDropParameters.currScene.entities = entities;


                DragAndDrop dad = DragDropParameters.currentEntity.GetComponent<DragAndDrop>();
                dad.entity = newEntity;
                dad.isNewEntity = false;
                dad.relationCavnas = relationCavnas;
                dad.entityCanvas = entityCanvas;
                OptionScreenHandler.addButtonActionToEntity(DragDropParameters.currentEntity, canvasEntityInfo);
                DragDropParameters.entityList.Add(DragDropParameters.currentEntity);
                DragDropParameters.newEntityImageSearched = false;
                resetResourcesEntity();
            }
            else
            {
                DragAndDrop dad = DragDropParameters.currentEntity.GetComponent<DragAndDrop>();
                ExtractJsonScene.Entity entity = dad.entity;
                DragDropParameters.newEntityImageSearched = false;
                entity.index = getIndex();
                entity.lemma = getLemma();
                entity.amount = getAmount();
                if (getId() != null)
                {
                    if (!entity.id.Equals(getId()))
                    {
                        dad.imageAttached = false;
                        entity.id = getId();
                    }
                }
                DragDropParameters.currScene.deleteAllAttributesForIndex(getIndex());
                DragDropParameters.currScene.addNewAttributesForIndex(getAttributes(), getIndex());
            }

            canvasEntityInfo.SetActive(false);
            indexInput.GetComponent<InputField>().text = "";
            lemmaInput.GetComponent<InputField>().text = "";
            amountInput.GetComponent<InputField>().text = "";
            idInput.GetComponent<InputField>().text = "";
            attributesInput.GetComponent<InputField>().text = "";
        }
    }

    private void resetResourcesEntity()
    {
        Vector3 scale = new Vector3(1,1,1);
        GameObject newEntity = Instantiate(entityPrefab, Vector3.zero, Quaternion.identity);
        newEntity.transform.parent = resourcesPanel.transform;
        newEntity.transform.localScale = scale;
        newEntity.transform.localPosition = Vector3.zero;
    }


    private int getIndex()
    {
        return indexInput.GetComponent<InputField>().text == "" ? -1 : int.Parse(indexInput.GetComponent<InputField>().text);
    }
    private string getLemma()
    {
        return lemmaInput.GetComponent<InputField>().text;
    }
    private int getAmount()
    {
        return amountInput.GetComponent<InputField>().text == "" ? -1 : int.Parse(amountInput.GetComponent<InputField>().text);
    }
    private string getId()
    {
        return !idInput.GetComponent<InputField>().text.Equals("") ? idInput.GetComponent<InputField>().text : null;
    }
    private string[] getAttributes()
    {
        return attributesInput.GetComponent<InputField>().text.Split(',');
    }

    private bool isValidEntity()
    {
        if (getIndex() < 0)
        {
            errorText.GetComponent<TMP_Text>().text = "Index has to be positive or 0!";
            return false;
        }
        if (entityIndexExists() && !entityIndexNotChanged())
        {
            errorText.GetComponent<TMP_Text>().text = "Index is already taken!";
            return false;
        }
        if (getLemma() != null)
        {
            if (getLemma().Equals(""))
            {
                errorText.GetComponent<TMP_Text>().text = "Lemma is missing!";
                return false;
            }
        }
        if (getAmount() <= 0)
        {
            errorText.GetComponent<TMP_Text>().text = "Amount has to be greater than 0!";
            return false;
        }
        if (getId() != null){
            if (!getId().Equals(""))
            {
                if (GlobalVariable.ShapeNetObjects[getId()] == null)
                {
                    errorText.GetComponent<TMP_Text>().text = "Leave Id empty or enter a valid Id!";
                    return false;
                }   
            }
        }
        return true;
    }

    private bool entityIndexExists()
    {
        foreach (GameObject entityObject in DragDropParameters.entityList)
        {
            ExtractJsonScene.Entity entity = entityObject.GetComponent<DragAndDrop>().entity;
            if (entity.index == getIndex())
            {
                return true;
            }
        }
        return false;
    }

    private bool entityIndexNotChanged()
    {
        GameObject entityObject = DragDropParameters.currentEntity;
        if (entityObject != null)
        {
            DragAndDrop dad = entityObject.GetComponent<DragAndDrop>();
            if (dad.entity != null && !dad.isNewEntity)
            {
                if (dad.entity.index == getIndex())
                {
                    return true;
                }
            }
        }
        return false;
    }
}
