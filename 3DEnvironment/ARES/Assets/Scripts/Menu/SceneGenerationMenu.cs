using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.UI;
using UnityEngine.SceneManagement;



public class SceneGenerationMenu : MonoBehaviour
{
    public GameObject inputText;
    public GameObject analyzeTextButton;
    public GameObject resetButton;
    public GameObject visualizeButton;
    public GameObject infoPanel;
    public GameObject pending;
    public GameObject jsonOutput;
    public GameObject dragAndDropMenu;

    public Toggle physicsToogle;

    private int pendingCounter = 0;
    private NLPHandler nlpHandler;

    enum State {NotAnalyzed, Pending, Analyzed};
    State currState = State.NotAnalyzed;

    public void analyzeText()
    {
        string text = inputText.GetComponent<TMP_InputField>().text;
        currState = State.Pending;
        pending.SetActive(true);
        nlpHandler = (new GameObject("requestHandler")).AddComponent<NLPHandler>();
        nlpHandler.SendRequest(text);
    }

    public void resetText()
    {
        inputText.GetComponent<TMP_InputField>().text = "";
        GlobalVariable.scenesForVisualization = null;
        GlobalVariable.inputText = null;
        infoPanel.SetActive(false);
        dragAndDropMenu.SetActive(false);
        analyzeTextButton.GetComponent<Button>().interactable = true;
        resetButton.GetComponent<Button>().interactable = false;
        visualizeButton.GetComponent<Button>().interactable = false;
        currState = State.Analyzed;
    }

    public void backToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void vizualize()
    {
        GlobalVariable.physicsActivated = physicsToogle.isOn;
        SceneManager.LoadScene("SceneGeneration");
    }

    private void Start() 
    {
        if (GlobalVariable.relations.result == null)
        {
            RelationMenu.getRelationsJson();
        }
        if (GlobalVariable.scenesForVisualization != null)
        {
            pending.SetActive(false);
            inputText.GetComponent<TMP_InputField>().text = GlobalVariable.inputText;
            jsonOutput.GetComponent<UnityEngine.UI.Text>().text = GlobalVariable.scenesForVisualization.getPrettyPrint();
            infoPanel.SetActive(true);
            dragAndDropMenu.SetActive(true);
            analyzeTextButton.GetComponent<Button>().interactable = false;
            resetButton.GetComponent<Button>().interactable = true;
            visualizeButton.GetComponent<Button>().interactable = true;
            currState = State.Analyzed;
        }
        InvokeRepeating("animatePending", 0f, 1f);
    }

    private void animatePending()
    {
        pendingCounter = pendingCounter < 3 ? pendingCounter + 1 : 0;
    }

    private void Update() 
    {
        if (currState == State.Pending)
        {
            pending.GetComponent<TextMeshProUGUI>().text = "Pending" + new string('.',pendingCounter);
        }
        try
        {
            if (nlpHandler.scenes != null && currState == State.Pending)
            {

                pending.SetActive(false);
                jsonOutput.GetComponent<UnityEngine.UI.Text>().text = nlpHandler.scenes.getPrettyPrint();
                infoPanel.SetActive(true);
                dragAndDropMenu.SetActive(true);
                analyzeTextButton.GetComponent<Button>().interactable = false;
                resetButton.GetComponent<Button>().interactable = true;
                visualizeButton.GetComponent<Button>().interactable = true;
                currState = State.Analyzed;
                GlobalVariable.scenesForVisualization = nlpHandler.scenes;
                GlobalVariable.inputText = inputText.GetComponent<TMP_InputField>().text;
            }
        }
        catch (Exception e)
        {
        }
    }
}
