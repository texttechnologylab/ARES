using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SearchIdMenu : MonoBehaviour
{
    public GameObject canvasSearchId;
    public GameObject entitySearchResultPrefab;
    public GameObject searchResultContent;
    public GameObject selectButton;
    public GameObject inputTextField;
    public GameObject sceneHandler;
    public GameObject entityIdInputField;
    public GameObject lastPageButton;
    public GameObject nextPageButton;
    public GameObject pageText;

    private List<GameObject> entityButtonList = new List<GameObject>();
    private string selectedId = "";
    private Color buttonDefaultColor;
    private ShapeNetHandler snh;
    private List<string> resultIdList = new List<string>();
    private int currentPage = 0;

    private void resetMenu()
    {
        selectButton.GetComponent<Button>().interactable = false;
        selectedId = "";
        while (entityButtonList.Count != 0)
        {
            GameObject button = entityButtonList[0];
            entityButtonList.RemoveAt(0);
            Destroy(button);
        }
        resultIdList = new List<string>();
        currentPage = 0;
        adjustDisplayPageSettings();
    }
    public void openSearchIdMenu()
    {
        canvasSearchId.SetActive(true);
        resetMenu();
    }
    public void selectId()
    {
        entityIdInputField.GetComponent<InputField>().text = selectedId;
        canvasSearchId.SetActive(false);
        resetMenu();
    }
    public void cancelSelection()
    {
        selectButton.GetComponent<Button>().interactable = false;
        while (entityButtonList.Count != 0)
        {
            GameObject button = entityButtonList[0];
            entityButtonList.RemoveAt(0);
            Destroy(button);
        }
        canvasSearchId.SetActive(false);
        resetMenu();
    }
    public void searchEntities()
    {
        resetMenu();
        string searchName= inputTextField.GetComponent<InputField>().text;
        foreach (string id in GlobalVariable.ShapeNetObjects.Keys)
        {
            ShapeNetModel snm = GlobalVariable.ShapeNetObjects[id];
            if (snm.ContainsTag(searchName))
            {
                resultIdList.Add(id);
            }
        }
        displayPage();
        adjustDisplayPageSettings();
    }

    public void displayPage()
    {
        while (entityButtonList.Count != 0)
        {
            GameObject button = entityButtonList[0];
            entityButtonList.RemoveAt(0);
            Destroy(button);
        }
        for (int idNr = Mathf.Min(Settings.maxEntitySearchResults * currentPage, resultIdList.Count); idNr < Mathf.Min(Settings.maxEntitySearchResults * (currentPage+1), resultIdList.Count); idNr++)
        {
            string id = resultIdList[idNr];
            ShapeNetModel snm = GlobalVariable.ShapeNetObjects[id];
            GameObject listButton =  Instantiate(entitySearchResultPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            EntitySearchResultButton esrb = listButton.AddComponent<EntitySearchResultButton>();
            esrb.id = id;

            ShapeNetObject sObj = GlobalVariable.ShapeNetObjects[id];
            if (sObj.Thumbnail != null)
            {
                Texture2D texture = sObj.Thumbnail;
                if (texture != null)
                {
                    listButton.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                }
            }
            else
            {
                snh.GetThumbnail(id);
            }
            listButton.transform.Find("Name").GetComponent<Text>().text = snm.Name;
            listButton.transform.Find("Annotations").GetComponent<Text>().text = string.Join(", ", snm.Tags);
            listButton.transform.Find("Id").GetComponent<Text>().text = id;
            listButton.transform.parent = searchResultContent.transform;
            listButton.transform.localScale = new Vector3(1f, 1f, 1f);
            entityButtonList.Add(listButton);
            buttonEventEntity(listButton, id);
        }
    }

    public void adjustDisplayPageSettings()
    {
        pageText.GetComponent<TMP_Text>().text = "-" + currentPage + "-";
        if (currentPage > 0)
        {
            lastPageButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            lastPageButton.GetComponent<Button>().interactable = false;
        }
        if (Settings.maxEntitySearchResults * (currentPage+1) < resultIdList.Count)
        {
            nextPageButton.GetComponent<Button>().interactable = true;
        }
        else
        {
            nextPageButton.GetComponent<Button>().interactable = false;
        }
    }

    public void gotToLastPage()
    {
        currentPage--;
        displayPage();
        adjustDisplayPageSettings();
    }

    public void goToNextPage()
    {
        currentPage++;
        displayPage();
        adjustDisplayPageSettings();
    }

    private void buttonEventEntity(GameObject curButton, string id)
    {
        curButton.GetComponent<UnityEngine.UI.Button>().onClick.AddListener(delegate {onClickListButtonEntity(curButton, id); });
    }

    private void onClickListButtonEntity(GameObject curButton, string id)
    {
        selectedId = id;
        ColorUtility.TryParseHtmlString("#383838", out buttonDefaultColor);
        for (int button_nr = 0; button_nr < entityButtonList.Count; button_nr++)
        {
            if (GameObject.ReferenceEquals(curButton, entityButtonList[button_nr]))
            {
                entityButtonList[button_nr].GetComponent<Image>().color = Color.black;
                selectButton.GetComponent<Button>().interactable = true;
            }
            else
            {
                entityButtonList[button_nr].GetComponent<Image>().color = buttonDefaultColor;
            }
        }
    }

    void Start()
    {
        snh = sceneHandler.GetComponent<ShapeNetHandler>();
    }

    void Update() 
    {
        foreach (GameObject button in entityButtonList)
        {
            EntitySearchResultButton esrb = button.GetComponent<EntitySearchResultButton>();
            if (!esrb.imageAttached)
            {
                string id = esrb.id;
                try{
                ShapeNetObject sObj = GlobalVariable.ShapeNetObjects[id];
                if (sObj.Thumbnail != null)
                {
                    Texture2D texture = sObj.Thumbnail;
                    if (texture != null)
                    {
                        button.transform.Find("Image").GetComponent<Image>().sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
                        esrb.imageAttached = true;
                    }
                }
                }
                catch
                {
                    esrb.imageAttached = true;
                }
            }
        }
    }
}
