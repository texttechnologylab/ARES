using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MainSettingsMenu : MonoBehaviour
{
    public GameObject subMenus;
    public GameObject textAnalysisMenu;
    public GameObject modelDatabaseMenu;
    public GameObject sceneAlgorithmMenu;
    public GameObject sceneBuilderMenu;
    public GameObject loginCredentialsMenu;
    public InputField apiEndpointInputField;
    public Dropdown modelDropDown;
    public InputField errorScoreGroundMovementInputField;
    public InputField errorScoreRotationMovementInputField;
    public InputField errorScorePositiveHeightMovementInputField;
    public InputField errorScoreNegativeHeightMovementInputField;
    public Toggle useDebugMode;
    public Toggle useCustomAttributes;
    public InputField maxSearchResultPageInputField;

    public void backToMainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    public void openTextAnalysis()
    {
        subMenus.SetActive(false);
        textAnalysisMenu.SetActive(true);
    }
    public void saveTextAnalysis()
    {
        subMenus.SetActive(true);
        textAnalysisMenu.SetActive(false);
        Settings.apiEndpoint = apiEndpointInputField.text;
        Settings.model = modelDropDown.options[modelDropDown.value].text;
    }

    public void openModelDatabase()
    {
        subMenus.SetActive(false);
        modelDatabaseMenu.SetActive(true);
    }
    public void saveModelDatabase()
    {
        subMenus.SetActive(true);
        modelDatabaseMenu.SetActive(false);
    }

    public void openSceneAlgorithm()
    {
        subMenus.SetActive(false);
        sceneAlgorithmMenu.SetActive(true);
    }
    public void saveSceneAlgorithm()
    {
        subMenus.SetActive(true);
        sceneAlgorithmMenu.SetActive(false);
        Settings.errorScoreGroundMovement = float.Parse(errorScoreGroundMovementInputField.text);
        Settings.errorScoreRotationMovement = float.Parse(errorScoreRotationMovementInputField.text);
        Settings.errorScorePositiveHeightMovement = float.Parse(errorScorePositiveHeightMovementInputField.text);
        Settings.errorScoreNegativeHeightMovement = float.Parse(errorScoreNegativeHeightMovementInputField.text);
        Settings.debugMode = useDebugMode.isOn;
        Settings.customAttributes = useCustomAttributes.isOn;
    }

    public void openSceneBuilder()
    {
        subMenus.SetActive(false);
        sceneBuilderMenu.SetActive(true);
    }
    public void saveSceneBuilder()
    {
        subMenus.SetActive(true);
        sceneBuilderMenu.SetActive(false);
        Settings.maxEntitySearchResults = int.Parse(maxSearchResultPageInputField.text);
    }

    public void openLoginCredentials()
    {
        subMenus.SetActive(false);
        loginCredentialsMenu.SetActive(true);
    }
    public void saveLoginCredentials()
    {
        subMenus.SetActive(true);
        loginCredentialsMenu.SetActive(false);
    }

    private void Start()
    {
        apiEndpointInputField.text = Settings.apiEndpoint;
        modelDropDown.value = modelDropDown.options.FindIndex(option => option.text == Settings.model);

        errorScoreGroundMovementInputField.text = Settings.errorScoreGroundMovement.ToString();
        errorScoreRotationMovementInputField.text = Settings.errorScoreRotationMovement.ToString();
        errorScorePositiveHeightMovementInputField.text = Settings.errorScorePositiveHeightMovement.ToString();
        errorScoreNegativeHeightMovementInputField.text = Settings.errorScoreNegativeHeightMovement.ToString();
        useDebugMode.isOn = Settings.debugMode;
        useCustomAttributes.isOn = Settings.customAttributes;

        maxSearchResultPageInputField.text = Settings.maxEntitySearchResults.ToString();
    }
}
