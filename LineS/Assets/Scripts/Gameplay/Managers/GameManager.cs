using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GamePlay
{
    public enum HardMode { Easy, Medium, Hard }
    public enum GameState { None, Initialize, Start, InProgress, Paused, End }
    public enum GameCommand { LoadBoard, SaveBoard, ResetBoard }
}

public class GameManager : Singleton<GameManager>, IEvent<GameCommandEvent>
{
    public GamePlay.GameState GameState = GamePlay.GameState.None;
    public GamePlay.HardMode HardMode = GamePlay.HardMode.Easy;

    void Start()
    {

    }

    void Update()
    {
        
    }

    protected virtual void OnEnable()
    {
        EventDispatcher.AddListener<GameCommandEvent>(this);
    }

    protected virtual void OnDisable()
    {
        EventDispatcher.RemoveListener<GameCommandEvent>(this);
    }

    public int NumOfKindBall
    {
        get 
        {
            if (HardMode > GamePlay.HardMode.Medium) return 7;
            if (HardMode > GamePlay.HardMode.Easy) return 6;

            return 3;
        }
    }

   

    public void OnEvent(GameCommandEvent eventType)
    {
        switch (eventType.Command)
        {
            
            case GamePlay.GameCommand.LoadBoard:
                break;
           
            default:
                break;
        }
    }
}

public struct GameCommandEvent
{
    public GamePlay.GameCommand Command;
    public object Object;

    public GameCommandEvent(GamePlay.GameCommand command, object obj)
    {
        Command = command;
        Object = obj;
    }
}

