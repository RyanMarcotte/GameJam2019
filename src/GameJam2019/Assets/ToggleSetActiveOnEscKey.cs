using UnityEngine;

public class ToggleSetActiveOnEscKey : MonoBehaviour
{
	public GameObject GameObjectToSet;

    void Update()
	{
		if (Input.GetKeyDown(KeyCode.Escape))
			GameObjectToSet.SetActive(!GameObjectToSet.activeSelf);
	}
}
