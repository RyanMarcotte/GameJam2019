using System;
using System.Collections;
using System.Collections.Generic;
using Extensions;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
	private Rigidbody2D _body;
	private float _horizontal;
	private float _vertical;
	private float _moveLimiter = 0.7f;
	
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