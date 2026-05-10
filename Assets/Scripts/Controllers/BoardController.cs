using DG.Tweening;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BoardController : MonoBehaviour
{
    public event Action OnMoveEvent = delegate { };
    public bool IsBusy { get; private set; }
    private Board m_board;
    private GameManager m_gameManager;
    private Camera m_cam;
    private GameSettings m_gameSettings;
    private bool m_gameOver;

    private TrayController m_trayController;
    public bool IsTimeAttack { get; private set; }

    public void StartGame(GameManager gameManager, GameSettings gameSettings)
    {
        m_gameManager = gameManager;
        m_gameSettings = gameSettings;
        m_gameManager.StateChangedAction += OnGameStateChange;
        m_cam = Camera.main;

        // Check mode
        IsTimeAttack = m_gameManager.CurrentMode == GameManager.eLevelMode.TIME_ATTACK;

        m_board = new Board(this.transform, gameSettings);
        m_board.Fill();

        m_trayController = new GameObject("TrayController").AddComponent<TrayController>();
        m_trayController.Setup(m_gameManager, this);
    }

    private void OnGameStateChange(GameManager.eStateGame state)
    {
        switch (state)
        {
            case GameManager.eStateGame.GAME_STARTED:
                IsBusy = false;
                break;
            case GameManager.eStateGame.PAUSE:
                IsBusy = true;
                break;
            case GameManager.eStateGame.GAME_OVER:
                m_gameOver = true;
                break;
        }
    }

    public void Update()
    {
        if (m_gameOver || IsBusy) return;

        if (Input.GetMouseButtonDown(0))
        {
            var hit = Physics2D.Raycast(m_cam.ScreenToWorldPoint(Input.mousePosition), Vector2.zero);
            if (hit.collider != null)
            {
                Cell cell = hit.collider.GetComponent<Cell>();
                if (cell != null && !cell.IsEmpty && m_trayController.CanAddItem())
                {
                    Item item = cell.Item;
                    cell.Free();
                    m_trayController.AddItem(item);
                    OnMoveEvent();
                }
                else
                {
                    TrayCell trayCell = hit.collider.GetComponent<TrayCell>();
                    if (trayCell != null && IsTimeAttack)
                    {
                        m_trayController.ReturnItemToBoard(trayCell.Index);
                    }
                }
            }
        }
    }

    public void ReturnItemToBoard(Item item)
    {
        // Find empty cell on board
        Cell emptyCell = m_board.GetFirstEmptyCell();
        if (emptyCell != null)
        {
            emptyCell.Assign(item);
            item.View.SetParent(this.transform);
            item.AnimationMoveToPosition();
        }
    }

    public void CheckWinCondition()
    {
        if (m_board.IsBoardEmpty())
        {
            m_gameManager.GameOver(true); // Win
        }
    }

    internal void Clear()
    {
        m_board.Clear();
        if (m_trayController) Destroy(m_trayController.gameObject);
    }

    // AUTOPLAY LOGIC
    private Coroutine autoplayCoroutine;
    public void StartAutoplay(bool autoLose)
    {
        if (autoplayCoroutine != null) StopCoroutine(autoplayCoroutine);
        autoplayCoroutine = StartCoroutine(AutoplayRoutine(autoLose));
    }

    private IEnumerator AutoplayRoutine(bool autoLose)
    {
        while (!m_gameOver && !IsBusy)
        {
            yield return new WaitForSeconds(0.5f);
            if (!m_trayController.CanAddItem()) continue;

            Cell targetCell = null;
            if (autoLose)
            {
                // Find a random item to force loss
                targetCell = m_board.GetRandomOccupiedCell();
            }
            else
            {
                // Find a logical item to win
                targetCell = GetBestCellForAutowin();
            }

            if (targetCell != null)
            {
                Item item = targetCell.Item;
                targetCell.Free();
                m_trayController.AddItem(item);
                OnMoveEvent();
            }
        }
    }

    private Cell GetBestCellForAutowin()
    {
        List<Item> trayItems = m_trayController.GetItems();
        
        if (trayItems.Count > 0)
        {
            var groups = trayItems.GroupBy(i => ((NormalItem)i).ItemType).ToList();
            groups = groups.OrderByDescending(g => g.Count()).ToList();
            
            foreach (var group in groups)
            {
                NormalItem.eNormalType targetType = group.Key;
                Cell matchingCell = m_board.GetCellWithItemType(targetType);
                if (matchingCell != null)
                {
                    return matchingCell;
                }
            }
        }
        
        return m_board.GetRandomOccupiedCell();
    }
}
