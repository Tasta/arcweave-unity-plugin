using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEditor;

namespace AW.Editor
{
    /*
     * The package window for setting up Arcweave.
     */
    public class ArcweaveWindow : EditorWindow
    {
        /*
         * Add a menu item to open this window.
         */
        [MenuItem("Component/Arcweave/Settings")]
        static void OpenWindow()
        {
            // Show
            ArcweaveWindow.ShowSettings();
        }

        [MenuItem("Component/Arcweave/Reset settings")]
        static void ClearProject()
        {
            ProjectUtils.ClearProjectFolder();
            if (ArcweaveWindow.Instance != null) {
                ArcweaveWindow.Instance.folderPath = null;
                ArcweaveWindow.Instance.project = null;
                ArcweaveWindow.Instance.Repaint();
            }
                
            Debug.Log("[Arcweave] Project folder cleared.");
        }

        // Members
        private static ArcweaveWindow Instance;
        private string folderPath = null;
        private Project project = null;

        // Precomputed lists of boards and elements
        private string[] boardNameList;
        private List<Element> potentialRoots;
        private string[] elementNameList;

        /*
         * Function called to create and show this window.
         */
        public static void ShowSettings()
        {
            Instance = GetWindow(typeof(ArcweaveWindow), false, "Arcweave") as ArcweaveWindow;
            Instance.project = AssetDatabase.LoadAssetAtPath<Project>("Assets/Resources/Arcweave/Project.asset");

            if (Instance.project != null) {
                // Load stuff from the object
                Instance.folderPath = Instance.project.folderPath;
                Instance.PrecomputeBoardNames();

                Board main = Instance.project.boards[Instance.project.startingBoardIdx];
                Instance.PrecomputeElementNames(main);
            }
        }

        /*
         * Draw contents of the window.
         */
        public void OnGUI()
        {
            try {
                // Setup the title
                EditorGUILayout.LabelField("Project Settings");

                EditorGUILayout.Separator();
                EditorGUILayout.Separator();
                EditorGUILayout.Separator();

                // Handle Project path
                HandleProjectFolderSelector();

                if (project != null) {
                    // Check if timestamp changed
                    FileInfo prjFile = new FileInfo(project.folderPath + "/" + ProjectUtils.projectFileName);
                    if (prjFile.Exists && prjFile.LastWriteTimeUtc.ToBinary() != project.sourceTimestamp) {
                        // Some space
                        for (int i = 0; i < 5; i++)
                            EditorGUILayout.Space();

                        // Handle project refresh
                        HandleProjectRefresh(project, prjFile.LastWriteTimeUtc);
                    }

                    // Handle board and root element selection
                    if (project.startingBoardIdx != -1) {
                        for (int i = 0; i < 5; i++)
                            EditorGUILayout.Space();

                        // Hint
                        const string selectionHint = "This section is for assigning the starting board, and the starting node for each board.";
                        EditorGUILayout.LabelField(selectionHint, Resources.styles.folderLabelStyle);

                        // Handle Board Selection
                        HandleStartingBoard(project);

                        // Handle Root Node Selection
                        HandleStartingRoot(project);
                    }
                }
            } catch (Exception e) {
                Debug.LogWarning("[Arcweave] Cannot draw plugin settings window: " + e.Message + "\n" + e.StackTrace);
            }
        }

        /*
         * Draw the project folder selection.
         */
        private void HandleProjectFolderSelector()
        {
            UnityAction<string> onSetFolder = (string currentPath) =>
            {
                string newFolderPath = EditorUtility.OpenFolderPanel("Project path", currentPath, null);

                // Check that project is inside Unity's Assets
                DirectoryInfo dir = new DirectoryInfo(newFolderPath);
                DirectoryInfo assetDir = new DirectoryInfo(Application.dataPath);
                if (!dir.FullName.Contains(assetDir.FullName)) {
                    string message = "The Arcweave project must be placed inside Unity's Asset Folder.\n";
                    message += "Unity Asset Folder: " + assetDir.FullName + "\n";
                    message += "Selected Folder: " + dir.FullName + "\n";
                    message += "Please try again.";
                    EditorUtility.DisplayDialog("Wrong Path", message, "Okay");
                    return;
                }

                // Check project file
                FileInfo projectFileInfo = ProjectUtils.GetProjectFile(newFolderPath);
                if (projectFileInfo == null) {
                    string message = "Oops... We cannot find the project at the specified path:\n";
                    message += newFolderPath;
                    message += "\nPlease try again.";
                    EditorUtility.DisplayDialog("Arcweave Project missing", message, "Okay");
                } else {
                    bool generateClasses = true;
                    if (!ProjectUtils.IsProjectFolderEmpty()) {
                        string message = "Generated Project folder is not empty.\n";
                        message += "Do you wish to proceed?\n";
                        message += "(doing this will delete all contents in Assets/Resources/Arcweave/)";
                        bool continueWithErasing = EditorUtility.DisplayDialog("Directory not empty", message, "Do it", "Nevermind");
                        if (continueWithErasing) {
                            ProjectUtils.ClearProjectFolder();
                            ProjectUtils.CreateProjectFolders();
                            generateClasses = true;
                        }
                    }

                    // Proceed with generation
                    if (generateClasses) {
                        // Start generating
                        Project newProject = CreateInstance<Project>();
                        if (ProjectUtils.ReadProject(newProject, newFolderPath)) {
                            // Set new folder path
                            folderPath = newFolderPath;

                            // Set board
                            project = newProject;
                            project.folderPath = newFolderPath;
                            project.startingBoardIdx = 0;
                            project.boardRootId = newProject.boards[0].elements[0].id;

                            // Setup the board names
                            PrecomputeBoardNames();

                            // Setup the element names
                            Board main = project.boards[project.startingBoardIdx];
                            PrecomputeElementNames(main);

                            // Setup the project file timestamp
                            project.sourceTimestamp = projectFileInfo.LastWriteTimeUtc.ToBinary();

                            // Save project asset
                            string projectPath = "Assets" + ProjectUtils.projectResourceFolder + "Project.asset";
                            AssetDatabase.CreateAsset(newProject, projectPath);
                            AssetDatabase.SaveAssets();
                        } else {
                            DestroyImmediate(newProject);
                        }
                    }
                }
            };

            if (string.IsNullOrEmpty(folderPath)) {
                EditorGUILayout.LabelField("No project folder selected.");
                if (GUILayout.Button("Setup")) {
                    onSetFolder(Application.dataPath);
                }
            } else {
                EditorGUILayout.LabelField("Current project path: " + folderPath, Resources.styles.folderLabelStyle);
                if (GUILayout.Button("Change")) {
                    onSetFolder(folderPath);
                }
            }
        }

        /*
         * Have the user select which board to run when the sample starts.
         */
        private void HandleStartingBoard(Project project)
        {
            EditorGUILayout.LabelField("Select a board:");

            int selectedIdx = project.startingBoardIdx;
            int newIdx = EditorGUILayout.Popup(selectedIdx, boardNameList);

            if (newIdx != selectedIdx) {
                project.startingBoardIdx = newIdx;
                project.boardRootId = null;
                PrecomputeElementNames(project.boards[project.startingBoardIdx]);
            }
        }

        /*
         * Have the user select which node to be used as root when this board is played.
         */
        private void HandleStartingRoot(Project project)
        {
            EditorGUILayout.LabelField("Select a root node:");

            Element current = potentialRoots.Find(x => x.id == project.boardRootId);
            int selectedIdx = (current != null) ? potentialRoots.IndexOf(current) : 0;
            int newIdx = EditorGUILayout.Popup(selectedIdx, elementNameList);
            project.boardRootId = potentialRoots[newIdx].id;
        }

        /*
         * Precompute board names.
         */
        private void PrecomputeBoardNames()
        {
            // Get list of boards
            boardNameList = new string[project.boards.Length];
            for (int i = 0; i < project.boards.Length; i++)
                boardNameList[i] = project.boards[i].name;
        }

        /*
         * Precompute element names.
         */
        private void PrecomputeElementNames(Board board)
        {
            potentialRoots = BoardUtils.ComputeRoots(board);
            elementNameList = new string[potentialRoots.Count];
            for (int i = 0; i < potentialRoots.Count; i++) {
                elementNameList[i] = potentialRoots[i].GetTitle();
            }
        }

        /*
         * Handle project refresh button.
         */
        private void HandleProjectRefresh(Project project, DateTime newTimestamp)
        {
            EditorGUILayout.LabelField("Project sources are newer than the generated Arcweave data.", Resources.styles.refreshWarningLabelStyle);
            if (GUILayout.Button("Refresh")) {
                if (ProjectUtils.ReadProject(project, project.folderPath)) {
                    project.sourceTimestamp = newTimestamp.ToBinary();
                    EditorUtility.SetDirty(project);
                    AssetDatabase.SaveAssets();
                }
            }
        }
    } // class ArcweaveWindow
} // namespace AW

