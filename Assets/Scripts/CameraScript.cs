using UnityEngine;
using System.Collections;

public class CameraScript : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}

    void LateUpdate()
    {
        Vector3 camPos = Camera.main.transform.position;
        camPos.x = PlayerMovement.PlayerPosition.x;
        camPos.y = PlayerMovement.PlayerPosition.y;
        Camera.main.transform.position = camPos;
    }
}
