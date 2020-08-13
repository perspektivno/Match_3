using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class BoardSetting
{
    public int xSize, ySize;
    public Tile tileGO;
    public Image img;
    public List<Sprite> tileSprite;
}

public class GameManager : MonoBehaviour
{
    public BoardSetting boardSetting;
    void Start()
    {
        BoardController.instance.Initialize(boardSetting);
    }

}
