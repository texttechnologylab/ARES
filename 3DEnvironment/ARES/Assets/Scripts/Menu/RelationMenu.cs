using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class RelationMenu : MonoBehaviour
{
    public GameObject contentComponent;
    public GameObject listButtonPrefab;
    private List<GameObject> listButtons = new List<GameObject>();
    private Color buttonDefaultColor;

    // Start is called before the first frame update
    void Start()
    {
        ColorUtility.TryParseHtmlString("#383838", out buttonDefaultColor);

        GlobalVariable.selectedRelation = -1;
        if (GlobalVariable.relations.result == null)
        {
            getRelationsJson();
        }

        for (int relation_nr = 0; relation_nr < GlobalVariable.relations.result.Length; relation_nr++)
        {
            RelationJson.Relation relation = GlobalVariable.relations.result[relation_nr];
            GameObject listButton =  Instantiate(listButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            listButton.GetComponentInChildren<TMP_Text>().text = new string(' ', 2) + relation.label;
            listButton.transform.parent = contentComponent.transform;
            listButton.transform.localScale = new Vector3(1f, 1f, 1f);
            listButtons.Add(listButton);
            buttonEvent(listButtons[relation_nr], relation_nr);
        }
    }

    public static void getRelationsJson()
    {
        string relationsPath = "Data/RelationDefinitions/currRelationList";
        TextAsset textJSON = Resources.Load<TextAsset>(relationsPath);
        GlobalVariable.relations = JsonUtility.FromJson<RelationJson.Relations>(textJSON.text);
    }

    public void backToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void RelationCreatorNew()
    {
        GlobalVariable.selectedRelation = -1;
        SceneManager.LoadScene("RelationCreator");
    }

    public void RelationCreatorEdit()
    {
        if (GlobalVariable.selectedRelation != -1)
        {
            SceneManager.LoadScene("RelationCreator");
        }
    }

    public void removeRelation()
    {
        if (GlobalVariable.selectedRelation != -1)
        {
            GlobalVariable.relations.removeRelationAtIndex(GlobalVariable.selectedRelation);
            Destroy(listButtons[GlobalVariable.selectedRelation]);
            GlobalVariable.selectedRelation = -1;
            RelationJson.saveJsonFile();
        }
    }

    private void onClickListButton(int index)
    {
        GlobalVariable.selectedRelation = index;
        for (int button_nr = 0; button_nr < listButtons.Count; button_nr++)
        {
            if (button_nr == index)
            {
                listButtons[button_nr].GetComponent<Image>().color = Color.black;
            }
            else
            {
                listButtons[button_nr].GetComponent<Image>().color = buttonDefaultColor;
            }
        }
    }

    private void buttonEvent(GameObject curButton, int index)
    {
        curButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate {onClickListButton(index); });
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
