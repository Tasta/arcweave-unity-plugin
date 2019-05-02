using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

using AW;

public class ComponentView : MonoBehaviour {
	// UI Components
	public Image icon;
	public Text title;
	public RectTransform attributeParent;
	public GameObject attributePrefab;

	public void Show(AW.Component component) {
		if (component == null) {
			Debug.LogWarning("Cannot show null component...");
			return;
		}

		// Activate
		gameObject.SetActive(true);

		// Set icon and title
		icon.sprite = component.image;
		title.text = component.name;

		// Populate attributes
		attributePrefab.SetActive(component.attributes.Count > 0);

		int i = 0;
		foreach (var item in component.attributes) {
			Attribute attribute = item.Value;

			GameObject prefab = null;
			if (i == 0)
				prefab = attributePrefab;
			else
				prefab = GameObject.Instantiate(attributePrefab, attributeParent);

			Text label = prefab.transform.Find("Label").GetComponent<Text>();
			Text value = prefab.transform.Find("ValueBox/Value").GetComponent<Text>();

			label.text = attribute.label;
			value.text = attribute.content;

			i++;
		}
	}

	private void ClearAttributes() {
		foreach (Transform child in attributeParent) {
			if (child.gameObject != attributePrefab)
				GameObject.Destroy(child.gameObject);
		}
	}

	public void OnClose() {
		ClearAttributes();
		gameObject.SetActive(false);
	}
} // class ComponentView
