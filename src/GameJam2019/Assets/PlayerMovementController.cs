﻿using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementController : MonoBehaviour
{
	private Rigidbody2D _body;
	private float _horizontal;
	private float _vertical;
	
	public float RunSpeed = 5;

	Animator _animator;
	private Vector3 _origPos;


	private void Start()
	{
		_body = GetComponent<Rigidbody2D>();
		_animator = GetComponent<Animator>();
		_origPos = transform.position;
	}

	private void Update()
	{
		_horizontal = Input.GetAxisRaw("Horizontal");
		_vertical = Input.GetAxisRaw("Vertical");
	}

	private void FixedUpdate()
	{
		_body.velocity = new Vector2(_horizontal, _vertical).ToNormalizedVector2() * RunSpeed;
		_animator.SetFloat("Speed", Math.Abs(_horizontal)+Math.Abs(_vertical));
		if (_body.velocity == Vector2.zero)
			_origPos = transform.position;
		SetRotation();
	}

	private void SetRotation()
	{
		if (_body.velocity != Vector2.zero)
		{
			float angle = Mathf.Atan2(_body.velocity.y, _body.velocity.x) * Mathf.Rad2Deg + 90;
			transform.rotation = Quaternion.AngleAxis(angle, new Vector3(0,0, 1));
		}
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

	public static Vector3 ToNormalizedVector3(this Vector3 source)
	{
		var result = new Vector3(source.x, source.y, source.z);
		result.Normalize();
		return result;
	}
}