using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
	private Rigidbody2D _body;
	private float _horizontal;
	private float _vertical;
	
	public float RunSpeed = 5;

	private void Start()
	{
		_body = GetComponent<Rigidbody2D>();
	}

	private void Update()
	{
		_horizontal = Input.GetAxisRaw("Horizontal");
		_vertical = Input.GetAxisRaw("Vertical");
	}

	private void FixedUpdate()
	{
		_body.velocity = new Vector2(_horizontal, _vertical).ToNormalizedVector2() * RunSpeed;
	}
}

public static class Vector2Extensions
{
	public static Vector2 ToNormalizedVector2(this Vector2 source)
	{
		var result = new Vector2(source.x, source.y);
		result.Normalize();
		return result;
	}
}