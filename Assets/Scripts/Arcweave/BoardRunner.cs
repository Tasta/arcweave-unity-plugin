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
     * Gets created with a Project (for references to Arcweave entities)
     * and with a MonoBehaviour (to attach coroutines to).
     *
     * Walks the given board starting from its root node, 
     * while getting ticked with the player choices.
     */
    public class BoardWalker
    {
        // The board this runs on
        private Project project;
        private Board board;

        // Current element
        public Element current { get; protected set; }

        // The runner used for coroutines
        private MonoBehaviour runner;

        // Callback with current element in play
        private UnityAction<Element> onElementTriggered = null;

        /*
         * Constructor for runner.
         */
        public BoardWalker(Project project, MonoBehaviour runner) {
            this.project = project;
            this.runner = runner;
        }

        /*
         * Called to play the given board.
         */
        public void Play(Board board, UnityAction<Element> callback)
        {
            this.board = board;
            if (this.board.root == null) {
                Debug.LogWarning("[Arcweave] Board set for running has no valid root.");
                return;
            }

            // Set current from root
            current = this.board.root;

            // Set callback
            onElementTriggered = callback;

            // Play root node
            if (onElementTriggered != null)
                onElementTriggered(current);
        }

        /*
         * Called by the user to continue running the graph.
         */
        public void ChooseTransition(int transitionIndex)
        {
            // Start transition
            runner.StartCoroutine(Advance(transitionIndex));
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
        private IEnumerator Advance(int connection)
        {
            if (current == null) {
                Debug.LogWarning("[Arcweave] No current node found. Cannot advance.");
                yield break;
            }

            // Get next element
            Element nextElement = current.GetOutNeighbour(connection, project);
            current = nextElement;

            // Wait two frames to make sure we don't callback
            // in the same frame.
            yield return new WaitForEndOfFrame();
            yield return new WaitForEndOfFrame();

            if (onElementTriggered != null)
                onElementTriggered(current);
        }

        /*
         * Push the given element as current.
         * ToDo: Check if this realy is needed, or we add a "Trigger" method to each node
         *       and links to the Runner in order to make the Play themselves.
         */
        public void SetCurrent(Element element)
        {
            // ToDo: Check that this element belongs to the currently running Board.

            // Set current
            current = element;

            // And notify listener
            if (onElementTriggered != null)
                onElementTriggered(current);
        }
    } // class BoardWalker
} // namespace AW
