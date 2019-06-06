using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

using AW;

public class ComponentView : MonoBehaviour {
	// UI Components
    [Header("Icon")]
    public RectTransform iconParent;
    public Image icon;

    [Header("Content")]
    public RectTransform contentParent;
	public Text title;
    public RectTransform scroller;
	public RectTransform attributeParent;
	public GameObject AttributePrefab;

    // Maximum image height is the initial one
    private Vector2 frameSize;

    void Awake()
    {
        frameSize = iconParent.sizeDelta;
    }

	public void Show(AW.Component component) {
		if (component == null) {
			Debug.LogWarning("Cannot show null component...");
			return;
		}

		// Activate
		gameObject.SetActive(true);

        // Set icon and compute size
        icon.sprite = component.image;
        if (component.image != null)
            HandlePictureSize();

        // Set title
        title.text = component.name;
        
		// Populate attributes
        for (int i = 0; i < component.attributes.Length; i++) {
        Attribute attribute = component.attributes[i];

            GameObject obj = GameObject.Instantiate(AttributePrefab, attributeParent);

            Text label = obj.transform.Find("Name").GetComponent<Text>();
            Text value = obj.transform.Find("Value").GetComponent<Text>();

            label.text = attribute.label.ToUpper();
            value.text = attribute.content;

            // Set attribute size manually, because VerticalLayoutGroup fucks up
            const float spacing = 8;
            float height = label.preferredHeight + spacing + value.preferredHeight;
            RectTransform attributeRT = obj.GetComponent<RectTransform>();
            Vector2 size = attributeRT.sizeDelta;
            size.y = height;
            attributeRT.sizeDelta = size;
        }

        // Do show animation
        StartCoroutine(DoViewAnimation(true, null));
    }

    private void HandlePictureSize()
    {
        Texture2D tex = icon.sprite.texture;
        RectTransform iconRT = icon.GetComponent<RectTransform>();

        // Copy size to icon
        iconRT.sizeDelta = new Vector2(tex.width, tex.height);

        // Compute the ratio, to properly resize
        float widthRatio = frameSize.x / tex.width;
        float heightRatio = frameSize.y / tex.height;
        float ratio = Mathf.Max(widthRatio, heightRatio);

        
        Vector2 size = iconRT.sizeDelta;
        size.x = size.x * ratio;
        size.y = size.y * ratio;
        iconRT.sizeDelta = size;
    }

	private void ClearAttributes() {
		foreach (Transform child in attributeParent) {
            if (child.name != "Name")
                GameObject.Destroy(child.gameObject);
        }
	}

	public void OnClose() {
		ClearAttributes();

        StartCoroutine(DoViewAnimation(false, () =>
        {
            gameObject.SetActive(false);
        }));
	}

    /*
     * Show/Hide animation.
     */
    public IEnumerator DoViewAnimation(bool show, UnityAction endCallback)
    {
        float xDelta = frameSize.x;

        Vector2 startPos, endPos;
        startPos.x = show ? xDelta : 0.0f;
        endPos.x = show ? 0.0f : xDelta;

        const float duration = 0.66f;
        float accum = 0.0f;

        RectTransform self = GetComponent<RectTransform>();
        while (accum < duration) {
            accum = Mathf.Min(accum + Time.deltaTime, duration);

            float lerpF = accum / duration;
            lerpF = lerpF * lerpF;

            float posX = Mathf.Lerp(startPos.x, endPos.x, lerpF);
            self.anchoredPosition = new Vector2(posX, 0.0f);

            yield return null;
        }

        if (endCallback != null)
            endCallback();
    }
} // class ComponentView
