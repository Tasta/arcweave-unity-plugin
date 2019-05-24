using SimpleJSON;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEditor;

namespace AW.Editor
{
    /*
     * A collection of static utilities for managing Boards.
     */
    public static class ProjectUtils
    {
        // Constant
        public const string projectFileName = "project_settings.json";
        public const string projectResourceFolder = "/Resources/Arcweave/";

        /*
         * Get project file at given path.
         */
        public static FileInfo GetProjectFile(string path)
        {
            string[] filePaths = Directory.GetFiles(path);

            FileInfo projectFileInfo = null;
            for (int i = 0; i < filePaths.Length; i++) {
                FileInfo fi = new FileInfo(filePaths[i]);
                if (fi.Name == projectFileName) {
                    // We're good, correct project path
                    projectFileInfo = fi;
                    return projectFileInfo;
                }
            }

            return null;
        }

        /*
         * Validate target project folder.
         */
        public static bool IsProjectFolderEmpty()
        {
            string resPath = Application.dataPath + projectResourceFolder;

            if (!Directory.Exists(resPath)) {
                CreateProjectFolders();
                return true;
            } else {
                // Check if there are any files in the directory
                string[] filePaths = Directory.GetFiles(resPath);
                return filePaths.Length == 0;
            }
        }

        /*
         * Create project folders.
         */
        public static void CreateProjectFolders()
        {
            string resPath = Application.dataPath + projectResourceFolder;
            Directory.CreateDirectory(resPath);

            // Create other folders
            Directory.CreateDirectory(resPath + "/Components");
            Directory.CreateDirectory(resPath + "/Boards");
        }

        /*
         * Clear project classes.
         */
        public static void ClearProjectFolder()
        {
            string resPath = Application.dataPath + projectResourceFolder;
            Directory.Delete(resPath, true);
        }

        /*
         * Read project
         */
        public static bool ReadProject(Project project, string projectFolder)
        {
            // Asset paths are relative to project folder
            string unityPrjFolder = Application.dataPath.Replace("Assets", "");
            projectFolder = projectFolder.Replace(unityPrjFolder, "");
            EditorUtility.DisplayProgressBar("Arcweave", "Creating components...", 15.0f);

            try {
                TextAsset projectAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(projectFolder + "/" + projectFileName);
                string projectContents = Encoding.UTF8.GetString(projectAsset.bytes);
                JSONNode root = JSONNode.Parse(projectContents);

                project.project = root["name"];

                // Read components
                EditorUtility.DisplayProgressBar("Arcweave", "Creating components...", 15.0f);
                ReadComponents(project, root["components"].AsObject, projectFolder);

                // Read elements
                EditorUtility.DisplayProgressBar("Arcweave", "Creating elements...", 30.0f);
                ReadElements(project, root["elements"].AsObject);

                // Read connections
                EditorUtility.DisplayProgressBar("Arcweave", "Creating connections...", 45.0f);
                ReadConnections(project, root["connections"].AsArray);

                // Read notes
                EditorUtility.DisplayProgressBar("Arcweave", "Creating notes...", 60.0f);
                ReadNotes(project, root["notes"].AsObject);

                // Read boards
                EditorUtility.DisplayProgressBar("Arcweave", "Creating boards...", 75.0f);
                BoardUtils.ReadBoards(project, root["boards"].AsObject);

                // Resolve references inside elements and connections
                // Must happen after reading everything, so Arcweave has the referenced elements instantiated.
                EditorUtility.DisplayProgressBar("Arcweave", "Preprocessing HTML...", 90.0f);
                for (int i = 0; i < project.elements.Length; i++) {
                    project.elements[i].ParseHTML(project);
                }

                for (int i = 0; i < project.connections.Length; i++) {
                    project.connections[i].ParseHTML(project);
                }
            } catch (Exception e) {
                Debug.LogError("[Arcweave] Cannot load project: " + e.Message + "\n" + e.StackTrace);
                EditorUtility.ClearProgressBar();
                return false;
            }

            EditorUtility.ClearProgressBar();
            return true;
        }

        /*
         * Read components from given JSON Class.
         */
        private static void ReadComponents(Project project, JSONClass componentRoot, string projectPath)
        {
            string componentPath = "Assets" + projectResourceFolder + "Components/";

            List<IComponentEntry> entries = new List<IComponentEntry>();

            IEnumerator children = componentRoot.GetEnumerator();
            while (children.MoveNext()) {
                // Get current
                KeyValuePair<string, JSONNode> current = (children.Current != null) ?
                    (KeyValuePair<string, JSONNode>)children.Current : default(KeyValuePair<string, JSONNode>);
                JSONNode child = current.Value;

                // Get its ID
                int id = int.Parse(current.Key);

                bool isFolder = child["children"] != null;

                if (isFolder) {
                    ComponentFolder folder = ScriptableObject.CreateInstance<ComponentFolder>();
                    folder.id = id;
                    ReadComponentFolder(folder, child);
                    entries.Add(folder);
                    AssetDatabase.CreateAsset(folder, componentPath + folder.name + ".asset");
                } else {
                    // Async operation because it might load images
                    Component component = ScriptableObject.CreateInstance<Component>();
                    component.id = id;
                    ReadComponent(component, child, projectPath);
                    entries.Add(component);
                    AssetDatabase.CreateAsset(component, componentPath + component.name + ".asset");
                }
            }

            project.components = entries.ToArray();
        }

        /*
         * Read component folder from JSON entry.
         */
        private static void ReadComponentFolder(ComponentFolder cf, JSONNode node)
        {
            cf.name = node["name"];

            JSONArray idxArray = node["children"].AsArray;
            if (idxArray.Count == 0)
                return;

            cf.childIds = new int[idxArray.Count];

            for (int i = 0; i < idxArray.Count; i++)
                cf.childIds[i] = idxArray[i].AsInt;
        }

        /*
         * Read component from JSON entry.
         */
        private static void ReadComponent(Component c, JSONNode root, string projectPath)
        {
            c.name = root["name"];

            // Attempt to load the image
            string imgPath = root["image"];
            if (!string.IsNullOrEmpty(imgPath) && imgPath != "null") {
                // Load sprite at given path
                string fullPath = projectPath + "/assets/" + imgPath;
                c.image = AssetDatabase.LoadAssetAtPath<Sprite>(fullPath);
                if (c.image == null) {
                    Debug.LogWarning("[Arcweave] Could not load image at path: " + fullPath + " for component " + c.name);
                }
            }

            // Load the attributes
            JSONArray attributeArray = root["attributes"].AsArray;
            List<Attribute> tmp = new List<Attribute>();
            for (int i = 0; i < attributeArray.Count; i++) {
                Attribute a = new Attribute();
                a.id = attributeArray[i]["id"].AsInt;
                a.label = attributeArray[i]["label"];
                a.content = attributeArray[i]["content"];
                tmp.Add(a);
            }
            c.attributes = tmp.ToArray();
        }

        /*
         * Read elements from JSON entry.
         */
        private static void ReadElements(Project project, JSONClass elementRoot)
        {
            List<Element> tmp = new List<Element>();

            IEnumerator children = elementRoot.GetEnumerator();
            while (children.MoveNext()) {
                // Get current
                KeyValuePair<string, JSONNode> current = (children.Current != null) ?
                    (KeyValuePair<string, JSONNode>)children.Current : default(KeyValuePair<string, JSONNode>);
                JSONNode child = current.Value;

                // Get id
                int id = int.Parse(current.Key);

                // Create element
                Element element = new Element();
                element.id = id;
                ReadElement(element, project, current.Value);

                // Add
                tmp.Add(element);
            }

            project.elements = tmp.ToArray();
        }

        /*
         * Read element from node.
         */
        private static void ReadElement(Element e, Project project, JSONNode root)
        {
            // Read & Parse Title
            e.title = root["title"];

            // Read & Parse Content
            e.content = root["content"];

            // Read components
            JSONArray componentArray = root["components"].AsArray;
            e.components = new Component[componentArray.Count];
            for (int i = 0; i < componentArray.Count; i++) {
                int compId = componentArray[i].AsInt;

                IComponentEntry cEntry = null;
                for (int cIdx = 0; cIdx < project.components.Length; cIdx++) {
                    if (project.components[cIdx].id == compId) {
                        cEntry = project.components[cIdx];
                        break;
                    }
                }

                if (cEntry == null) {
                    Debug.LogWarning("[Arcweave] Cannot find component for given id: " + compId);
                    continue;
                }

                if (!(cEntry is Component)) {
                    Debug.LogWarning("[Arcweave] Requested component id=" + compId + " found, but not valid.");
                    continue;
                }

                Component c = cEntry as Component;
                e.components[i] = c;
            }

            // Handle linked board tag
            string linkedBoardID = root["linkedBoard"];
            if (e.linkedBoard != null) {
                Debug.LogWarning("[Arcweave] Linked board became available for reading!");
            }
        }

        /*
         * Read connections from JSON entry.
         */
        private static void ReadConnections(Project project, JSONArray connectionArray)
        {
            project.connections = new Connection[connectionArray.Count];
            for (int i = 0; i < connectionArray.Count; i++) {
                JSONNode connNode = connectionArray[i];

                Connection c = new Connection();
                c.label = connNode["label"];
                c.sourceElementIdx = connNode["sourceid"].AsInt;
                c.targetElementIdx = connNode["targetid"].AsInt;
                c.AttributeConnections(project);
                project.connections[i] = c;
            }
        }

        /*
         * Read notes from JSON entry.
         */
        private static void ReadNotes(Project project, JSONClass noteRoot)
        {
            List<Note> tmp = new List<Note>();            

            IEnumerator children = noteRoot.GetEnumerator();
            while (children.MoveNext()) {
                // Get current
                KeyValuePair<string, JSONNode> current = (children.Current != null) ?
                    (KeyValuePair<string, JSONNode>)children.Current : default(KeyValuePair<string, JSONNode>);
                JSONNode child = current.Value;
                
                // Create element
                Note note = new Note(current.Key, current.Value);

                // Add
                tmp.Add(note);
            }

            project.notes = tmp.ToArray();
        }
    } // ProjectUtils
} // AW.Editor
