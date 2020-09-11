using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIScripts : MonoBehaviour
{
    //Slider for path points count
    public Slider pointsSlider;
    public TextMeshProUGUI pointsText;

    //Slider for path time value
    public Slider timeSlider;
    public TextMeshProUGUI timeText;

    //Main menu
    public GameObject mainMenu;

    //Generate menu
    public GameObject generateMenu;

    //Loading screen
    public GameObject loadingScreen;

    //Generating screen
    public GameObject generateScreen;

    //Downloading path while loading screen is shown
    //After download the path scene will shown (call inside Download coroutine)
    public void PlayPath()
    {
        StartCoroutine(WebRequests.Download());
        generateMenu.SetActive(false);
        mainMenu.SetActive(false);
        loadingScreen.SetActive(true);
    }

    public void RestartPath()
    {
        SceneManager.LoadScene("Path");
    }

    public void ExitGame()
    {
        Application.Quit();
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("Menu");
    }

    public void Update()
    {
        //Updating UI texts for sliders value
        if (SceneManager.GetActiveScene().name == "Menu")
        {
            pointsText.text = pointsSlider.value.ToString();
            timeText.text = timeSlider.value.ToString();
        }
    }

    //Some loading screen while uploading path to Dropbox
    public void UploadPath()
    {
        StartCoroutine(IUploadPath());
    }

    private IEnumerator IUploadPath()
    {
        generateMenu.SetActive(false);
        generateScreen.SetActive(true);

        //Wait 4s hardcoded based on average uploading to Dropbox time + 1s
        yield return new WaitForSeconds(4f);

        generateMenu.SetActive(true);
        generateScreen.SetActive(false);
    }
}
