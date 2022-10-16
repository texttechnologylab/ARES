using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RelationInfoMenu : MonoBehaviour
{
    public GameObject relationCanvas;
    public GameObject typeInput;
    public GameObject linePrefab;

    public void saveRelation()
    {
        if (getType() != null)
        {
            ExtractJsonScene.Relation newRelation = new ExtractJsonScene.Relation();
            GameObject headObject = DragDropParameters.currentRelationHead;
            GameObject tailObject = DragDropParameters.currentRelationTail;
            newRelation.head = headObject.GetComponent<DragAndDrop>().entity.index;
            newRelation.tail = tailObject.GetComponent<DragAndDrop>().entity.index;
            newRelation.type = getType();
            ExtractJsonScene.Relation[] relations = new ExtractJsonScene.Relation[DragDropParameters.currScene.relations.Length + 1];
            for (int idx = 0; idx < DragDropParameters.currScene.relations.Length; idx++)
            {
                relations[idx] = DragDropParameters.currScene.relations[idx];
            }
            relations[relations.Length-1] = newRelation;
            DragDropParameters.currScene.relations = relations;

            int headIndex = DragAndDropHandler.getEntityObjectIndex(newRelation.head);
            int tailIndex =  DragAndDropHandler.getEntityObjectIndex(newRelation.tail);
            if (headIndex != -1 && tailIndex != -1)
            {
                GameObject head = DragDropParameters.entityList[headIndex];
                GameObject tail = DragDropParameters.entityList[tailIndex];
                GameObject line = Instantiate(linePrefab, new Vector3(0, 0, 0), Quaternion.identity);
                RelationLine rl = line.GetComponent<RelationLine>();
                rl.head = head;
                rl.tail = tail;
                rl.relation = newRelation;
                DragDropParameters.relationList.Add(line);
            }

            relationCanvas.SetActive(false);
        }
    }

    private string getType()
    {
        return typeInput.GetComponent<InputField>().text;
    }
}
