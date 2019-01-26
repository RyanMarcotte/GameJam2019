using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class SelectOnInput : MonoBehaviour {

	public EventSystem EventSystem;
	public GameObject SelectedObject;

	private bool IsButtonSelected;

	void Update () 
	{
		if (Input.GetAxisRaw("Vertical") == 0 || IsButtonSelected) return;

		EventSystem.SetSelectedGameObject(SelectedObject);
		IsButtonSelected = true;
	}

	private void OnDisable() 
		=> IsButtonSelected = false;
}