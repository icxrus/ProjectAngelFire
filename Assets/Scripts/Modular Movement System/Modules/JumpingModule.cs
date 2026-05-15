using UnityEngine;

[RequireComponent(typeof(BasicMovementModule))]
public class JumpingModule : MonoBehaviour
{
    private ApplyMovement _applyMovement;

    private float jumpHeight = 1f;
    private float gravityValue = -9.81f;

    [SerializeField]
    private bool isGrounded;

    [SerializeField]
    private Vector3 playerVelocity;

    private void OnEnable()
    {
        _applyMovement = GetComponent<ApplyMovement>();

        GetComponent<InputHandler>().JumpTriggeredCallback += Jumping;
        Debug.Log("Subscribed jump function to Jump Triggered");
    }

    private void OnDisable()
    {
        GetComponent<InputHandler>().JumpTriggeredCallback -= Jumping;
        Debug.Log("Unsubscribed jump function to Jump Triggered");
    }

    private float _pendingJumpVelocity = 0;

    private void Jumping()
    {
        if (_applyMovement.GroundCheck())
        {
            _pendingJumpVelocity = Mathf.Sqrt(jumpHeight * -2.0f * gravityValue);
            
        }
    }

    public float GetAndResetJumpVelocity()
    {
        float v = _pendingJumpVelocity;
        _pendingJumpVelocity = 0;
        return v;
    }

}
