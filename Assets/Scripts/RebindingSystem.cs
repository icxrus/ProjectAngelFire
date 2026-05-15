using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class RebindingSystem : MonoBehaviour
{
    [Header("UI Elements")]
    [Tooltip("Should contain as children: TMP text object for action name, Input field for Binding and a reset button.")]
    public GameObject rebindUIPrefab;
    [Tooltip("Recommended: contains a scroll rect component.")]
    public Transform rebindUIParent;
    public GameObject waitingForInputPanel;
    [Tooltip("Should have TMP text as child object.")]
    public GameObject actionMapPanelWText;

    public InputActionAsset inputActions;
    private Dictionary<string, InputBinding> originalBindings;
    private const string BINDINGS_SAVE_KEY = "bindings";

    private InputActionRebindingExtensions.RebindingOperation rebindOperation;
    private bool isRebinding = false;

    [Tooltip("Generates a Header style object in the UI.")]
    public bool generateActionMapNameUI = false;

    public static event Action OnBindingsChanged;
    public static event Action<InputAction, int> OnRebindStarted;
    public static event Action<InputAction, int> OnRebindFinished;

    private void Start()
    {
        Debug.Log("RebindingSystem Start() called.");
        originalBindings = new Dictionary<string, InputBinding>();

        //Store the original bindings when the game starts
        StoreOriginalBindings();

        //Load previously saved bindings (if any)
        LoadBindings();

        //Generate UI for each binding
        //GenerateRebindingUI();
    }

    private void Update()
    {
        //Check for ESC when rebinding
        if (isRebinding && Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            rebindOperation.Cancel();
            isRebinding = false;
        }
    }

    //Store the original bindings for reset
    private void StoreOriginalBindings()
    {
        foreach (InputAction action in inputActions)
        {
            foreach (var binding in action.bindings)
            {
                if (!originalBindings.ContainsKey(binding.id.ToString()))
                {
                    originalBindings[binding.id.ToString()] = binding;
                }
            }
        }
    }

    private void SaveBindings()
    {
        //Save only the overrides (bindings that have been modified) to PlayerPrefs
        string bindingsToSave = inputActions.SaveBindingOverridesAsJson();
        PlayerPrefs.SetString(BINDINGS_SAVE_KEY, bindingsToSave);
        Debug.Log("Bindings saved: " + bindingsToSave);
    }

    //Load saved bindings
    private void LoadBindings()
    {
        if (PlayerPrefs.HasKey(BINDINGS_SAVE_KEY))
        {
            string savedBindings = PlayerPrefs.GetString(BINDINGS_SAVE_KEY);
            Debug.Log("Loaded bindings: " + savedBindings);

            try
            {
                inputActions.LoadBindingOverridesFromJson(savedBindings);
            }
            catch (Exception e)
            {
                Debug.LogError("Failed to load bindings: " + e.Message);
            }
        }
    }

    //Generates UI for each binding
    private void GenerateRebindingUI()
    {
        // Check if inputActions contains any action maps
        if (inputActions == null || inputActions.actionMaps.Count == 0)
        {
            Debug.LogError("No action maps found in inputActions.");
            return;
        }

        // Iterate through each action map
        foreach (var actionMap in inputActions.actionMaps)
        {
            if (generateActionMapNameUI) { 
                GameObject rebindUIActionMapPanel = Instantiate(actionMapPanelWText, rebindUIParent);
                actionMapPanelWText.GetComponentInChildren<TMP_Text>().text = actionMap.name.ToUpper();
            }
            // Now loop through each action in this action map
            foreach (var action in actionMap.actions)
            {
                for (int i = 0; i < action.bindings.Count; i++)
                {
                    var binding = action.bindings[i];

                    // Skip composite bindings or non-keyboard/mouse bindings
                    if (binding.isComposite || binding.isPartOfComposite || !IsKeyboardOrMouseBinding(binding))
                        continue;

                    // Create a new UI item for the keybinding
                    GameObject rebindUIObject = Instantiate(rebindUIPrefab, rebindUIParent);

                    // Find relevant components in the prefab structure
                    TMP_Text rebindText = rebindUIObject.GetComponentInChildren<TMP_Text>();
                    TMP_InputField keybindInputField = rebindUIObject.GetComponentInChildren<TMP_InputField>();
                    Button resetButton = rebindUIObject.GetComponentInChildren<Button>();
                    TMP_Text resetButtonText = resetButton.GetComponentInChildren<TMP_Text>();

                    // Set the action name in the RebindText field
                    rebindText.text = action.name.ToUpper();

                    // Set the current keybinding in the InputField
                    string bindingDisplayName = InputControlPath.ToHumanReadableString(binding.effectivePath, InputControlPath.HumanReadableStringOptions.OmitDevice);
                    keybindInputField.text = bindingDisplayName.ToUpper();

                    // Set the Reset button's text (e.g., "Reset")
                    resetButtonText.text = "Reset".ToUpper();

                    // Store original binding (can use the binding ID to reference later)
                    originalBindings[binding.id.ToString()] = binding;

                    // Set input field callback for rebinding
                    int bindingIndex = i;  // Capture index in local scope for delegate
                    keybindInputField.onSelect.AddListener(delegate { StartRebinding(action, bindingIndex); });

                    // Set button callback for resetting the binding
                    resetButton.onClick.AddListener(() => ResetSingleBinding(action, bindingIndex));
                }
            }
        }
    }

    private bool IsKeyboardOrMouseBinding(InputBinding binding)
    {
        // Skip bindings with Vector2 type (e.g., joystick, mouse movement)
        if (binding.effectivePath.Contains("<Gamepad>/leftStick") ||
            binding.effectivePath.Contains("<Gamepad>/rightStick") ||
            binding.effectivePath.Contains("<Mouse>/position") ||
            binding.effectivePath.Contains("<Mouse>/delta"))
        {
            return false;
        }

        // Ensure it's specifically a Keyboard or Mouse binding
        return binding.groups.Contains("Keyboard") || binding.groups.Contains("Mouse");
    }

    // Starts the rebinding process for the given action and binding index
    private void StartRebinding(InputAction action, int bindingIndex)
    {
        OnRebindStarted?.Invoke(action, bindingIndex);
        DisableInputs();

        // Show the UI for waiting for input
        waitingForInputPanel.SetActive(true);
        isRebinding = true;

        // Perform rebinding with a mask to target the specific binding index
        rebindOperation = action.PerformInteractiveRebinding(bindingIndex)
            .WithControlsExcluding("<Mouse>/position") //Exclude bindings that aren't usually used
            .WithControlsExcluding("<Gamepad>/leftStick") 
            .WithControlsExcluding("<Gamepad>/rightStick")
            .WithControlsExcluding("<Keyboard>/escape")
            .WithControlsExcluding("<keyboard>/anyKey") //Fixes issue with rebinding cancellation
            .OnMatchWaitForAnother(0.1f)  // To wait for accidental double input
            .OnComplete(operation => RebindingComplete(action, bindingIndex, operation))
            .OnCancel((operation) =>
            {
                Debug.Log("Cancelling binding.");
                ResetSingleBinding(action, bindingIndex);
                CancelRebinding();
            })
            .Start();
    }

    private void CancelRebinding()
    {
        waitingForInputPanel.SetActive(false);
        isRebinding = false;

        // If the rebind operation exists, delete it
        if (rebindOperation != null)
        {
            rebindOperation.Dispose();
            rebindOperation = null;
        }

        EnableInputs();
        
    }

    // Callback for when rebinding is complete
    private void RebindingComplete(InputAction action, int bindingIndex, InputActionRebindingExtensions.RebindingOperation rebindOperation)
    {
        waitingForInputPanel.SetActive(false);
        isRebinding = false;

        // Ensure no duplicate bindings by removing old ones
        string newPath = rebindOperation.selectedControl.path;

        // Remove duplicates across all actions
        RemoveDuplicateBindings(action, newPath, bindingIndex);

        // Apply the new binding only if it's unique across all actions
        action.ApplyBindingOverride(bindingIndex, newPath);

        rebindOperation.Dispose();
        SaveBindings();
        EnableInputs();
        OnRebindFinished?.Invoke(action, bindingIndex);
        OnBindingsChanged?.Invoke();
       
    }

    // Removes any duplicate bindings that conflict with the newly assigned one
    private void RemoveDuplicateBindings(InputAction action, string newPath, int bindingIndex)
    {
        //foreach (var otherAction in inputActions)
        //{
        //    if (otherAction != action)  // Don't check the same action that was rebinding
        //    {
        //        for (int i = 0; i < otherAction.bindings.Count; i++)
        //        {
        //            var binding = otherAction.bindings[i];
        //            if (binding.effectivePath == newPath)
        //            {
        //                otherAction.RemoveBindingOverride(i);
        //                Debug.Log($"Removed duplicate binding for {otherAction.name} at binding index {i}");
        //            }
        //        }
        //    }
        //}
        foreach (var otheraction in inputActions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                if (otheraction == action && i == bindingIndex)
                    continue;

                if (action.bindings[i].effectivePath == newPath)
                {
                    action.RemoveBindingOverride(i);
                    Debug.Log($"Removed conflict: {action.name} binding {i}");
                }
            }
        }
    }

    // Resets a specific binding to its original configuration
    private void ResetSingleBinding(InputAction action, int bindingIndex)
    {
        var binding = action.bindings[bindingIndex];
        if (originalBindings.ContainsKey(binding.id.ToString()))
        {
            action.RemoveBindingOverride(bindingIndex);
        }

        // Save and refresh UI
        SaveBindings();
        OnBindingsChanged?.Invoke();
        
    }

    // Resets all bindings to their original configuration
    public void ResetBindings()
    {
        foreach (var action in inputActions)
        {
            for (int i = 0; i < action.bindings.Count; i++)
            {
                action.RemoveBindingOverride(i);
            }
        }

        // Save and refresh UI
        SaveBindings();
        OnBindingsChanged?.Invoke();
        
    }


    private void DisableInputs()
    {
        foreach (var actionMap in inputActions.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                action.Disable();
            }
        }
    }

    private void EnableInputs()
    {
        foreach (var actionMap in inputActions.actionMaps)
        {
            foreach (var action in actionMap.actions)
            {
                action.Enable();
            }
        }
    }
}
