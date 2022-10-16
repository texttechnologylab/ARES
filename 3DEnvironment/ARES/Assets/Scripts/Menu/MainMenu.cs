using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    void Start()
    {
        Cursor.lockState = CursorLockMode.None;
    }
    public void setAttributes()
    {
        SceneManager.LoadScene("AttributeCreation");
    }
    public void setRelations()
    {
        SceneManager.LoadScene("RelationMenu");
    }
    public void generateScene()
    {
        SceneManager.LoadScene("GenerateSceneMenu");
    }
    public void settings()
    {
        SceneManager.LoadScene("MainSettingsMenu");
    }
    public void exit()
    {
        Application.Quit();
    }
}
