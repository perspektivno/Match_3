using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public static UI instance;
    public Text textScore, textMoves;
    private int score = 15;
    private int moves = 15;
    public GameObject panelGameOver;
    public Text resultText;
    public AudioSource winSound,  lostSound, gameSound;
    private void Awake()
    {
        instance = this;
    }
    public void Score(int x)
    {
        score -= x;
        if (score < 1)
        {
            textScore.text = "0";
            GameOver();
            resultText.text = "YOU WON!";
            resultText.color = Color.green;
            gameSound.Stop();
            winSound.Play();
        }
        else
        {
            textScore.text = score.ToString();

        }

    }

    public void Moves(int x)
    {
        textMoves.text = moves.ToString();
        moves -= x;
        if(moves<=0 && textScore.text != "0")
        {
            GameOver();
            resultText.text = "YOU LOST!";
            resultText.color = Color.red;
            gameSound.Stop();
            lostSound.Play();
        }
        textMoves.text = "Moves: " + moves.ToString();
    }

    public void GameOver()
    {
        panelGameOver.SetActive(true);
    }
    public void Pause()
    {
        panelGameOver.SetActive(true);
    }
}
