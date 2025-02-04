using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneUtility : MonoBehaviour
{


    public void LoadScene(int index)
    {
        SceneManager.LoadScene(index);
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
    public void Quit()
    {
        Application.Quit();
    }
}
