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
    public Text elementContent;
    public RectTransform elementLayout;
    public RectTransform actionLayout;
    public GameObject ActionPrefab;

    [Header("Component Items")]
    [Space(7.0f)]
    public GameObject componentParent;
    public RectTransform componentLayout;
    public GameObject ComponentPrefab;
    public ComponentView componentView;

    [Header("Back Action")]
    [Space(7.0f)]
    public GameObject backAction;
    public Button backActionBtn;
    public Text backActionLabel;

    // Link to the sample runner
    private Sample sample;

    // A stack of played elements, so we can always hit "Back"
    private Stack<Element> elementStack = new Stack<Element>();

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

        // Handle back button in respect of stack status
        backAction.SetActive(elementStack.Count > 0);

        // Force layout recalculations
        LayoutRebuilder.ForceRebuildLayoutImmediate(elementLayout);
        Canvas.ForceUpdateCanvases();

        // Push this element on stack
        elementStack.Push(awElement);
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

        // Set new ones, if necessary
        componentParent.SetActive(awElement.components.Length > 0);
        if (awElement.components.Length > 0) {
            for (int i = 0; i < awElement.components.Length; i++) {
                GameObject compObj = GameObject.Instantiate(ComponentPrefab, componentLayout.transform);
                compObj.SetActive(true);

                // The component label
                Text label = compObj.transform.Find("Text").GetComponent<Text>();
                label.text = awElement.components[i].name;

                // The component icon
                Image icon = compObj.transform.Find("Mask/Image").GetComponent<Image>();
                icon.sprite = awElement.components[i].image;
                icon.preserveAspect = true;

                // ToDo: Rescale to fit the circle neat and tidy
                RectTransform iconRT = icon.GetComponent<RectTransform>();
                RectTransform maskRT = iconRT.parent.GetComponent<RectTransform>();
                float widthRatio = maskRT.sizeDelta.x / iconRT.sizeDelta.x;
                float heightRatio = maskRT.sizeDelta.y / iconRT.sizeDelta.y;
                float minRatio = Mathf.Min(widthRatio, heightRatio);
                iconRT.localScale = Vector3.one * minRatio;

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
                string connLabel = awElement.outConnections[i].label;
                Text label = actionObj.transform.Find("Text").GetComponent<Text>();

                if (string.IsNullOrEmpty(connLabel) || connLabel == "null") {
                    // Fetch label from out node
                    Element e = sample.project.GetElement(awElement.outConnections[i].targetElementId);
                    label.text = e.GetTitle();
                } else {
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

                // And set default shiz
                backActionLabel.text = "Back";
            }

            // Bind back button to default
            BindBackBtn(DefaultBackAction);
        } else {
			if (awElement.title == "Game Over" || awElement.title == "Restart game") {
				backActionLabel.text = "Restart Game";

				// Bind an action to it, to advance the Play.
                BindBackBtn(() =>
				{
                    // Clear the stack
                    elementStack.Clear();

                    // Restart the sample
                    sample.Restart();
				});
			} else {
                backActionLabel.text = "Back";

				// Bind an action to it, to advance the Play.
                BindBackBtn(DefaultBackAction);
			}
        }

        // Resize back button action
        {
            const float marginSize = 10;
            const float decoSize = 75;
            float textSize = backActionLabel.preferredWidth;
            RectTransform btnRT = backActionBtn.GetComponent<RectTransform>();
            Vector2 size = btnRT.sizeDelta;
            size.x = marginSize + decoSize + textSize;
            btnRT.sizeDelta = size;
        }
    }

    ////////////////////////////////////////////////////////////
    // Back button usage
    private void BindBackBtn(UnityAction action)
    {
        // Clear it first
        backActionBtn.onClick.RemoveAllListeners();

        // Bind action
        backActionBtn.onClick.AddListener(action);
    }

    private void DefaultBackAction()
    {
        if (elementStack.Count == 0) {
            Debug.LogWarning("[SampleViewController] Back button shouldn't be available when there's no further |Back| node.");
            return;
        }

        // Pop current element
        elementStack.Pop();

        // And pop again to new current node
        Element newCurrent = elementStack.Pop();

        // Override in runner, which will repopulate
        sample.runner.active.SetCurrent(newCurrent);

        // Clear selection to avoid stupid button remaining selected/highlighted
        EventSystem.current.SetSelectedGameObject(null);
    }
} // class SampleViewController
