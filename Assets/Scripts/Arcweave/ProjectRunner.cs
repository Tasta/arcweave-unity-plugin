using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Events;

namespace AW
{
	/*
	 * Project walker.
	 * 
	 * Container for all BoardRunners in Arcweave.
	 * Wraps the usage of the active BoardRunner to manage
	 * the switch between boards.
	 */
	public class ProjectRunner
	{
		// The board this runs on
		private Project project;
		private List<BoardWalker> walkers;

		// The active board walker
		public BoardWalker active { get; protected set; }

		// Game callback
		private UnityAction<Element> onElementTriggered;

		// The MonoBehaviour to run plays on
		private MonoBehaviour runner;

		/*
		 * Create the runner from given project, with given runner.
		 */
		public ProjectRunner(Project project, MonoBehaviour runner)
		{
			this.project = project;
			this.runner = runner;

			walkers = new List<BoardWalker>();
			foreach (var item in project.boards) {
				if (!(item.Value is Board))
					continue;

				Board board = item.Value as Board;
				board.ComputeRoot();

				try {
					BoardWalker walker = new BoardWalker(board, this.project);
					walkers.Add(walker);
				} catch (Exception e) {
					Debug.LogWarning("[Arcweave] Cannot create BoardWalker for Board " + board.name + ".\n" + e.Message);
				}
			}

			// If there's no walker loaded, bail!
			if (walkers.Count == 0)
				throw new Exception("Cannot create ProjectRunner. No board loaded.");

			// ToDo: Maybe identify starting board by some other mechanism
			active = walkers[0];

			// Set callbacks to board runners
			walkers.ForEach(x => x.SetElementCallback(OnBoardCallback));
		}

		/*
         * Called to play with given callback.
         */
		public void Play(UnityAction<Element> callback)
		{
			// Set game callback to ProjectRunner
			onElementTriggered = callback;

			// Play root node of active board
			if (onElementTriggered != null)
				onElementTriggered(active.current);
		}

		/*
		 * Callback used with the BoardRunners to wrap their usage and add functionality
		 * to their callbacks.
		 */
		private void OnBoardCallback(Element element) {
			if (element.linkedBoard == null) {
				// Regular flow
				if (onElementTriggered != null)
					onElementTriggered(element);
			} else {
				BoardWalker nextBoard = walkers.Find(x => x.board == element.linkedBoard);
				if (nextBoard == null) {
					Debug.LogWarning("[Arcweave] Cannot find linked board runner for: " + element.linkedBoard.name);

					// Fallback to regular flow, pray to god something handles this
					if (onElementTriggered != null)
						onElementTriggered(element);
				} else {
					// Set next board as active
					active = nextBoard;

					// Basicly, a refresh to trigger the callback
					active.SetCurrent(active.current);
				}
			}
		}

		/*
         * Called by the user to continue running the graph.
         */
		public void ChooseTransition(int transitionIndex)
		{
			// Start transition
			runner.StartCoroutine(active.Advance(transitionIndex));
		}
	} // class ProjectRunner
} // namespace AW