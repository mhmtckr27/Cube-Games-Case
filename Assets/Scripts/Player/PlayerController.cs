using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] public float runSpeed = 3;
    [SerializeField] public ParticleSystem plusOneVFX;
    [SerializeField] private GameObject playerMesh;

    private Rigidbody rb;
    public Animator animator;

    private static PlayerController instance;
    public static PlayerController Instance { get => instance; private set => instance = value; }

    private Collider platformCollider;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else if (Instance != this)
        {
            Destroy(gameObject);
        }

        rb = GetComponentInChildren<Rigidbody>();
        animator = GetComponentInChildren<Animator>();
    }

    public void StartRunning(bool start, bool gameOver)
    {
        InputManager.Instance.EnableInput(start);
        SetMoveSpeed(start ? runSpeed : 0);
        if (!start && !gameOver)
        {
            animator.SetTrigger("OnVictory");
        }
        else if (start)
        {
            playerMesh.transform.rotation = Quaternion.identity;
            playerMesh.transform.position = Vector3.zero;
        }
    }

    private void OnEnable()
    {
        InputManager.Instance.OnMovementInput += OnMovementInput;
        TapToPlayScreen.OnTapToPlay += TapToPlayScreen_OnTapToPlay;
    }

    private void TapToPlayScreen_OnTapToPlay()
    {
        platformCollider = GameObject.FindWithTag("Ground")?.GetComponentInChildren<BoxCollider>();
    }

    private void OnDisable()
    {
        InputManager.Instance.OnMovementInput -= OnMovementInput;
        TapToPlayScreen.OnTapToPlay -= TapToPlayScreen_OnTapToPlay;
    }

    private void SetMoveSpeed(float speed)
    {
        rb.velocity = new Vector3(0, 0, speed);
        animator.SetFloat("Velocity", speed);
    }

    private void OnMovementInput(float moveAmount)
    {
        Vector3 newPos = ValidateMovementInput(moveAmount);
        transform.position = newPos;        
    }

    private Vector3 ValidateMovementInput(float moveAmount)
    {
        Vector3 newPos = transform.position;
        
        newPos.x += Mathf.LerpUnclamped(0, platformCollider.bounds.size.x, moveAmount / (Screen.width / 2));

        Vector3 finalPos = platformCollider.ClosestPoint(newPos);
        finalPos.z = newPos.z;
        return finalPos;
    }
}
