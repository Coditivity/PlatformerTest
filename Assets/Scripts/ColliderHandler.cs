using UnityEngine;
using System.Collections;

public class ColliderHandler : MonoBehaviour {

   
    public enum ColliderType {
        Idle = 0,
        Running =1
    }
    [SerializeField]
    private GameObject[] _colliderHolders;

    private BoxCollider2D[] _colliders;

    int lastColliderIndex = -1;
    public static void SetCollider(ColliderType type)
    {
        if (s_instance.lastColliderIndex >=0)
        {
            s_instance._colliderHolders[s_instance.lastColliderIndex].SetActive(false);
        }
        s_instance._colliderHolders[(int)type].SetActive(true);
        s_instance.lastColliderIndex = (int)type;
    }

    public static BoxCollider2D CurrentCollider
    {
        get
        {
            return s_instance._colliders[s_instance.lastColliderIndex];
        }
    }

    private static ColliderHandler s_instance = null;

	// Use this for initialization
	void Start () {
        s_instance = this;
        int i = 0;
        _colliders = new BoxCollider2D[_colliderHolders.Length];
        foreach(GameObject g in _colliderHolders)
        {
            _colliders[i++] = g.GetComponent<BoxCollider2D>();
        }
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
