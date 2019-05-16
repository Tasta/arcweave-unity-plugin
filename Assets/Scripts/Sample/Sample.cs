using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using AW;

public class Sample : MonoBehaviour {
    // The view of this 
    public SampleViewController viewController;
    public CanvasGroup loader;

    // The project
    public Project project { get; protected set; }

    // The test board walker
	public ProjectRunner runner { get; protected set; }

    /*
     * Awake the loader.
     */
    void Awake()
    {
        loader.gameObject.SetActive(true);
    }

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

        // Destroy the Loader
        const float loadOffDuration = 0.66f;
        float accum = 0.0f;
        while (accum < loadOffDuration) {
            accum += Time.deltaTime;
            loader.alpha = Mathf.Lerp(1.0f, 0.0f, accum / loadOffDuration);
            yield return null;
        }
        GameObject.Destroy(loader.gameObject);

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
