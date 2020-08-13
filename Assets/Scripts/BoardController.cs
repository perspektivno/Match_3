using System;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;
using UnityEditor.Experimental.AssetImporters;
using System.Transactions;
using System.Linq.Expressions;
public class BoardController : MonoBehaviour
{
    public static BoardController instance;
    public int xSize, ySize;
    private Color colorTile0, colorTile1;
    private List<Sprite> tileSprite = new List<Sprite>();
    private Tile[,] tileArray;
    private Tile tile;
    private bool swapRunning;
    private Image img;
    private Tile oldSelectionTile;
    private List<Tile> forInvoke = new List<Tile>();
    public void Initialize(BoardSetting boardSetting)
    {
        var board = Board.instance.CreateBoard(boardSetting);
     
        this.tileArray = board;
        this.xSize = boardSetting.xSize;
        this.ySize = boardSetting.ySize;
        this.tileSprite = boardSetting.tileSprite;
        this.tile = Board.instance.SetValue2(boardSetting.tileGO);
        this.img = boardSetting.img;
        Board.instance.SetImg(this.img, boardSetting.tileSprite);
    }

    public void SetValue(Tile[,] tileArray, int xSize, int ySize, List<Sprite> tileSprite, Tile tile, Image img)
    {
        this.tileArray = tileArray;
        this.xSize = xSize;
        this.ySize = ySize;
        this.tileSprite = tileSprite;
        this.tile = tile;
        this.img = img;

    }
    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        if (Input.GetMouseButtonDown(0) && swapRunning == false)
        {
            RaycastHit2D ray = Physics2D.GetRayIntersection(Camera.main.ScreenPointToRay(Input.mousePosition));
            if (ray != false)
            {
                CheckSelectionTile(ray.collider.gameObject.GetComponent<Tile>());
            } 
        }
    }

    private void SwapTwoTiles(Tile tile)
    {
        swapRunning = true;
        if (oldSelectionTile.spriteRenderer.sprite == tile.spriteRenderer.sprite)
        {
            return;
        }
        List<Tile> cashFindMatch = new List<Tile>();
        Sprite cashSprite = oldSelectionTile.spriteRenderer.sprite;

        oldSelectionTile.spriteRenderer.sprite = tile.spriteRenderer.sprite;
        tile.spriteRenderer.sprite = cashSprite;

        SearchForMatches(cashFindMatch);
        if (cashFindMatch.Count > 0)
        {
            MakeNulls(cashFindMatch);

            CancelInvoke("HelpPlayerSetColor");
            CancelInvoke("HelpPlayerResetColor");
            Invoke("HelpPlayerSetColor", 7);

            List<Tile> cashNulls = new List<Tile>();
            SearchForNulls(cashNulls);
            while (cashNulls.Count > 0)
            {
                cashFindMatch.Clear();
                TilesFallDown();

                SearchForMatches(cashFindMatch);

                MakeNulls(cashFindMatch);

                SearchForNulls(cashNulls);
            }
            UI.instance.Moves(1);
        }
        else
        {
            tile.spriteRenderer.sprite = oldSelectionTile.spriteRenderer.sprite;
            oldSelectionTile.spriteRenderer.sprite = cashSprite;
            return;
        }
        if (SearchForMoves().Count == 0)
        {
            var gameManager = FindObjectOfType<GameManager>();
            for (int i = 0; i < xSize; i++)
            {
                for (int j = 0; j < ySize; j++)
                {
                    tileArray[i, j].spriteRenderer.sprite = null;

                }
            }
            Initialize(gameManager.boardSetting);
        }
        swapRunning = false;
    }
    private void HelpPlayerSetColor()
    {
        SearchForMoves();
        if (forInvoke.Count > 0)
        {
            colorTile0 = forInvoke[0].spriteRenderer.color;
            colorTile1 = forInvoke[1].spriteRenderer.color;
            forInvoke[0].spriteRenderer.color = Color.green;
            forInvoke[1].spriteRenderer.color = Color.green;
            Invoke("HelpPlayerResetColor", 0.3f);
        }
    }
    private void HelpPlayerResetColor()
    {
        forInvoke[0].spriteRenderer.color = colorTile0;
        forInvoke[1].spriteRenderer.color = colorTile1;
    }

    private List<Tile> SearchForMoves()
    {
        List<Tile> Cash = new List<Tile>();
        Cash.Clear();
        for(int i = 0; i < ySize-1; i++)
        {
            for(int j = 0; j < xSize-1; j++)
            {
                List<Tile> SearchFor = new List<Tile>();
                if (tileArray[i, j].spriteRenderer.sprite != tileArray[i, j + 1].spriteRenderer.sprite)
                {
                    tile.spriteRenderer.sprite = tileArray[i, j].spriteRenderer.sprite;
                    tileArray[i, j].spriteRenderer.sprite = tileArray[i, j + 1].spriteRenderer.sprite;
                    tileArray[i, j + 1].spriteRenderer.sprite = tile.spriteRenderer.sprite;
                    if (SearchForMatches(SearchFor).Count > 0)
                    {
                        Cash.Add(tileArray[i, j]);
                        Cash.Add(tileArray[i, j + 1]);
                    }
                    tileArray[i, j + 1].spriteRenderer.sprite = tileArray[i, j].spriteRenderer.sprite;
                    tileArray[i, j].spriteRenderer.sprite = tile.spriteRenderer.sprite;
                }

                if (tileArray[j, i].spriteRenderer.sprite != tileArray[j + 1, i].spriteRenderer.sprite)
                {
                    tile.spriteRenderer.sprite = tileArray[j, i].spriteRenderer.sprite;
                    tileArray[j, i].spriteRenderer.sprite = tileArray[j + 1, i].spriteRenderer.sprite;
                    tileArray[j + 1, i].spriteRenderer.sprite = tile.spriteRenderer.sprite;
                    if (SearchForMatches(SearchFor).Count > 0)
                    {
                        Cash.Add(tileArray[j, i]);
                        Cash.Add(tileArray[j + 1, i]);
                    }
                    tileArray[j + 1, i].spriteRenderer.sprite = tileArray[j, i].spriteRenderer.sprite;
                    tileArray[j, i].spriteRenderer.sprite = tile.spriteRenderer.sprite;
                }
                
            }
        }
        forInvoke.Clear();
        for (int i = 0; i < Cash.Count; i++)
        {
            forInvoke.Add(Cash[i]);
        }
        return Cash;
    }

    private List<Tile> SearchForMatches(List<Tile> cashFindMatch)
    {

        for (int i = 0; i < ySize; i++)
        {
            for (int j = 0; j < xSize-2; j++)
            {
                if (tileArray[j, i].spriteRenderer.sprite == tileArray[j + 1, i].spriteRenderer.sprite &&
                    tileArray[j + 1, i].spriteRenderer.sprite == tileArray[j + 2, i].spriteRenderer.sprite)
                {
                    cashFindMatch.Add(tileArray[j, i]);
                    cashFindMatch.Add(tileArray[j + 1, i]);
                    cashFindMatch.Add(tileArray[j + 2, i]);
                }
                if (tileArray[i, j].spriteRenderer.sprite == tileArray[i, j + 1].spriteRenderer.sprite &&
                    tileArray[i, j + 1].spriteRenderer.sprite == tileArray[i, j + 2].spriteRenderer.sprite)
                {
                    cashFindMatch.Add(tileArray[i, j]);
                    cashFindMatch.Add(tileArray[i, j + 1]);
                    cashFindMatch.Add(tileArray[i, j + 2]);
                }
            }
        }
        for (int i = 0; i < ySize; i++)
        {
            for (int j = 0; j < xSize - 3; j++)
            {
                if (tileArray[j, i].spriteRenderer.sprite == tileArray[j + 1, i].spriteRenderer.sprite &&
                    tileArray[j + 1, i].spriteRenderer.sprite == tileArray[j + 2, i].spriteRenderer.sprite &&
                    tileArray[j + 2, i].spriteRenderer.sprite == tileArray[j + 3, i].spriteRenderer.sprite)
                {
                    cashFindMatch.Add(tileArray[j + 3, i]);
                }
                if (tileArray[i, j].spriteRenderer.sprite == tileArray[i, j + 1].spriteRenderer.sprite &&
                    tileArray[i, j + 1].spriteRenderer.sprite == tileArray[i, j + 2].spriteRenderer.sprite &&
                    tileArray[i, j + 2].spriteRenderer.sprite == tileArray[i, j + 3].spriteRenderer.sprite)
                {
                    cashFindMatch.Add(tileArray[i, j + 3]);
                }
            }
        }
        for (int i = 0; i < ySize; i++)
        {
            for (int j = 0; j < xSize - 4; j++)
            {
                if (tileArray[j, i].spriteRenderer.sprite == tileArray[j + 1, i].spriteRenderer.sprite &&
                    tileArray[j + 1, i].spriteRenderer.sprite == tileArray[j + 2, i].spriteRenderer.sprite &&
                    tileArray[j + 2, i].spriteRenderer.sprite == tileArray[j + 3, i].spriteRenderer.sprite &&
                    tileArray[j + 3, i].spriteRenderer.sprite == tileArray[j + 4, i].spriteRenderer.sprite)
                {
                    cashFindMatch.Add(tileArray[j + 4, i]);
                }
                if (tileArray[i, j].spriteRenderer.sprite == tileArray[i, j + 1].spriteRenderer.sprite &&
                    tileArray[i, j + 1].spriteRenderer.sprite == tileArray[i, j + 2].spriteRenderer.sprite &&
                    tileArray[i, j + 2].spriteRenderer.sprite == tileArray[i, j + 3].spriteRenderer.sprite &&
                    tileArray[i, j + 3].spriteRenderer.sprite == tileArray[i, j + 4].spriteRenderer.sprite)
                {
                    cashFindMatch.Add(tileArray[i, j + 4]);
                }
            }
        }
        for (int i = 0; i < ySize; i++)
        {
            for (int j = 0; j < xSize - 5; j++)
            {
                if (tileArray[j, i].spriteRenderer.sprite == tileArray[j + 1, i].spriteRenderer.sprite &&
                    tileArray[j + 1, i].spriteRenderer.sprite == tileArray[j + 2, i].spriteRenderer.sprite &&
                    tileArray[j + 2, i].spriteRenderer.sprite == tileArray[j + 3, i].spriteRenderer.sprite &&
                    tileArray[j + 3, i].spriteRenderer.sprite == tileArray[j + 4, i].spriteRenderer.sprite &&
                    tileArray[j + 4, i].spriteRenderer.sprite == tileArray[j + 5, i].spriteRenderer.sprite)
                {
                    cashFindMatch.Add(tileArray[j + 5, i]);
                }
                if (tileArray[i, j].spriteRenderer.sprite == tileArray[i, j + 1].spriteRenderer.sprite &&
                    tileArray[i, j + 1].spriteRenderer.sprite == tileArray[i, j + 2].spriteRenderer.sprite &&
                    tileArray[i, j + 2].spriteRenderer.sprite == tileArray[i, j + 3].spriteRenderer.sprite &&
                    tileArray[i, j + 3].spriteRenderer.sprite == tileArray[i, j + 4].spriteRenderer.sprite &&
                    tileArray[i, j + 4].spriteRenderer.sprite == tileArray[i, j + 5].spriteRenderer.sprite)
                {
                    cashFindMatch.Add(tileArray[i, j + 5]);
                }
            }
        }

        cashFindMatch = cashFindMatch.Distinct().ToList();
        return cashFindMatch;
    }
    

    private List<Tile> MakeNulls(List<Tile> cashFindMatch)
    {
        for (int i = 0; i < cashFindMatch.Count; i++)
        {
            if (cashFindMatch[i].spriteRenderer.sprite != null && cashFindMatch[i].spriteRenderer.sprite.name ==
                img.sprite.name)
            {
                UI.instance.Score(1);
            }
            cashFindMatch[i].spriteRenderer.sprite = null;

        }
        return cashFindMatch;
    }
    
    private List<Tile> SearchForNulls(List<Tile> cashNulls)
    {
        cashNulls.Clear();
        for(int i = 0; i < ySize; i++)
        {
            for (int j = 0; j < xSize; j++)
            {
                if (tileArray[i,j].spriteRenderer.sprite == null)
                {
                    cashNulls.Add(tileArray[i, j]);
                }
            }
        }
        return cashNulls;
    }
    private void TilesFallDown()
    {
        List<Tile> cashNulls = new List<Tile>();
        SearchForNulls(cashNulls);
        List<Sprite> renderer = new List<Sprite>();
        renderer.AddRange(tileSprite);
        while (cashNulls.Count > 0)
        {
            int nullsInColumn = 0;
            char[] firstNull = cashNulls[0].name.ToCharArray();
            int firstNumericFirst = (int)Char.GetNumericValue(firstNull[0]);
            int secondNumericFirst = (int)Char.GetNumericValue(firstNull[1]);
            
            for(int i = 0; i< cashNulls.Count; i++)
            {
                char[] anotherNull = cashNulls[i].name.ToCharArray();
                int firstNumericAnother = (int)Char.GetNumericValue(anotherNull[0]);
                int secondNumericAnother = (int)Char.GetNumericValue(anotherNull[1]);
                if(firstNumericAnother == firstNumericFirst && secondNumericFirst > secondNumericAnother)
                {
                    firstNumericFirst = (int)Char.GetNumericValue(anotherNull[0]);
                    secondNumericFirst = (int)Char.GetNumericValue(anotherNull[1]); 
                }
            }
            for (int i = 0; i < ySize; i++)
            {
                if (tileArray[firstNumericFirst, i].spriteRenderer.sprite == null)
                {
                    nullsInColumn++;
                }
            }
                for (int i = 0; i < nullsInColumn; i++)
                {
                    for (int k = 0; k < (ySize - secondNumericFirst - 1); k++)
                    {
                        tileArray[firstNumericFirst, secondNumericFirst + k].spriteRenderer.sprite = 
                        tileArray[firstNumericFirst, secondNumericFirst + k + 1].spriteRenderer.sprite;
                    }
                    tileArray[firstNumericFirst, ySize-1].spriteRenderer.sprite = renderer[Random.Range(0, renderer.Count)];
                }  
                for (int i = 0; i < cashNulls.Count; i++)
                {
                    char[] temp = cashNulls[i].name.ToCharArray();
                    int tempNumeric = (int)Char.GetNumericValue(temp[0]);
                    if (tempNumeric == firstNumericFirst)
                    {
                    cashNulls.RemoveAt(i);
                        i = -1;
                    }
                }
        }
        
    }
    private bool AdjacentTiles(Tile tile)
    {
        char[] firstTile = tile.name.ToCharArray();
        int firstNumericFirstTile = (int)Char.GetNumericValue(firstTile[0]);
        int secondNumericFirstTile = (int)Char.GetNumericValue(firstTile[1]);
        char[] secondTile = oldSelectionTile.name.ToCharArray();
        int firstNumericSecondTile = (int)Char.GetNumericValue(secondTile[0]);
        int secondNumericSecondTile = (int)Char.GetNumericValue(secondTile[1]);

        if ((firstNumericFirstTile == firstNumericSecondTile && Mathf.Abs(secondNumericFirstTile - secondNumericSecondTile) ==1)||
            (secondNumericFirstTile == secondNumericSecondTile && Mathf.Abs(firstNumericFirstTile - firstNumericSecondTile) == 1))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void SelectTile(Tile tile)
    {
        tile.isSelected = true;
        tile.spriteRenderer.color = new UnityEngine.Color(0.4f, 0.4f, 0.4f);
        oldSelectionTile = tile;
    }

    private void DeselectTile(Tile tile)
    {
        tile.isSelected = false;
        tile.spriteRenderer.color = new UnityEngine.Color(1, 1, 1);
        oldSelectionTile = null;
    }

    private void CheckSelectionTile(Tile tile)
    {
        if (tile.isEmpty)
        {
            return;
        }
        if (tile.isSelected)
        {
            DeselectTile(tile);
        }
        else
        {
            if (!tile.isSelected && oldSelectionTile == null)
            {
                SelectTile(tile);
            }
            else
            {
                if (AdjacentTiles(tile) == true)
                {
                    SwapTwoTiles(tile);
                    DeselectTile(oldSelectionTile);
                }
                else
                {
                    DeselectTile(oldSelectionTile);
                    SelectTile(tile);
                }
            }
        }
    }
}
