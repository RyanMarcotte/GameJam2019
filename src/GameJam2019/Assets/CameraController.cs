using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour
{
	public GameObject Player;
	public GameObject Map;
	private Vector3 _offset;
	private float _timeElapsed;

	// bounds
	private float _minX;
	private float _maxX;
	private float _minY;
	private float _maxY;

	// shake
	public float Magnitude;
	public float Intensity;
	private readonly Vector3 _axis = Vector3.right;

	// Start is called before the first frame update
	void Start()
    {
	    _offset = transform.position - Player.transform.position;
	    
	    var vertExtent = Camera.main.GetComponent<Camera>().orthographicSize;
	    var horzExtent = vertExtent * Screen.width / Screen.height;

	    var terrainGenerator = Map.GetComponent<TerrainGenerator>();
	    float mapX = terrainGenerator != null ? terrainGenerator.SizeX : 1000f;
	    float mapY = terrainGenerator != null ? terrainGenerator.SizeY : 1000f;

	    // Calculations assume map is position at the origin
	    _minX = horzExtent - mapX / 2;
	    _maxX = mapX / 2 - horzExtent;
	    _minY = vertExtent - mapY / 2;
	    _maxY = mapY / 2 - vertExtent;
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

	    var v3 = transform.position;
	    v3.x = Mathf.Clamp(v3.x, _minX, _maxX);
	    v3.y = Mathf.Clamp(v3.y, _minY, _maxY);
	    transform.position = v3;
	}
}
