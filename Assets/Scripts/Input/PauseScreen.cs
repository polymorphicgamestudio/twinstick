using ShepProject;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class PauseScreen : MonoBehaviour
{

    public int mainSceneIndex;
    public Button exitButton;
    public GameObject background;

    // Start is called before the first frame update
    void Start()
    {
        ShepGM.inst.Input.pauseEvent += OnPause;
        exitButton.onClick.AddListener(ExitToMainMenu);

    }

    private void OnPause(bool value)
    {

        background.SetActive(value);


    }

    private void ExitToMainMenu()
    {

        SceneManager.LoadScene(mainSceneIndex);

    }


}
