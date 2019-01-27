using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CursorHandler : MonoBehaviour
{
	public Texture2D MouseOverTexture;

	public void OnMouseOver() 
		=> Cursor.SetCursor(MouseOverTexture, Vector2.zero, CursorMode.Auto);

	public void OnMouseExit() 
		=> Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
}