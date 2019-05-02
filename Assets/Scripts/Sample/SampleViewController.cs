using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

using AW;

/*
 * An example of a simple view to display the flow
 * in the scene's example Board.
 */
public class SampleViewController : MonoBehaviour
{
    // UI Components
    public Text title;
    public GameObject componentParent;
    public GameObject componentItem;
    public Text content;
    public GameObject actionParent;
    public GameObject actionItem;
	public ComponentView componentView;

    // Link to the sample runner
    private Sample sample;

    /*
     * Bind to 
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
        title.text = awElement.title;
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
        foreach (Transform child in componentParent.transform) {
            if (child.gameObject == componentItem)
                continue;
            GameObject.Destroy(child.gameObject);
        }

        // Set new ones, if necessary
        componentParent.SetActive(awElement.components.Count > 0);
        if (awElement.components.Count > 0) {
            for (int i = 0; i < awElement.components.Count; i++) {
                GameObject compObj = null;
                if (i == 0)
                    compObj = componentItem;
                else
                    compObj = GameObject.Instantiate(componentItem, componentParent.transform);

                Image icon = compObj.transform.Find("Icon").GetComponent<Image>();
                icon.sprite = awElement.components[i].image;

                // Bind an action to it, to show component description.
				AW.Component awComponent = awElement.components[i];
				Button actionBtn = compObj.GetComponent<Button>();
				actionBtn.onClick.RemoveAllListeners(); // Because I'm too lazy to clean up properly
				actionBtn.onClick.AddListener(() => {
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
            content.transform.parent.gameObject.SetActive(false);
        } else {
            content.transform.parent.gameObject.SetActive(true);
            content.text = awElement.content;
        }
    }

    /*
     * Populate actions
     */
    private void PopulateActions(Element awElement)
    {
        // Clean up old ones
        foreach (Transform child in actionParent.transform) {
            if (child.gameObject == actionItem)
                continue;
            GameObject.Destroy(child.gameObject);
        }

        // Set new ones, if necessary
        if (awElement.outConnections.Count > 0) {
            for (int i = 0; i < awElement.outConnections.Count; i++) {
                GameObject actionObj = null;
                if (i == 0)
                    actionObj = actionItem;
                else
                    actionObj = GameObject.Instantiate(actionItem, actionParent.transform);

                string connLabel = awElement.outConnections[i].label;
                Text label = actionObj.transform.Find("Label").GetComponent<Text>();
                label.text = (string.IsNullOrEmpty(connLabel) || connLabel == "null") ? "Proceed" : connLabel;

                // Bind an action to it, to advance the Play.
                int connIdx = i;
                Button btn = actionObj.GetComponent<Button>();
                btn.onClick.RemoveAllListeners();
                btn.onClick.AddListener(() =>
                {
                    // Choose this transition on user tap
                    sample.runner.ChooseTransition(connIdx);
                });
            }
        } else {
			if (awElement.title == "Game Over" || awElement.title == "Restart game") {
				Text label = actionItem.transform.Find("Label").GetComponent<Text>();
				label.text = "Restart Game";

				// Bind an action to it, to advance the Play.
				Button btn = actionItem.GetComponent<Button>();
				btn.onClick.RemoveAllListeners();
				btn.onClick.AddListener(() =>
				{
					// Restart the sample
					sample.Restart();
				});
			} else {
				Text label = actionItem.transform.Find("Label").GetComponent<Text>();
				label.text = "Back...";

				// Bind an action to it, to advance the Play.
				Button btn = actionItem.GetComponent<Button>();
				btn.onClick.RemoveAllListeners();
				btn.onClick.AddListener(() =>
				{
					// Go back to last element that had a label
					Element newRoot = awElement.GoBack(sample.project);
					sample.runner.active.SetCurrent(newRoot);
				});
			}
        }
    }
} // class SampleViewController
