using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

using AW;

/*
 * An example of a simple view to display the flow
 * in the scene's example Board.
 */
public class SampleViewController : MonoBehaviour
{
    [Header("Element Items")]
    public Image elementHeader;
    public Text elementContent;
    public VerticalLayoutGroup elementLayout;
    public RectTransform actionLayout;
    public GameObject ActionPrefab;

    [Header("Component Items")]
    [Space(7.0f)]
    public GameObject componentParent;
    public RectTransform componentLayout;
    public GameObject ComponentPrefab;
    public ComponentView componentView;

    // Link to the sample runner
    private Sample sample;

    /*
     * Bind to given sample.
     */
    public void Bind(Sample sample)
    {
        this.sample = sample;
    }

    /*
     * Populate from Element.
     */
    public void Populate(Element awElement)
    {
        PopulateComponents(awElement);
        PopulateContent(awElement);
        PopulateActions(awElement);
    }

    /*
     * Populate components from element.
     */
    private void PopulateComponents(Element awElement)
    {
        // Clean up old ones
        foreach (Transform child in componentLayout) {
            GameObject.Destroy(child.gameObject);
        }

        // Wrap the ComponentView back
        if (componentView.isUnwrapped)
            componentView.OnClose();

        // Set new ones, if necessary
        componentParent.SetActive(awElement.components.Length > 0);
        if (awElement.components.Length > 0) {
            for (int i = 0; i < awElement.components.Length; i++) {
                GameObject compObj = GameObject.Instantiate(ComponentPrefab, componentLayout.transform);
                compObj.SetActive(true);

                // The component label
                Text label = compObj.transform.Find("Text").GetComponent<Text>();
                label.text = awElement.components[i].realName;

                // The mask
                Sprite sprite = awElement.components[i].cover;
                RectTransform mask = compObj.transform.Find("Mask").GetComponent<RectTransform>();
                mask.gameObject.SetActive(sprite != null);
                if (sprite != null) {
                    // The component icon
                    Image icon = mask.Find("Image").GetComponent<Image>();
                    icon.sprite = awElement.components[i].cover;
                    icon.SetNativeSize();

                    if (sprite.texture.width > sprite.texture.height) {
                        // Scale by height
                        float scale = mask.rect.height / sprite.texture.height;
                        icon.transform.localScale = Vector3.one * scale;
                    } else {
                        // Scale by width
                        float scale = mask.rect.width / sprite.texture.width;
                        icon.transform.localScale = Vector3.one * scale;
                    }
                }

                // Bind an action to it, to show component description.
                AW.Component awComponent = awElement.components[i];
                Button actionBtn = compObj.GetComponent<Button>();
                actionBtn.onClick.RemoveAllListeners(); // Because I'm too lazy to clean up properly
                actionBtn.onClick.AddListener(() =>
                {
                    componentView.Show(awComponent);
                });
            }
        }
    }

    /*
     * Populate content.
     */
    private void PopulateContent(Element awElement)
    {
        elementHeader.sprite = awElement.cover;
        elementHeader.gameObject.SetActive(awElement.cover != null);

        // ToDo: Remove "null" comparison when null is properly passed.
        if (string.IsNullOrEmpty(awElement.content) || awElement.content == "null") {
            elementContent.text = "";
        } else {
            elementContent.text = awElement.content;
        }
    }

    /*
     * Populate actions
     */
    private void PopulateActions(Element awElement)
    {
        // Clean up old ones
        foreach (Transform child in actionLayout) {
            GameObject.Destroy(child.gameObject);
        }

        // Enable/Disable action parent
        bool hasActions = awElement.outConnections.Count > 0;
        actionLayout.gameObject.SetActive(hasActions);

        // Set new ones, if necessary
        if (hasActions) {
            // Put the action
            for (int i = 0; i < awElement.outConnections.Count; i++) {
                GameObject actionObj = GameObject.Instantiate(ActionPrefab, actionLayout.transform);

                // Set text
                string connLabel = awElement.outConnections[i].labelNoStyle;
                Text label = actionObj.transform.Find("Text").GetComponent<Text>();

                if (string.IsNullOrEmpty(connLabel) || connLabel == "null") {
                    // Fetch label from out node
                    Element e = sample.project.GetElement(awElement.outConnections[i].targetElementId);
                    label.text = e.GetActionLabel();
                } else {
                    const int maxDisplayChar = 40;

                    if (connLabel.Length > maxDisplayChar)
                        label.text = connLabel.Substring(0, maxDisplayChar) + "...";
                    else
                        label.text = connLabel;
                }

                // Bind an action to it, to advance the Play.
                int connIdx = i;
                Button btn = actionObj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    // Choose this transition on user tap
                    sample.runner.ChooseTransition(connIdx);
                });

                // Resize button to cover the text
                const float marginSize = 10;
                const float decoSize = 75;
                float textSize = label.preferredWidth;
                RectTransform btnRT = actionObj.GetComponent<RectTransform>();
                Vector2 size = btnRT.sizeDelta;
                size.x = marginSize + decoSize + textSize;
                btnRT.sizeDelta = size;
            }
        }
    }
} // class SampleViewController
