using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;

public class LoadScene : MonoBehaviour
{
    private bool checkSound;
    public Text sound;
    public void RestartScene()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    public void SceneLoad(string name)
    {
        SceneManager.LoadScene(name);
    }
    public void Sound()
    {
        checkSound = !checkSound;
        if (checkSound)
        {
            AudioListener.volume = 0;
            sound.text = "Sound: On";
        }
        else
        {
            AudioListener.volume = 1;
            sound.text = "Sound: Off";
        }

    }
    

}
