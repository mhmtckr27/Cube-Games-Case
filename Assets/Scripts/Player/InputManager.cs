using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InputManager : MonoBehaviour
{
    public event Action<float> OnMovementInput;

    public bool isInputEnabled = true;
    Vector2 touchPosition;

    private Vector2 fingerDown;
    private Vector2 fingerUp;
    public bool detectSwipeOnlyAfterRelease = false;

    public float swipeThreshold = 20f;
    private Vector2 initialPos;

    public bool isControlsReversed; //TODO: may add extra feature

    #region Singleton

    private static InputManager instance;
    public static InputManager Instance
    {
        get => instance;
        private set
        {
            instance = value;
        }
    }

    #endregion

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else if(Instance != this)
        {
            Destroy(gameObject);
        }
    }

    void Update()
    {
        if (!isInputEnabled)
        {
            return;
        }
       
        if (IsInputReceived())
        {
            ProcessInput();
        }
    }

    private bool IsInputReceived()
    {
        bool isInputReceived = false;
#if UNITY_EDITOR
        isInputReceived = Input.GetMouseButton(0);
        touchPosition = Input.mousePosition;
#else
        isInputReceived = Input.touchCount > 0;
        if (isInputReceived)
        {
            touchPosition = Input.GetTouch(0).position;
        }
#endif
        return isInputReceived;
    }

    void ProcessInput()
    {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0))
        {
            initialPos = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            MouseSwipe(Input.mousePosition);
            initialPos = Input.mousePosition;
        }

#else
        Touch touch = Input.GetTouch(0);
        if (touch.phase == TouchPhase.Began)
        {
            fingerDown = touch.position;
            fingerUp = touch.position;
        }
        else if (touch.phase == TouchPhase.Moved)
        {
            if (!detectSwipeOnlyAfterRelease)
            {
                fingerUp = touch.position;
                CheckSwipe();
            }
        }
        else if (touch.phase == TouchPhase.Ended)
        {
            fingerUp = touch.position;
            CheckSwipe();
        }
#endif
    }


    void CheckSwipe()
    {
        if (HorizontalMove() > swipeThreshold)
        {
            OnSwipe(fingerUp.x - fingerDown.x);
            fingerDown = fingerUp;
        }
    }

    void MouseSwipe(Vector3 finalPos)
    {
        float disX = Mathf.Abs(initialPos.x - finalPos.x);
        if (disX > 0)
        {
            OnSwipe(finalPos.x - initialPos.x);
        }
    }

    float HorizontalMove()
    {
        return Mathf.Abs(fingerDown.x - fingerUp.x);
    }

    void OnSwipe(float moveAmount)
    {
        OnMovementInput?.Invoke(moveAmount);
    }
    public void EnableInput(bool enable)
    {
        isInputEnabled = enable;
    }
}