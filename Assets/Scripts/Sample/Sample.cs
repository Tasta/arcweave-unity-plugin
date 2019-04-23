using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AW;

public class Sample : MonoBehaviour {
    // The view of this sample
    public SampleViewController viewController;

    // The project
    public Project project { get; protected set; }

    // The test board walker
    public BoardWalker walker { get; protected set; }

	/*
     * Read the project on start.
     */
	void Start () {
        // Bind view-controller
        viewController.Bind(this);

        // Start the Play
        StartCoroutine(Play());
	}

    /*
     * Do test on Arcweave test project.
     */
    private IEnumerator Play()
    {
        project = new Project();
        yield return project.Read();

        Board main = project.GetBoardByName("Main Flow");
        main.ComputeRoot();

        // Create the walker
        walker = new BoardWalker(project, this);
        walker.Play(main, OnElementTriggered);

        Debug.LogWarning("Test started!");
    }

    /*
     * Gets called back by the runner when an element is in Play. 
     */
    public void OnElementTriggered(Element element)
    {
        if (element.title == "Game start") {
            // Only one transition from the start, to the intro
            walker.ChooseTransition(0);
        } else if (element.title == "Restart game") {
            // Restart from main flow
            Board main = project.GetBoardByName("Main Flow");

            // Create a fresh new walker
            walker = new BoardWalker(project, this);
            walker.Play(main, OnElementTriggered);
        } else {
            viewController.Populate(element);
            string label = null;

            if (element.outConnections.Count == 0) {
                label = "Back";
                // ToDo: Show single button with given action
            } else if (element.outConnections.Count == 1) {
                string action = element.outConnections[0].label;
                if (string.IsNullOrEmpty(action)) {
                    label = "Proceed";
                } else {
                    label = action;
                }
                // ToDo: Show single button with given action
            } else {
                // ToDo: Show all transitions
            }
        }
    }
} // class Sample
