using UnityEngine;

public class ApplyMovement : MonoBehaviour
{
    private CharacterController controller;
    private InputHandler _inputHandler;
    private BasicMovementModule _basicMovementModule;

    [Header("Movement State")]
    [SerializeField] private Vector3 playerVelocity;
    private float vertVelocity;
    private bool isGrounded = true;
    private Vector3 groundNormal = Vector3.up;
    private float currentSlopeAngle;

    [Header("Physics Settings")]
    [SerializeField] private float gravityValue = -9.81f;
    [SerializeField] private float stickinessForce = 5f;
    [SerializeField] private float slideSpeed = 10f;

    private float jumpGracePeriod;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
        _inputHandler = GetComponent<InputHandler>();
        _basicMovementModule = GetComponent<BasicMovementModule>();
    }

    void Update()
    {
        isGrounded = GroundCheck();
        playerVelocity = _basicMovementModule.ReturnMoveVector3Values();
        float horizontalSpeed = new Vector3(playerVelocity.x, 0, playerVelocity.z).magnitude;

        if (isGrounded)
        {
            currentSlopeAngle = Vector3.Angle(Vector3.up, groundNormal);
            bool isTooSteep = currentSlopeAngle > controller.slopeLimit;

            if (isTooSteep)
            {
                Vector3 slipDirection = new Vector3(groundNormal.x, 0, groundNormal.z).normalized;
                playerVelocity += slipDirection * slideSpeed;
            }

            HandleJumping(isTooSteep, horizontalSpeed);
        }
        else
        {
            vertVelocity += gravityValue * Time.deltaTime;
        }

        playerVelocity.y = vertVelocity;

        controller.Move(playerVelocity * Time.deltaTime);

        if (jumpGracePeriod > 0) jumpGracePeriod -= Time.deltaTime;
    }

    private void HandleJumping(bool isTooSteep, float horizontalSpeed)
    {
        if (TryGetComponent<JumpingModule>(out var jumpingModule))
        {
            float jumpImpulse = jumpingModule.GetAndResetJumpVelocity();

            if (jumpImpulse > 0)
            {
                float slopeMultiplier = 1f;
                float moveDotNormal = Vector3.Dot(playerVelocity.normalized, groundNormal);

                if (!isTooSteep && groundNormal.y < 0.95f && moveDotNormal < 0)
                {
                    slopeMultiplier = Mathf.Lerp(2.0f, 1.5f, groundNormal.y);
                }
                else if (isTooSteep)
                {
                    slopeMultiplier = 0.7f;
                }

                vertVelocity = jumpImpulse * slopeMultiplier;

                controller.Move(Vector3.up * 0.1f);
                jumpGracePeriod = 0.2f;
            }
            else if (vertVelocity <= 0 && jumpGracePeriod <= 0)
            {
                if (!isTooSteep)
                {
                    vertVelocity = -stickinessForce - (horizontalSpeed * 0.5f);
                }
                else
                {
                    vertVelocity = -1f;
                }
            }
        }
    }

    public bool GroundCheck()
    {
        float radius = controller.radius * 0.9f;
        float dist = (controller.height * 0.5f) - radius + 0.15f;
        Vector3 origin = transform.position;

        if (Physics.SphereCast(origin, radius, Vector3.down, out RaycastHit hit, dist))
        {
            groundNormal = hit.normal;
            return true;
        }

        groundNormal = Vector3.up;
        return false;
    }
}