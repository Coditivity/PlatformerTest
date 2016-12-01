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
}
