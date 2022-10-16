using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionScreenHandler : MonoBehaviour
{
    public static void addButtonActionToEntity(GameObject entityObject, GameObject canvasEntityInfo)
    {
        Transform optionTransform = entityObject.transform.Find("Options");
        if (optionTransform != null)
        {
            Button button = optionTransform.GetComponent<Button>();
            button.onClick.AddListener(() => {showEntityAttributes(entityObject, canvasEntityInfo); });
        }
    }
    private static void showEntityAttributes(GameObject buttonGameObject, GameObject canvasEntityInfo){
        ExtractJsonScene.Entity entity = buttonGameObject.GetComponent<DragAndDrop>().entity;
        DragDropParameters.currentEntity = buttonGameObject;
        canvasEntityInfo.SetActive(true);
        Transform entityInfo = canvasEntityInfo.transform.Find("EntityInfo");
        entityInfo.Find("IndexInputField").GetComponent<InputField>().text = entity.index.ToString();
        entityInfo.Find("LemmaInputField").GetComponent<InputField>().text = entity.lemma;
        entityInfo.Find("AmountInputField").GetComponent<InputField>().text = entity.amount.ToString();
        entityInfo.Find("IdInputField").GetComponent<InputField>().text = entity.id;
        entityInfo.Find("AttributesInputField").GetComponent<InputField>().text = string.Join(",", DragDropParameters.currScene.getAllAttributesForIndex(entity.index));
    }
}
