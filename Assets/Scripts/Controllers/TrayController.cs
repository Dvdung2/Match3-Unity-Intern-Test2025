using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using DG.Tweening;

public class TrayController : MonoBehaviour
{
    public int MaxCells = 5;
    private List<Item> trayItems = new List<Item>();

    public List<Item> GetItems()
    {
        return trayItems;
    }
    private Transform[] cellTransforms;
    private GameManager m_gameManager;
    private BoardController m_boardController;
    
    public void Setup(GameManager gameManager, BoardController boardController)
    {
        m_gameManager = gameManager;
        m_boardController = boardController;
        CreateTrayCells();
    }

    private void CreateTrayCells()
    {
        cellTransforms = new Transform[MaxCells];
        Vector3 origin = new Vector3(-MaxCells * 0.5f + 0.5f, -4.5f, 0f);
        GameObject prefabBG = Resources.Load<GameObject>(Constants.PREFAB_CELL_BACKGROUND);
        for (int i = 0; i < MaxCells; i++)
        {
            GameObject go = GameObject.Instantiate(prefabBG);
            go.transform.position = origin + new Vector3(i, 0, 0f);
            go.transform.SetParent(this.transform);
            cellTransforms[i] = go.transform;
            
            // For time attack returning to board
            var collider = go.AddComponent<BoxCollider2D>();
            collider.size = new Vector2(1, 1);
            var trayCell = go.AddComponent<TrayCell>();
            trayCell.Index = i;
            trayCell.Tray = this;
        }
    }

    public bool CanAddItem()
    {
        return trayItems.Count < MaxCells;
    }

    public void AddItem(Item item)
    {
        if (!CanAddItem()) return;
        
        trayItems.Add(item);
        UpdateTrayPositions();
        
        StartCoroutine(CheckMatchesCoroutine());
    }

    private void UpdateTrayPositions()
    {
        for (int i = 0; i < trayItems.Count; i++)
        {
            trayItems[i].View.SetParent(this.transform);
            trayItems[i].View.DOMove(cellTransforms[i].position, 0.2f);
            trayItems[i].SetSortingLayerHigher();
        }
    }

    private IEnumerator CheckMatchesCoroutine()
    {
        yield return new WaitForSeconds(0.25f);
        
        bool matched = false;
        var groups = trayItems.GroupBy(i => ((NormalItem)i).ItemType).ToList();
        
        foreach (var group in groups)
        {
            if (group.Count() >= 3)
            {
                matched = true;
                var itemsToRemove = group.Take(3).ToList();
                foreach (var item in itemsToRemove)
                {
                    trayItems.Remove(item);
                    if (item.View)
                    {
                        item.View.DOScale(0f, 0.2f).OnComplete(() => {
                            item.Clear();
                        });
                    }
                }
                break; // Only one match at a time
            }
        }

        if (matched)
        {
            yield return new WaitForSeconds(0.2f);
            UpdateTrayPositions();
            m_boardController.CheckWinCondition();
        }
        else
        {
            if (trayItems.Count >= MaxCells)
            {
                if (m_gameManager.State != GameManager.eStateGame.GAME_OVER) {
                    if (!m_boardController.IsTimeAttack) {
                        m_gameManager.GameOver(); // LOSE
                    }
                }
            }
        }
    }

    public void ReturnItemToBoard(int cellIndex)
    {
        if (!m_boardController.IsTimeAttack) return;
        if (cellIndex < 0 || cellIndex >= trayItems.Count) return;
        
        Item itemToReturn = trayItems[cellIndex];
        trayItems.RemoveAt(cellIndex);
        UpdateTrayPositions();
        
        m_boardController.ReturnItemToBoard(itemToReturn);
    }
}

public class TrayCell : MonoBehaviour
{
    public int Index;
    public TrayController Tray;
}
