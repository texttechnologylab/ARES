using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UserMenu : MonoBehaviour
{
    public Camera mainCam;
    public Camera characterCam;
    public GameObject debugMenu;
    public GameObject userMenu;

    public GameObject pointer;
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.C))
        {
            Cursor.lockState = mainCam.enabled ? CursorLockMode.Locked : CursorLockMode.None;
            mainCam.enabled = !mainCam.enabled;
            characterCam.enabled = !characterCam.enabled;
            debugMenu.SetActive(mainCam.enabled);
            userMenu.SetActive(characterCam.enabled);
            pointer.SetActive(characterCam.enabled);
        }
    }
}
