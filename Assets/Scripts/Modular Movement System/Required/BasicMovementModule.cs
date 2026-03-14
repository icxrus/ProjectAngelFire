using UnityEngine;

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

    private int speedXAnimationParameterID;
    private int speedYAnimationParameterID;

    private void OnEnable()
    {
        animator = GetComponent<Animator>();
        speedXAnimationParameterID = Animator.StringToHash("MoveX");
        speedYAnimationParameterID = Animator.StringToHash("MoveZ");
        cameraTransform = Camera.main.transform;

        GetComponent<InputHandler>().MovementTriggeredCallBack += MovePlayer;
        Debug.Log("Subscribed movement function to Movement Triggered");
    }

    private void OnDisable()
    {
        GetComponent<InputHandler>().MovementTriggeredCallBack -= MovePlayer;
        Debug.Log("Unsubscribed movement function to Movement Triggered");
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
            }
            else
            {
                activeMovementSpeed = basicMovementSpeed;
            }
        }
        else if (GetComponent<CrouchingModule>() && GetComponent<InputHandler>().CrouchingActive())
        {
            float activeSpeed = GetComponent<CrouchingModule>().GetCrouchSpeed();
            if (activeSpeed != 0f)
            {
                activeMovementSpeed = activeSpeed;
            }
            else
            {
                activeMovementSpeed = basicMovementSpeed;
            }
        }
        else
        {
            activeMovementSpeed = basicMovementSpeed;
        }
    }

}
