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

                // ToDo: Bind an action to it, to show component description.
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
                    sample.walker.ChooseTransition(connIdx);
                });
            }
        } else {
            Text label = actionItem.transform.Find("Label").GetComponent<Text>();
            label.text = "Back...";

            // Bind an action to it, to advance the Play.
            Button btn = actionItem.GetComponent<Button>();
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() =>
            {
                for (int i = 0; i < awElement.inConnections.Count; i++) {
                    Element source = awElement.GetInNeighbour(i, sample.project);
                    sample.walker.SetCurrent(source);
                }
            });
        }
    }
} // class SampleViewController
