using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;


namespace AW.Editor
{
    /*
     * The editor GUI styles used in the ArcweaveWindow.
     */
    public static class Resources
    {
        public static Styles styles = new Styles();

        /*
         * Class for instantiating and holding all styles.
         */
        public class Styles
        {
            public GUIStyle folderLabelStyle;
            public GUIStyle boardSelectionLabelStyle;
            public GUIStyle refreshWarningLabelStyle;

            public Styles()
            {
                folderLabelStyle = new GUIStyle("Label");
                folderLabelStyle.wordWrap = true;

                boardSelectionLabelStyle = new GUIStyle("Label");
                boardSelectionLabelStyle.wordWrap = true;

                refreshWarningLabelStyle = new GUIStyle("Label");
                refreshWarningLabelStyle.wordWrap = true;
                refreshWarningLabelStyle.normal.textColor = Color.yellow;
            }
        }
    } // class ArcweaveStyles
} // namespace AW
