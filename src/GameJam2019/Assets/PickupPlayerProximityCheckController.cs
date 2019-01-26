﻿using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class PickupPlayerProximityCheckController : MonoBehaviour
{
	private const float MAXIMUM_DISTANCE = 3f;

	public GameObject Player;
	public GameObject Camera;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
	    var component = Camera.GetComponent<CameraController>();
		var distance = (transform.position - Player.transform.position).magnitude;
	    if (component == null)
		    return;

	    component.Magnitude = distance < MAXIMUM_DISTANCE ? (MAXIMUM_DISTANCE - distance)  : 0f;
    }
}
