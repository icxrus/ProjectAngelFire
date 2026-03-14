using UnityEngine;

[RequireComponent(typeof(BasicMovementModule))]
public class RunningModule : MonoBehaviour
{
    private float runningSpeed = 6f;
    private bool isRunning;

    private void OnEnable()
    {
        GetComponent<InputHandler>().RunTriggeredCallback += PlayerRun;
        Debug.Log("Subscribed run function to Running Triggered");
    }

    private void OnDisable()
    {
        GetComponent<InputHandler>().RunTriggeredCallback -= PlayerRun;
        Debug.Log("Unsubscribed run function to Running Triggered");
    }

    private void PlayerRun()
    {
        isRunning = true;
    }

    public float GetRunSpeed()
    {
        if (isRunning)
        {
            try 
            {
                return runningSpeed;
            }
            finally
            {
                isRunning = false;
            }

        }
        else
            return 0f;
    }

}
