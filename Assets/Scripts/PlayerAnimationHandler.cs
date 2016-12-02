using UnityEngine;
using System.Collections;

public class PlayerAnimationHandler : MonoBehaviour {

    private static PlayerAnimationHandler s_instance = null;
	// Use this for initialization
	void Start () {
        s_instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    public static void OnPlayerLand()
    {

    }

    public static void OnFall()
    {
        PlayerStates.Set(PlayerStates.AnimationParameter.Falling);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Running);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
    }

    public static void OnFallDodge()
    {
        PlayerStates.Set(PlayerStates.AnimationParameter.Falling);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Running);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgingInAir);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgeLanding);
    }

    public static void OnDodgeEnd()
    {
        PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgeLanding);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgingInAir);
    }

    public static void OnDodgeEndLandFall()
    {
        PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgingInAir);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgeLanding);
        PlayerStates.Set(PlayerStates.AnimationParameter.Falling);
    }

    public static void OnDodgeEndDamp()
    {
        PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgeLanding);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.DodgingInAir);
    }

    public static void OnDamp()
    {
        PlayerStates.Set(PlayerStates.AnimationParameter.Idling);
    }

    public static void OnRun()
    {
        PlayerStates.Set(PlayerStates.AnimationParameter.Running);
    }

    public static void OnUnIdle()
    {
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
    }
    public static void OnStop()
    {
        PlayerStates.Set(PlayerStates.AnimationParameter.Stop);
    }

    public static void OnNoMoveKeysPressed()
    {
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Running);
        PlayerStates.UnSet(PlayerStates.AnimationParameter.Idling);
    }
    public static void OnIdling()
    {
        PlayerStates.Set(PlayerStates.AnimationParameter.Idling);
    }
}
