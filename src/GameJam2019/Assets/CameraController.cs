using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public GameObject Player;
	private Vector3 _offset;

	// Start is called before the first frame update
	void Start()
    {
		_offset = transform.position - Player.transform.position;
	}

    void LateUpdate()
    {
	    transform.position = Player.transform.position + _offset;
    }
}
