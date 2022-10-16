using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class AttributeMenu : MonoBehaviour
{
    private List<GameObject> listButtons = new List<GameObject>();
    public GameObject listButtonPrefab;
    private Color buttonDefaultColor;
    private int selectedAttribute = -1;

    public GameObject creationPanel;
    public InputField attrName;
    public Dropdown type;
    public GameObject content;

    public Button createNewAttributeButton;
    public Button editAttributeButton;
    public Button removeAttributeButton;

    public GameObject colorPanel;
    public Slider colorR;
    public Slider colorG;
    public Slider colorB;
    public Slider colorA;
    public Image colorDisplay;
    public Toggle colorIsActive;

    private AttributeJson.Attribute currAttribute;


    public void backToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void createNewAttribute()
    {
        resetCreationPanel();

        currAttribute = null;

        createNewAttributeButton.interactable = false;
        editAttributeButton.interactable = false;
        removeAttributeButton.interactable = false;

        creationPanel.SetActive(true);
    }
    public void editAttribute()
    {
        resetCreationPanel();

        currAttribute = GlobalVariable.attributes.result[selectedAttribute];
        attrName.text = currAttribute.name;
        attrName.interactable = false;
        colorR.value = currAttribute.attributeColor.r;
        colorG.value = currAttribute.attributeColor.g;
        colorB.value = currAttribute.attributeColor.b;
        colorA.value = currAttribute.attributeColor.a;
        colorIsActive.isOn = currAttribute.attributeColor.isActive;

        createNewAttributeButton.interactable = false;
        editAttributeButton.interactable = false;
        removeAttributeButton.interactable = false;

        creationPanel.SetActive(true);
    }
    public void removeAttribute()
    {
        if (selectedAttribute != -1)
        {
            GlobalVariable.attributes.removeAttributeAtIndex(selectedAttribute);
            selectedAttribute = -1;
            AttributeJson.saveJsonFile();
            editAttributeButton.interactable = false;
            removeAttributeButton.interactable = false;
            removeAttributeButtons();
            addAttributeButtons();
        }
    }
    public void setDisplayColor()
    {
        colorDisplay.color = new Color(colorR.value, colorG.value, colorB.value, colorA.value);
    }
    public void saveAttribute()
    {
        if (currAttribute == null)
        {
            currAttribute = new AttributeJson.Attribute();
            currAttribute.name = attrName.text;
            AttributeJson.AttributeColor currColor = new AttributeJson.AttributeColor(colorR.value, colorG.value, colorB.value, colorA.value);
            currColor.isActive = colorIsActive.isOn;
            currAttribute.attributeColor = currColor;

            int totalAttributeNumber = GlobalVariable.attributes.result.Length + 1;
            AttributeJson.Attribute[] attributes = new AttributeJson.Attribute[totalAttributeNumber];
            for (int attribute_nr = 0; attribute_nr < totalAttributeNumber - 1; attribute_nr++)
            {
                attributes[attribute_nr] = GlobalVariable.attributes.result[attribute_nr];
            }
            attributes[totalAttributeNumber - 1] = currAttribute;
            GlobalVariable.attributes.result = attributes;
        }
        else
        {
            AttributeJson.AttributeColor currColor = new AttributeJson.AttributeColor(colorR.value, colorG.value, colorB.value, colorA.value);
            currColor.isActive = colorIsActive.isOn;
            currAttribute.attributeColor = currColor;
        }
        creationPanel.SetActive(false);
        resetCreationPanel();
        AttributeJson.saveJsonFile();
        removeAttributeButtons();
        addAttributeButtons();
        createNewAttributeButton.interactable = true;
        editAttributeButton.interactable = false;
        removeAttributeButton.interactable = false;
        selectedAttribute = -1;
    }

    public void resetCreationPanel()
    {
        attrName.name = "";
        colorR.value = 1;
        colorG.value = 1;
        colorB.value = 1;
        colorA.value = 1;
        colorIsActive.isOn = true;
        attrName.interactable = true;
    }

    // Start is called before the first frame update
    void Start()
    {
        AttributeJson.getAttributesJson();
        addAttributeButtons();
    }

    public void addAttributeButtons()
    {
        for (int attribute_nr = 0; attribute_nr < GlobalVariable.attributes.result.Length; attribute_nr++)
        {
            AttributeJson.Attribute attribute = GlobalVariable.attributes.result[attribute_nr];
            GameObject listButton =  Instantiate(listButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            listButton.GetComponentInChildren<TMP_Text>().text = new string(' ', 2) + attribute.name;
            listButton.transform.parent = content.transform;
            listButton.transform.localScale = new Vector3(1f, 1f, 1f);
            listButtons.Add(listButton);
            buttonEvent(listButtons[attribute_nr], attribute_nr);
        }
    }

    public void removeAttributeButtons()
    {
        while (listButtons.Count != 0)
        {
            GameObject button = listButtons[0];
            listButtons.RemoveAt(0);
            Destroy(button);
        }
    }

    private void onClickListButton(int index)
    {
        ColorUtility.TryParseHtmlString("#383838", out buttonDefaultColor);

        selectedAttribute = index;

        for (int button_nr = 0; button_nr < listButtons.Count; button_nr++)
        {
            if (button_nr == index)
            {
                listButtons[button_nr].GetComponent<Image>().color = Color.black;
                editAttributeButton.interactable = true;
                removeAttributeButton.interactable = true;
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
}
