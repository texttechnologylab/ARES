using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class TokenSearch : MonoBehaviour
{
    private List<GameObject> listButtons = new List<GameObject>();
    public GameObject listButtonPrefab;
    private Color buttonDefaultColor;
    private int selectedToken = -1;

    public GameObject tokenContent;
    public InputField tokenIndex;

    public Button selectButton;
    public GameObject tokenSearchCanvas;
    public void displayTokens()
    {
        tokenSearchCanvas.SetActive(true);
        removeTokenButtons();
        addTokenButtons();
        selectButton.interactable = false;
        selectedToken = -1;
    }

    public void cancelSelection()
    {
        displayTokens();
        tokenSearchCanvas.SetActive(false);
    }

    public void selectSelection()
    {
        tokenIndex.text = selectedToken.ToString();
        displayTokens();
        tokenSearchCanvas.SetActive(false);
    }

    public void addTokenButtons()
    {
        for (int token_nr = 0; token_nr < DragDropParameters.currScene.tokens.Length; token_nr++)
        {
            string token = DragDropParameters.currScene.tokens[token_nr];
            GameObject listButton =  Instantiate(listButtonPrefab, new Vector3(0, 0, 0), Quaternion.identity);
            listButton.GetComponentInChildren<TMP_Text>().text = new string(' ', 2) + token_nr.ToString() + ": " + token;
            listButton.transform.parent = tokenContent.transform;
            listButton.transform.localScale = new Vector3(1f, 1f, 1f);
            listButtons.Add(listButton);
            if (entityIndexExists(token_nr))
            {
                listButton.GetComponent<Button>().interactable = false;
            }
            buttonEvent(listButtons[token_nr], token_nr);
        }
    }

    public void removeTokenButtons()
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

        selectedToken = index;

        for (int button_nr = 0; button_nr < listButtons.Count; button_nr++)
        {
            if (button_nr == index)
            {
                listButtons[button_nr].GetComponent<Image>().color = Color.black;
                selectButton.interactable = true;
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

    private bool entityIndexExists(int index)
    {
        foreach (GameObject entityObject in DragDropParameters.entityList)
        {
            ExtractJsonScene.Entity entity = entityObject.GetComponent<DragAndDrop>().entity;
            if (entity.index == index)
            {
                return true;
            }
        }
        return false;
    }
}
