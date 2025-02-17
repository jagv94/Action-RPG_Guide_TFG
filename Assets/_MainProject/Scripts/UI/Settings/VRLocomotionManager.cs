// VRLocomotionManager.cs (Compatible con XRI 3.0.7)
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Movement;
using UnityEngine.XR.Interaction.Toolkit.Locomotion.Turning;
using UnityEngine.XR.Interaction.Toolkit.Samples.StarterAssets;

public class VRLocomotionManager : MonoBehaviour
{
    public enum LocomotionType
    {
        MoveAndStrafe,
        TeleportAndTurn,
    }

    public enum TurnStyle
    {
        Snap,
        Smooth,
    }

    [SerializeField]
    private DynamicMoveProvider dynamicMoveProvider;

    [SerializeField]
    private ContinuousTurnProvider smoothTurnProvider;

    [SerializeField]
    private SnapTurnProvider snapTurnProvider;

    [SerializeField]
    private TwoHandedGrabMoveProvider twoHandedGrabMoveProvider;

    [SerializeField]
    private ControllerInputActionManager leftHandManager;

    [SerializeField]
    private ControllerInputActionManager rightHandManager;

    [SerializeField]
    private LocomotionType leftHandLocomotionType;

    [SerializeField]
    private LocomotionType rightHandLocomotionType;

    [SerializeField]
    private TurnStyle leftHandTurnStyle;

    [SerializeField]
    private TurnStyle rightHandTurnStyle;

    [SerializeField]
    private bool enableComfortMode;

    [SerializeField]
    private GameObject comfortMode;

    [SerializeField]
    private bool useGravity;

    [SerializeField]
    private bool enableFly;

    [SerializeField]
    private bool enableGrabMovement;

    private void Awake()
    {
        if (comfortMode == null)
            Debug.LogWarning("Comfort Mode GameObject is not set in the Locomotion Manager.", this);
    }

    private void OnEnable()
    {
        SetMoveScheme(leftHandLocomotionType, true);
        SetMoveScheme(rightHandLocomotionType, false);
        SetTurnStyle(leftHandTurnStyle, true);
        SetTurnStyle(rightHandTurnStyle, false);

        if (comfortMode != null)
            comfortMode.SetActive(enableComfortMode);

        dynamicMoveProvider.useGravity = useGravity;
        twoHandedGrabMoveProvider.useGravity = useGravity;

        dynamicMoveProvider.enableFly = enableFly;
        twoHandedGrabMoveProvider.enableFreeYMovement = enableFly;

        twoHandedGrabMoveProvider.enabled = enableGrabMovement;
    }

    private void SetMoveScheme(LocomotionType scheme, bool leftHand)
    {
        var targetHand = leftHand ? leftHandManager : rightHandManager;
        targetHand.smoothMotionEnabled = (scheme == LocomotionType.MoveAndStrafe);
    }

    private void SetTurnStyle(TurnStyle style, bool leftHand)
    {
        var targetHand = leftHand ? leftHandManager : rightHandManager;
        targetHand.smoothTurnEnabled = (style == TurnStyle.Smooth);
    }
}