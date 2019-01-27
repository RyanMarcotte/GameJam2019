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

	private static IDictionary<Vector3, float> _magnitudes;

    // Start is called before the first frame update
    void Start()
    {
        _magnitudes = new Dictionary<Vector3, float>();
    }

    // Update is called once per frame
    void Update()
    {
	    var component = Camera.GetComponent<CameraController>();
		if (component == null)
		    return;

		var distance = (transform.position - Player.transform.position).magnitude;
		if (!_magnitudes.ContainsKey(transform.position))
			_magnitudes.Add(transform.position, 0);
		_magnitudes[transform.position] = distance < MAXIMUM_DISTANCE ? (MAXIMUM_DISTANCE - distance) : 0f;
		
	    component.Magnitude = _magnitudes.Values.Sum();
    }

	void OnDestroy()
	{
		_magnitudes.Remove(transform.position);
	}
}
