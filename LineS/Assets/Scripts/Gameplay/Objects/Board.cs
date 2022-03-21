﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Board : MonoBehaviour, IEvent<GameCommandEvent>
{
    public const int BOARD_SIZE = 9;
    public const int POINT_TO_SCORE = 3;

    protected Cell[,] mCells = new Cell[BOARD_SIZE, BOARD_SIZE];

    public Cell CellTemplate;
    public Ball BallTemplate;

    protected Cell mSelectedCell = null;
    protected Ball mSelectedBall = null;

    protected List<Vector2Int> mPointsToCheck;
    protected List<Vector2Int> mMovePath;

    protected bool mIsMoving;
    protected bool mIsTouchable = true;
    protected float mDeltaPressTime = 0;

    protected Vector2Int mPointerIndex = new Vector2Int(0, 0);

    void Awake() => Initialize();

    protected virtual void Initialize()
    {
        mPointsToCheck = new List<Vector2Int>();
    }

    void Start()
    {
        CreateBoard();
        AddBalls(10, GameManager.Instance.NumOfKindBall, Ball.Size.Ball);
        AddBalls(5, GameManager.Instance.NumOfKindBall, Ball.Size.Dot);
    }

    void Update()
    {

    }

    protected virtual void OnEnable()
    {
        GameInput.RegisterTouchEvent(GameInput.InputType.TouchDown, OnTouchDown);
        GameInput.RegisterTouchEvent(GameInput.InputType.TouchMove, OnTouchMove);
        GameInput.RegisterTouchEvent(GameInput.InputType.TouchUp, OnTouchUp);

        GameInput.RegisterKeyEvent(GameInput.InputType.KeyDown, OnKeyDown);
        GameInput.RegisterKeyEvent(GameInput.InputType.KeyUp, OnKeyUp);
        GameInput.RegisterKeyEvent(GameInput.InputType.KeyHold, OnKeyHold);

        EventDispatcher.AddListener<GameCommandEvent>(this);
    }

    protected virtual void OnDisable()
    {
        GameInput.UnRegisterTouchEvent(GameInput.InputType.TouchDown, OnTouchDown);
        GameInput.UnRegisterTouchEvent(GameInput.InputType.TouchMove, OnTouchMove);
        GameInput.UnRegisterTouchEvent(GameInput.InputType.TouchUp, OnTouchUp);

        GameInput.UnRegisterKeyEvent(GameInput.InputType.KeyDown, OnKeyDown);
        GameInput.UnRegisterKeyEvent(GameInput.InputType.KeyUp, OnKeyUp);
        GameInput.UnRegisterKeyEvent(GameInput.InputType.KeyHold, OnKeyHold);

        EventDispatcher.RemoveListener<GameCommandEvent>(this);
    }

    private void OnKeyDown(GameInput.KeyEvent keyEvent)
    {
        switch (keyEvent)
        {
            
            case GameInput.KeyEvent.ButtonA:
                SelectCell(mPointerIndex);
                break;
        }

    }

    private void OnKeyUp(GameInput.KeyEvent keyEvent)
    {

    }

    private void OnKeyHold(GameInput.KeyEvent keyEvent)
    {
        mDeltaPressTime += Time.deltaTime;

        if (mDeltaPressTime < 0.1f) return;
        mDeltaPressTime = 0f;

        switch (keyEvent)
        {
            case GameInput.KeyEvent.DpadUp:
                MovePointer(-1, 0);
                break;
            case GameInput.KeyEvent.DpadDown:
                MovePointer(1, 0);
                break;
            case GameInput.KeyEvent.DpadLeft:
                MovePointer(0, -1);
                break;
            case GameInput.KeyEvent.DpadRight:
                MovePointer(0, 1);
                break;
        }

        mCells[mPointerIndex.x, mPointerIndex.y].OnHover(true);
    }

    protected void MovePointer(int offsetx, int offsety)
    {
        Vector2Int nextIndex = mPointerIndex + new Vector2Int(offsetx, offsety);
        if (IsInside(nextIndex))
        {
            mCells[mPointerIndex.x, mPointerIndex.y].OnHover(false);
            mPointerIndex = nextIndex;
            mCells[mPointerIndex.x, mPointerIndex.y].OnHover(true);
        }
    }

    public void SelectCell(Vector2Int pointerIndex)
    { 
        if(mSelectedCell)
        {
            if(mSelectedCell.Index == pointerIndex)
            {
                mSelectedCell.ChangeBallState(Ball.State.Idle);
                mSelectedCell = null;

                return;
            }

            int value = PointToCell(pointerIndex);
            if(value == 0)
            {
                mCells[pointerIndex.x, pointerIndex.y].AttachBall(mSelectedCell.DettachBall());
                mCells[pointerIndex.x, pointerIndex.y].ChangeBallState(Ball.State.Idle);
                mPointsToCheck.Add(pointerIndex);
                MoveBallOnPath(mCells[pointerIndex.x, pointerIndex.y].Ball, mMovePath);
                mSelectedCell = null;
            }
            else if(value == 1)
            {
                if (mCells[pointerIndex.x, pointerIndex.y].IsSelectable)
                {
                    if (mSelectedCell.Ball) mSelectedCell.ChangeBallState(Ball.State.Idle);
                    mSelectedCell = mCells[pointerIndex.x, pointerIndex.y];
                    if (mSelectedCell) mSelectedCell.ChangeBallState(Ball.State.Selected);
                }
            }
        }
        else
        { 
            if(mCells[pointerIndex.x, pointerIndex.y].IsSelectable)
            {
                mSelectedCell = mCells[pointerIndex.x, pointerIndex.y];
                if (mSelectedCell) mSelectedCell.ChangeBallState(Ball.State.Selected);
            }
        }
    }

    public int PointToCell(Vector2Int index)
    {
        mMovePath = null;
        if (!mCells[index.x, index.y].IsAvailable)
            return 1;

        if (mMovePath.IsNullOrEmpty())
            return 2;
        return 0;
    }

    public void OnTouchDown(Vector3 postion)
    {
        if (!mIsTouchable) return;

        var seletedIndex = GetIndex(postion);
        if (seletedIndex.x > -1 && seletedIndex.y > -1)
            SelectCell(seletedIndex);
    }

    protected Vector2Int GetIndex(Vector3 position)
    {
        foreach (Cell cell in mCells)
        {
            if (cell.Bounds.Contains(position))
                return cell.Index;
        }

        return new Vector2Int(-1, -1);
    }


    public void CheckBoard()
    {
        foreach (Cell cell in mCells)
        {
            Ball ball = cell.Ball;
            if (ball && ball.BallSize == Ball.Size.Dot)
            {
                mPointsToCheck.Add(new Vector2Int(cell.Index.x, cell.Index.y));
                ball.SetSize(Ball.Size.Ball);
            }
        }
    }

    public void OnTouchMove(Vector3 postion)
    {

    }

    public void OnTouchUp(Vector3 postion)
    {

    }

    public void CreateBoard()
    {
        if (CellTemplate == null) return;

        float cellSize = 1f;
        float offset = (BOARD_SIZE - 1) * cellSize / 2;

        float offsetX = -offset, offsetY = offset;

        for (int i = 0; i < BOARD_SIZE; i++)
        {
            for (int j = 0; j < BOARD_SIZE; j++)
            {
                mCells[i, j] = Instantiate(CellTemplate, transform);
                mCells[i, j].transform.position = new Vector3(offsetX, offsetY);
                mCells[i, j].Index = new Vector2Int(i, j);
                if ((i * BOARD_SIZE + j) % 2 != 0)
                    mCells[i, j].SetColorLight();
                else
                    mCells[i, j].SetColorDark();
                offsetX += cellSize;
            }

            offsetY -= cellSize;
            offsetX = -offset;
        }
    }


    public void MoveBallOnPath(Ball ball, List<Vector2Int> cellsIndex)
    {
        if (ball == null || cellsIndex.IsNullOrEmpty()) return;

        List<Vector3> pos = cellsIndex.Select(p => mCells[p.x, p.y].transform.position).ToList<Vector3>();
        ball.transform.position = pos[0];
    }

    protected bool IsInside(Vector2Int pos)
    {
        return 0 <= pos.x && BOARD_SIZE > pos.x && 0 <= pos.y && BOARD_SIZE > pos.y;
    }

    public List<Cell> AddBalls(int numOfBall, int numOfType, Ball.Size size)
    {
        List<Cell> emptyCells = new List<Cell>();
        foreach (Cell cell in mCells)
        {
            if (cell.IsEmpty) emptyCells.Add(cell);
        }

        System.Random rnd = new System.Random();
        List<Cell> randomCells = emptyCells.OrderBy(x => rnd.Next()).Take(numOfBall).ToList<Cell>();
        List<Ball.Color> colors = new List<Ball.Color>();

        for(int i = 0; i < numOfBall; i++)
        {
            Cell cell = randomCells[i];
            if (cell)
            {
                cell.AttachBall(DataManager.Instance.TakeRandomBall(numOfType));
                cell.SetBallSize(size);

                if(cell.Ball && cell.Ball.BallSize == Ball.Size.Dot)
                {
                    colors.Add(cell.Ball.BallColor);
                }
            }
        }

        if (colors.Count > 0) EventDispatcher.TriggerEvent<BallChangeEvent>(new BallChangeEvent(BallChangeEnum.Change, colors));

        return randomCells;
    }

    public void OnEvent(GameCommandEvent eventType)
    {
        switch(eventType.Command)
        {
            
        }
    }
}

public enum BallChangeEnum { Add, Change }
public struct BallChangeEvent
{
    public BallChangeEnum ChangeType;
    public List<Ball.Color> Colors;

    public BallChangeEvent(BallChangeEnum type, List<Ball.Color> colors)
    {
        ChangeType = type;
        Colors = colors;
    }
}
