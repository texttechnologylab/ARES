using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RelationHandler : MonoBehaviour
{
    public Transform headCube;
    public Transform tailCube;
    public GameObject labelInputField;
    public GameObject namesInputField;
    public Slider headSlider;
    public Slider tailSlider;
    public Toggle useScaleToogle;
    public Toggle useRotationToogle;
    public Toggle useSpreadToogle;

    private InputField label;
    private InputField names;

    // Start is called before the first frame update
    void Start()
    {
        if (GlobalVariable.selectedRelation != -1)
        {
            label = labelInputField.GetComponent<InputField>();
            names = namesInputField.GetComponent<InputField>();

            RelationJson.Relation relation = GlobalVariable.relations.result[GlobalVariable.selectedRelation];
            label.text = relation.label;
            names.text = string.Join(",", relation.names);

            headCube.position = relation.head.position.getVector3();
            tailCube.position = relation.tail.position.getVector3();

            headCube.localScale = relation.head.scale.getVector3();
            tailCube.localScale = relation.tail.scale.getVector3();

            headCube.eulerAngles = relation.head.rotation.getVector3();
            tailCube.eulerAngles = relation.tail.rotation.getVector3();

            headSlider.value = relation.head.spread;
            tailSlider.value = relation.tail.spread;

            headSlider.value = relation.head.spread;
            tailSlider.value = relation.tail.spread;

            useScaleToogle.isOn = relation.useScale;
            useRotationToogle.isOn = relation.useRotation;
            useSpreadToogle.isOn = relation.useSpread;
        }
    }

    public RelationJson.Relation getRelationFromScene()
    {
        label = labelInputField.GetComponent<InputField>();
        names = namesInputField.GetComponent<InputField>();

        RelationJson.Relation relation = new RelationJson.Relation();
        relation.label = label.text;
        relation.names = names.text.Split(',');

        RelationJson.Entity head = new RelationJson.Entity();
        head.setPos(headCube.position);
        head.setSca(headCube.localScale);
        head.setRot(headCube.eulerAngles);
        head.spread = headSlider.value;
        relation.head = head;

        RelationJson.Entity tail = new RelationJson.Entity();
        tail.setPos(tailCube.position);
        tail.setSca(tailCube.localScale);
        tail.setRot(tailCube.eulerAngles);
        tail.spread = tailSlider.value;
        relation.tail = tail;

        relation.useScale = useScaleToogle.isOn;
        relation.useRotation = useRotationToogle.isOn;
        relation.useSpread = useSpreadToogle.isOn;

        return relation;
    }

    public void saveAndExit()
    {
        RelationJson.Relation relation = getRelationFromScene();
        if (GlobalVariable.selectedRelation != -1)
        {
            GlobalVariable.relations.result[GlobalVariable.selectedRelation] = relation;
        }
        else
        {
            int totalRelationNumber = GlobalVariable.relations.result.Length + 1;
            RelationJson.Relation[] relations = new RelationJson.Relation[totalRelationNumber];
            for (int relation_nr = 0; relation_nr < totalRelationNumber - 1; relation_nr++)
            {
                relations[relation_nr] = GlobalVariable.relations.result[relation_nr];
            }
            relations[totalRelationNumber - 1] = relation;

            GlobalVariable.relations.result = relations;
        }

        RelationJson.saveJsonFile();
        SceneManager.LoadScene("RelationMenu");
    }
}
