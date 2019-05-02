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
     * Board Walker
     *
     * Gets created for each Board (containing references to Arcweave entities)
     * and with a MonoBehaviour (to attach coroutines to).
     *
     * Walks the given board starting from its root node, 
     * while getting ticked with the player choices.
     */
    public class BoardWalker
    {
        // The board this runs on
        private Project project;
		public Board board { get; protected set; }

        // Current element
        public Element current { get; protected set; }

        // Callback with current element in play
        private UnityAction<Element> onElementTriggered = null;

        /*
         * Constructor for runner.
         */
        public BoardWalker(Board board, Project project) {
			this.board = board;
            this.project = project;

			this.board = board;
			if (this.board.root == null) {
				throw new Exception("Board set for running has no valid root.");
			}

			// Set current from root
			current = this.board.root;
        }

		/*
		 * Set the element callback.
		 */
		public void SetElementCallback(UnityAction<Element> callback) {
			// Set callback
			onElementTriggered = callback;
		}

        /*
         * Advance the graph with given transition.
         *
         * Defined to be used as a coroutine because some elements
         * might have asynchronous operations.
         * E.g. An element that waits for N seconds before passing control
         *      to his outputs automatically.
         *
         * ToDo: Make it synchronous later if it proves useless.
         */
        public IEnumerator Advance(int connection)
        {
            if (current == null) {
                Debug.LogWarning("[Arcweave] No current node found. Cannot advance.");
                yield break;
            }

            // Get next element
            Element nextElement = current.GetOutNeighbour(connection, project);

			// Wait two frames to make sure we don't callback
			// in the same frame.
			yield return new WaitForEndOfFrame();
			yield return new WaitForEndOfFrame();

			if (nextElement.linkedBoard == null) {
				// Set next element as current
				current = nextElement;

				if (onElementTriggered != null)
					onElementTriggered(current);
			} else {
				// Pass it to the ProjectRunner to handle
				if (onElementTriggered != null)
					onElementTriggered(nextElement);
			}
        }

        /*
         * Push the given element as current.
         * ToDo: Check if this realy is needed, or we add a "Trigger" method to each node
         *       and links to the Runner in order to make the Play themselves.
         */
        public void SetCurrent(Element element)
        {
            // ToDo: Maybe check that this element belongs to the currently running Board.

			// Set current
			current = element;

			// And notify listener
			if (onElementTriggered != null)
				onElementTriggered(current);
        }
    } // class BoardWalker
} // namespace AW
