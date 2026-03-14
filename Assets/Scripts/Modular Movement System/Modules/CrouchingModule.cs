using UnityEngine;

[RequireComponent(typeof(BasicMovementModule))]
public class CrouchingModule : MonoBehaviour
{
    private float crouchingSpeed = 1.5f;
    private bool isCrouching;

    private void OnEnable()
    {
        GetComponent<InputHandler>().CrouchTriggeredCallback += CrouchMovement;
        Debug.Log("Subscribed crouch function to Crouch Triggered");
    }

    private void OnDisable()
    {
        GetComponent<InputHandler>().CrouchTriggeredCallback -= CrouchMovement;
        Debug.Log("Unsubscribed crouch function to Crouch Triggered");
    }

    private void CrouchMovement()
    {
        isCrouching = true;
    }

    public float GetCrouchSpeed()
    {
        if (isCrouching)
        {
            try
            {
                return crouchingSpeed;
            }
            finally
            {
                isCrouching = false;
            }

        }
        else
            return 0f;
    }
}
