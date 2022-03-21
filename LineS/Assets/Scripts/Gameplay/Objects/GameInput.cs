﻿using System;
using System.Collections.Generic;
using UnityEngine;

public class GameInput : Singleton<GameInput>
{
    public enum InputType { TouchDown, TouchUp, TouchMove, KeyDown, KeyUp, KeyHold }
    public enum KeyEvent
    {
        Undefined, LeftTrigger, RightTrigger, LeftStickX, LeftStickY, RightStickX, RightStickY,
        DpadUp, DpadDown, DpadLeft, DpadRight, LeftBumper, RightBumper, ButtonY, ButtonA, ButtonX, ButtonB,
        ButtonStart, ButtonSelect, ButtonBack, LeftStickButton, RightStickButton
    }

    protected Dictionary<InputType, List<Action<Vector3>>> mTouchListeners = new Dictionary<InputType, List<Action<Vector3>>>();
    protected Dictionary<InputType, List<Action<KeyEvent>>> mKeyListeners = new Dictionary<InputType, List<Action<KeyEvent>>>();
    protected Camera mMainCamera;

    void Start()
    {
        mMainCamera = Camera.main;
    }

    void Update()
    {
        HandleTouch();
        HandleKey();
    }

    protected virtual void HandleTouch()
    {
        Vector3 position;

        if (Input.GetMouseButton(0))
        {
            position = mMainCamera.ScreenToWorldPoint(Input.mousePosition);
            OnTouch(InputType.TouchMove, position);
        }

        if (Input.GetMouseButtonDown(0))
        {
            position = mMainCamera.ScreenToWorldPoint(Input.mousePosition);
            OnTouch(InputType.TouchDown, position);
        }

        if (Input.GetMouseButtonUp(0))
        {
            position = mMainCamera.ScreenToWorldPoint(Input.mousePosition);
            OnTouch(InputType.TouchUp, position);
        }
    }

    protected void HandleKey()
    {
        //if (Input.GetKeyDown(KeyCode.UpArrow) || Input.GetKeyDown(KeyCode.W)) OnKey(InputType.KeyDown, KeyEvent.DpadUp);
        //if (Input.GetKeyUp(KeyCode.UpArrow) || Input.GetKeyUp(KeyCode.W)) OnKey(InputType.KeyUp, KeyEvent.DpadUp);
        if (Input.GetKey(KeyCode.UpArrow) || Input.GetKey(KeyCode.W)) OnKey(InputType.KeyHold, KeyEvent.DpadUp);

        //if (Input.GetKeyDown(KeyCode.DownArrow) || Input.GetKeyDown(KeyCode.S)) OnKey(InputType.KeyDown, KeyEvent.DpadDown);
        //if (Input.GetKeyUp(KeyCode.DownArrow) || Input.GetKeyUp(KeyCode.S)) OnKey(InputType.KeyUp, KeyEvent.DpadDown);
        if (Input.GetKey(KeyCode.DownArrow) || Input.GetKey(KeyCode.S)) OnKey(InputType.KeyHold, KeyEvent.DpadDown);

        //if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.A)) OnKey(InputType.KeyDown, KeyEvent.DpadLeft);
        //if (Input.GetKeyUp(KeyCode.LeftArrow) || Input.GetKeyUp(KeyCode.A)) OnKey(InputType.KeyUp, KeyEvent.DpadLeft);
        if (Input.GetKey(KeyCode.LeftArrow) || Input.GetKey(KeyCode.A)) OnKey(InputType.KeyHold, KeyEvent.DpadLeft);

        //if (Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.D)) OnKey(InputType.KeyDown, KeyEvent.DpadRight);
        //if (Input.GetKeyUp(KeyCode.RightArrow) || Input.GetKeyUp(KeyCode.D)) OnKey(InputType.KeyUp, KeyEvent.DpadRight);
        if (Input.GetKey(KeyCode.RightArrow) || Input.GetKey(KeyCode.D)) OnKey(InputType.KeyHold, KeyEvent.DpadRight);

        if (Input.GetKeyDown(KeyCode.Return)) OnKey(InputType.KeyDown, KeyEvent.ButtonA);
        if (Input.GetKeyUp(KeyCode.Return)) OnKey(InputType.KeyUp, KeyEvent.ButtonA);
    }

    protected void OnTouch(InputType type, Vector3 position)
    {
        if (!mTouchListeners.ContainsKey(type)) return;

        foreach (var action in mTouchListeners[type])
        {
            action(new Vector3(position.x, position.y, 0.0f));
        }
    }

    protected void OnKey(InputType type, KeyEvent keyEvent)
    {
        if (!mKeyListeners.ContainsKey(type)) return;

        foreach (var action in mKeyListeners[type])
        {
            action(keyEvent);
        }
    }

    protected void AddListener<T>(Dictionary<InputType, List<Action<T>>> listener, InputType type, Action<T> action)
    {
        if(!listener.ContainsKey(type))
            listener[type] = new List<Action<T>>();

        for (int i = 0; i < listener[type].Count; i++)
        {
            if (listener[type][i] == action)
            {
                return;
            }
        }

        listener[type].Add(action);
    }

    protected void RemoveListener<T>(Dictionary<InputType, List<Action<T>>> listener, InputType type, Action<T> action)
    {
        if (!listener.ContainsKey(type))
        {
            return;
        }

        for (int i = 0; i < listener[type].Count; i++)
        {
            listener[type].Remove(action);
        }
    }

    public static void RegisterTouchEvent(InputType type, Action<Vector3> action)
    {
        if (type > InputType.TouchMove) return;

        Instance.AddListener<Vector3>(Instance.mTouchListeners, type, action);
    }

    public static void UnRegisterTouchEvent(InputType type, Action<Vector3> action)
    {
        if (type > InputType.TouchMove) return;

        Instance.RemoveListener<Vector3>(Instance.mTouchListeners, type, action);
    }

    public static void RegisterKeyEvent(InputType type, Action<KeyEvent> action)
    {
        if (type < InputType.KeyDown) return;

        Instance.AddListener<KeyEvent>(Instance.mKeyListeners, type, action);
    }

    public static void UnRegisterKeyEvent(InputType type, Action<KeyEvent> action)
    {
        if (type < InputType.KeyDown) return;

        Instance.RemoveListener<KeyEvent>(Instance.mKeyListeners, type, action);
    }
}
