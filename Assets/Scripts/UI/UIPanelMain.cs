using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UIPanelMain : MonoBehaviour, IMenu
{
    [SerializeField] private Button btnTimer;

    [SerializeField] private Button btnMoves;

    [SerializeField] private Button btnAutoplay;

    [SerializeField] private Button btnAutoLose;

    [SerializeField] private Button btnTimeAttack;

    private UIMainManager m_mngr;

    private void Awake()
    {
        if (btnMoves) btnMoves.onClick.AddListener(OnClickMoves);
        if (btnTimer) btnTimer.onClick.AddListener(OnClickTimer);
        
        if (btnAutoplay) btnAutoplay.onClick.AddListener(() => StartGameWithAutoplay(false));
        if (btnAutoLose) btnAutoLose.onClick.AddListener(() => StartGameWithAutoplay(true));
        if (btnTimeAttack) btnTimeAttack.onClick.AddListener(() => m_mngr.LoadLevel(GameManager.eLevelMode.TIME_ATTACK));
    }

    private void StartGameWithAutoplay(bool autoLose)
    {
        m_mngr.LoadLevelMoves();
        BoardController bc = FindObjectOfType<BoardController>();
        if (bc != null)
        {
            bc.StartAutoplay(autoLose);
        }
    }

    private void OnDestroy()
    {
        if (btnMoves) btnMoves.onClick.RemoveAllListeners();
        if (btnTimer) btnTimer.onClick.RemoveAllListeners();
    }

    public void Setup(UIMainManager mngr)
    {
        m_mngr = mngr;
    }

    private void OnClickTimer()
    {
        m_mngr.LoadLevel(GameManager.eLevelMode.TIMER);
    }

    private void OnClickMoves()
    {
        m_mngr.LoadLevel(GameManager.eLevelMode.MOVES);
    }

    public void Show()
    {
        this.gameObject.SetActive(true);
    }

    public void Hide()
    {
        this.gameObject.SetActive(false);
    }
}
