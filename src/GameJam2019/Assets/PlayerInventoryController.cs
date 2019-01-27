using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerInventoryController : MonoBehaviour
{
	private readonly Dictionary<string, int> _itemCountLookup = new Dictionary<string, int>();

	public GameObject Home;
	public GameObject Win;

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
		if (itemIdentifierComponent.ID == "Item4")
		{
			Destroy(gameObject);
			Win.SetActive(true);
		}

		else
			Destroy(other.gameObject);
	}

	private void AddItem(string id, int count)
	{
		if (!_itemCountLookup.ContainsKey(id))
			_itemCountLookup.Add(id, 0);

		_itemCountLookup[id] += count;

		if (_itemCountLookup.Values.Sum() >= 3)
			Home.SetActive(true);
	}
}
