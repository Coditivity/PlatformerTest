using UnityEngine;
using System.Collections;

public class ColliderHandler : MonoBehaviour {

   
    public enum ColliderType {
        Idle = 0,
        Running =1
    }
    [SerializeField]
    private BoxCollider2D[] _colliders;

    public static void SetCollider(ColliderType type)
    {
       
        PlayerMovement.PlayerCollider = s_instance._colliders[(int)type];
        
    }

    private static ColliderHandler s_instance = null;

	// Use this for initialization
	void Start () {
        s_instance = this;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
