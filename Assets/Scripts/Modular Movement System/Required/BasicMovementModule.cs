using System.Collections;
using UnityEngine;
using UnityEngine.Windows;

[RequireComponent(typeof(InputHandler), typeof(ApplyMovement), typeof(Animator))]
public class BasicMovementModule : MonoBehaviour
{
    private Animator animator;
    private Transform cameraTransform;

    [SerializeField]
    private float activeMovementSpeed;
    private float basicMovementSpeed = 3f;
    private float rotationSpeed = 5f;
    private float animationSmoothTime = 0.15f;

    private Vector3 move;
    private Vector2 currentAnimationBlendVector;
    private Vector2 animationVelocity;
    private Vector2 _input;

    private int speedXAnimationParameterID;
    private int speedYAnimationParameterID;


    private Coroutine movementRoutine;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        speedXAnimationParameterID = Animator.StringToHash("MoveX");
        speedYAnimationParameterID = Animator.StringToHash("MoveZ");
        cameraTransform = Camera.main.transform;

        GetComponent<InputHandler>().MovementTriggeredCallBack += OnMoveInput;
        Debug.Log("Subscribed movement function to Movement Triggered");
    }

    private void OnDisable()
    {
        GetComponent<InputHandler>().MovementTriggeredCallBack -= OnMoveInput;
        Debug.Log("Unsubscribed movement function to Movement Triggered");
    }

    public void OnMoveInput()
    {
        _input = GetComponent<InputHandler>().ReturnInputValuesForMovement();

        if (_input != Vector2.zero && movementRoutine == null)
        {
            movementRoutine = StartCoroutine(MovementLoop());
        }
    }

    private IEnumerator MovementLoop()
    {
        // Run as long as we have input OR as long as we haven't finished smoothing to zero
        while (_input != Vector2.zero || currentAnimationBlendVector.sqrMagnitude > 0.001f)
        {
            MovePlayer();
            yield return null;
        }

        movementRoutine = null;
    }

    private void MovePlayer()
    {
        
        Vector2 input = GetComponent<InputHandler>().ReturnInputValuesForMovement();
        currentAnimationBlendVector = Vector2.SmoothDamp(currentAnimationBlendVector, input, ref animationVelocity, animationSmoothTime);
        Vector3 _lastInput = input;

        move = new(currentAnimationBlendVector.x, 0, currentAnimationBlendVector.y);
        move = move.x * cameraTransform.right.normalized + move.z * cameraTransform.forward.normalized;
        move.y = 0f;

        SmoothOutAnimationsInBlendTree();

        RotateMovementToCameraDirection(_lastInput);
    }

    private void RotateMovementToCameraDirection(Vector3 _lastInput)
    {
        // Rotate towards camera direction when moving
        if (_lastInput.sqrMagnitude == 0) return;
        float targetAngle = cameraTransform.eulerAngles.y;
        Quaternion targetRotation = Quaternion.Euler(0, targetAngle, 0);
        transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, rotationSpeed * Time.deltaTime);
    }

    private void SmoothOutAnimationsInBlendTree()
    {
        animator.SetFloat(speedXAnimationParameterID, currentAnimationBlendVector.x);
        animator.SetFloat(speedYAnimationParameterID, currentAnimationBlendVector.y);
    }

    public Vector3 ReturnMoveVector3Values()
    {
        try
        {
            DetermineMovementSpeed();
            return activeMovementSpeed * move;
        }
        finally //Reset movement
        {
            move = Vector3.zero;
        }
    }

    private void DetermineMovementSpeed()
    {
        if (GetComponent<RunningModule>() && GetComponent<InputHandler>().RunningActive())
        {
            float activeSpeed = GetComponent<RunningModule>().GetRunSpeed();
            if (activeSpeed != 0f)
            {
                activeMovementSpeed = activeSpeed;
                animator.SetBool("Run", true);
            }
            else
            {
                activeMovementSpeed = basicMovementSpeed;
                animator.SetBool("Run", false);
            }
        }
        else if (GetComponent<CrouchingModule>() && GetComponent<InputHandler>().CrouchingActive())
        {
            float activeSpeed = GetComponent<CrouchingModule>().GetCrouchSpeed();
            if (activeSpeed != 0f)
            {
                activeMovementSpeed = activeSpeed;
                animator.SetBool("Crouch", true);
            }
            else
            {
                activeMovementSpeed = basicMovementSpeed;
                animator.SetBool("Crouch", false);
            }
        }
        else
        {
            activeMovementSpeed = basicMovementSpeed;
            animator.SetBool("Run", false);
            animator.SetBool("Crouch", false);
        }
    }

}
