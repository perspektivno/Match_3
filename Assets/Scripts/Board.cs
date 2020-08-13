using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Board : MonoBehaviour
{
    public static Board instance;
    private int xSize, ySize;
    private Tile tileGO;
    private List<Sprite> tileSprite = new List<Sprite>();

    private void Awake()
    {
        instance = this;
    }
    public Tile SetValue2(Tile tileGO)
    {
        this.tileGO = tileGO;
        return CreateTile();
    }

    public void SetImg(Image img, List<Sprite> tileSprite)
    {
        List<Sprite> tempSprite = new List<Sprite>();
        tempSprite.AddRange(tileSprite);
        img.sprite = tempSprite[Random.Range(0, tempSprite.Count)];
    }

    public Tile CreateTile()
    {
        Tile newTile = Instantiate(tileGO, transform.position, Quaternion.identity);
        newTile.transform.position = new Vector2(100, 100);
        newTile.spriteRenderer.sprite = null;
        return newTile;
    }

    public Tile[,] CreateBoard(BoardSetting boardSetting)
    {
        this.xSize = boardSetting.xSize;
        this.ySize = boardSetting.ySize;
        this.tileGO = boardSetting.tileGO;
        this.tileSprite = boardSetting.tileSprite;
        
        Tile[,] tileArray = new Tile[xSize, ySize];

        float xPos = transform.position.x;
        float yPos = transform.position.y;
        Vector2 tileSize = tileGO.spriteRenderer.bounds.size;

        for (int i = 0; i < xSize; i++)
        {
            for (int j = 0; j < ySize; j++)
            {
                Tile newTile = Instantiate(tileGO, transform.position, Quaternion.identity);
                newTile.transform.position = new Vector2(xPos + (tileSize.x * i), yPos + (tileSize.y * j));
                newTile.transform.parent = transform;

                tileArray[i, j] = newTile;
                List<Sprite> tempSprite = new List<Sprite>();
                tempSprite.AddRange(tileSprite);
                if (i % 2 == 0 && i != 0)
                {
                    tempSprite.Remove(tileArray[i - 1, j].spriteRenderer.sprite);
                }
                if (j % 2 == 0 && j != 0)
                {
                    tempSprite.Remove(tileArray[i, j - 1].spriteRenderer.sprite);
                }
                newTile.spriteRenderer.sprite = tempSprite[Random.Range(0, tempSprite.Count)];
                newTile.name = i.ToString() + j.ToString();
            }
        }
        return tileArray;
    }
}
