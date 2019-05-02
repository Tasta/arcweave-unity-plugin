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
	public ProjectRunner runner { get; protected set; }

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

        // Create the walker
		runner = new ProjectRunner(project, this);
		runner.Play(OnElementTriggered);
        
        Debug.LogWarning("Test started!");
    }

    /*
     * Gets called back by the runner when an element is in Play. 
     */
    public void OnElementTriggered(Element element)
    {
		if (element.title == "Game start") {
			// Only one transition from the start, to the intro
			runner.ChooseTransition(0);
        } else {
            viewController.Populate(element);
        }
    }

	/*
	 * Restart game.
	 */
	public void Restart() {
		// Create a fresh new runner
		runner = new ProjectRunner(project, this);
		runner.Play(OnElementTriggered);
	}
} // class Sample
