using UnityEngine;
using UnityEngine.SceneManagement;

public class NextLevel : MonoBehaviour
{
    public string nextSceneName;

    public void GoToNextLevel()
    {
        SceneManager.LoadScene(nextSceneName);
    }
}
