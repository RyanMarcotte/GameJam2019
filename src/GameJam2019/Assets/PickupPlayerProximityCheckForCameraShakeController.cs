using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using UnityEngine;

public class PickupPlayerProximityCheckForCameraShakeController : MonoBehaviour
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
		if (component == null)
		    return;

		var distance = (transform.position - Player.transform.position).magnitude;
	    component.AddShake(distance < MAXIMUM_DISTANCE ? (MAXIMUM_DISTANCE - distance) : 0f);
    }

	void OnDestroy()
	{
		
	}
}
