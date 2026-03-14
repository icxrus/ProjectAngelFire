using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput), typeof(CharacterController))]
public class InputHandler : MonoBehaviour
{
    private PlayerInput playerInput;

    //Input Actions - Add all new actions here following the format (action must be present in Player Inputs).
    private InputAction movementAction, jumpAction, runAction, crouchAction;
    private bool movementHoldDown, runHoldDown, crouchHoldDown;

    private void Awake()
    {
        playerInput = GetComponent<PlayerInput>();

        //Assign correct key to Actions - Name must be identical to Player Inputs name.
        movementAction = playerInput.actions["Movement"];
        jumpAction = playerInput.actions["Jump"];
        runAction = playerInput.actions["Run"];
        crouchAction = playerInput.actions["Crouch"];
    }

    #region Delegates
    //Create a subscribable function for each action - Subscribe to function from seperate modules
    public delegate void MovementTriggered();
    public event MovementTriggered MovementTriggeredCallBack;

    public delegate void JumpTriggered();
    public event JumpTriggered JumpTriggeredCallback;

    public delegate void RunTriggered();
    public event RunTriggered RunTriggeredCallback;

    public delegate void CrouchTriggered();
    public event CrouchTriggered CrouchTriggeredCallback;
    #endregion

    private void Update()
    {
        movementAction.performed += _ => movementHoldDown = true;
        movementAction.canceled += _ => movementHoldDown = false;

        runAction.performed += _ => runHoldDown = true;
        runAction.canceled += _ => runHoldDown = false;

        crouchAction.performed += _ => crouchHoldDown = true;
        crouchAction.canceled += _ => crouchHoldDown = false;

        //Invoke each delegate if the corresponding button is triggered
        if (movementHoldDown)
            MovementTriggeredCallBack?.Invoke();

        if (jumpAction.triggered)
            JumpTriggeredCallback?.Invoke();

        if (runHoldDown)
            RunTriggeredCallback?.Invoke();

        if (crouchHoldDown)
            CrouchTriggeredCallback?.Invoke();
    }

    #region Helper Functions
    public bool JumpTriggeredCheckForDownForce()
    {
        if (jumpAction.triggered)
        {
            return true;
        }
        else
            return false;
    }

    public bool RunningActive()
    {
        if (runHoldDown)
        {
            return true;
        }
        else
            return false;
    }
    public bool CrouchingActive()
    {
        if (crouchHoldDown)
        {
            return true;
        }
        else
            return false;
    }

    public Vector2 ReturnInputValuesForMovement()
    {
        Vector2 input = movementAction.ReadValue<Vector2>();
        return input;
    }
    #endregion
}
