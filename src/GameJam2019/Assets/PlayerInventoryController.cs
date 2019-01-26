using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
	private readonly Dictionary<string, int> _itemCountLookup = new Dictionary<string, int>();

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

	void OnTriggerEnter2D(Collider2D other)
	{
		var itemIdentifierComponent = other.gameObject.GetComponent<ItemIdentifierComponent>();
		if (itemIdentifierComponent == null)
			return;

		AddItem(itemIdentifierComponent.ID, 1);
		Destroy(other.gameObject);
	}

	private void AddItem(string id, int count)
	{
		if (!_itemCountLookup.ContainsKey(id))
			_itemCountLookup.Add(id, 0);

		_itemCountLookup[id] += count;
	}
}
