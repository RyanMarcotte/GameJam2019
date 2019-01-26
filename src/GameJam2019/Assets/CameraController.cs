using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public GameObject Player;
	private Vector3 _offset;
	private float _timeElapsed;

	// shake
	public float Magnitude;
	public float Intensity;
	private readonly Vector3 _axis = Vector3.right;

	// Start is called before the first frame update
	void Start()
    {
		_offset = transform.position - Player.transform.position;
	}

	void Update()
	{
		Magnitude -= 0.05f;
		if (Magnitude < 0f)
			Magnitude = 0f;

		if (Math.Abs(Magnitude) > 0)
			_timeElapsed += Time.deltaTime;
		else
			_timeElapsed = 0f;
	}

    void LateUpdate()
    {
	    var shakeOffset = _axis * (Magnitude * 0.25f) * (float)Math.Sin(10 * Intensity * _timeElapsed);
		transform.position = Player.transform.position + _offset + shakeOffset;
    }
}
