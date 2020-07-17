using System;
using System.Xml;
using UnityEngine;

namespace AW
{
	/*
	 * A static collection of utilities used inside Arcweave.
	 */
	public class Utils
	{
		/*
		 * Parse the HTML imported from the Arcweave project.
		 *
		 * Use the C# built-in XML parser.
		 * Although this is HTML code, we hack it a bit to parse as XML.
		 *
		 * Resolve the |board| reference.
		 */
		public static void ParseHTML(string content, ref string parsedContent, ref string parsedContentNoStyle, ref string linkedBoardId)
		{
            if (string.IsNullOrEmpty(content) || content == "null")
                return;

            // Handle <br>
            content = content.Replace("<br>", "<br />");

            // Add the &nbsp HTML entity, and put everything inside a <root> element
            string xmlHeader = "<?xml version=\"1.0\" encoding=\"ISO-8859-9\"?>\n";
			string nbspRef = "<!DOCTYPE doctypeName [\n   <!ENTITY nbsp \"&#160;\">\n]>";
			string rootedContent = xmlHeader + nbspRef + " <root>" + content + "</root>";
            parsedContent = "";
            parsedContentNoStyle = "";

			// Create the XML reader
			XmlReaderSettings settings = new XmlReaderSettings();
            settings.DtdProcessing = DtdProcessing.Ignore;
			XmlReader reader = XmlReader.Create(new System.IO.StringReader(rootedContent), settings);

			// Values for parsing
			int paragraphCount = 0;
			bool inColorTag = false;

            try {
                // Start reading
                while (reader.Read()) {
                    switch (reader.NodeType) {
                        case XmlNodeType.DocumentType:
                        case XmlNodeType.Whitespace:
                            //parsedContent += " ";
                            break;
                        case XmlNodeType.Element:
                            if (reader.Name == "root")
                                break;
                            else if (reader.Name == "b" || reader.Name == "strong")
                                parsedContent += "<b>";
                            else if (reader.Name == "i" || reader.Name == "em")
                                parsedContent += "<i>";
                            else if (reader.Name == "p" && paragraphCount > 0)
                                parsedContent += "\n\n";
                            else if (reader.Name == "u")
                                break; // Don't have underline yet
                            else if (reader.Name == "a")
                                break; // Don't have Anchors yet
                            else if (reader.Name == "span") {
                                if (reader.HasAttributes) {
                                    // Check data-type and data-id for links
                                    string dataType = reader.GetAttribute("data-type");
                                    string dataId = reader.GetAttribute("data-id");

                                    // Validate
                                    if (!string.IsNullOrEmpty(dataType) &&
                                        string.IsNullOrEmpty(dataId)) {
                                        Debug.LogWarning("[Arcweave] Data type " + dataType + " with no id encountered.");
                                        break;
                                    }

                                    // ToDo: Add component links too
                                    if (dataType == "board") {
                                        linkedBoardId = dataId;
                                    }

                                    // Color if reader has a data-type attribute
                                    //if (!string.IsNullOrEmpty(dataType)) {
                                    //	parsedContent += "<color=orange>";
                                    //	inColorTag = true;
                                    //}

                                    // Append the content
                                    parsedContent += reader.Value;
                                    parsedContentNoStyle += reader.Value;
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            if (reader.Name == "b" || reader.Name == "strong")
                                parsedContent += "</b>";
                            else if (reader.Name == "i" || reader.Name == "em")
                                parsedContent += "</i>";
                            else if (reader.Name == "p")
                                paragraphCount++;
                            else if (reader.Name == "u")
                                break; // Don't have underline yet
                            else if (reader.Name == "a")
                                break; // Don't have Anchors yet
                            else if (reader.Name == "span") {
                                if (inColorTag) {
                                    //parsedContent += "</color>";
                                    inColorTag = false;
                                }
                            } else if (reader.Name == "root")
                                break;
                            else
                                Debug.LogWarning("[Arcweave] Unhandled EndElement: " + reader.Name + " in " + content);
                            break;
                        case XmlNodeType.Text:
                            parsedContent += reader.Value;
                            parsedContentNoStyle += reader.Value;
                            break;
                        case XmlNodeType.XmlDeclaration:
                            break;
                        default:
                            Debug.LogWarning("[Arcweave] Unhandled node: " + reader.NodeType);
                            break;
                    }
                }
            } catch (Exception e) {
                Debug.LogWarning("[Arcweave] Failed to parse content. (see below)\n" + content + "\n" + e.Message);
            }
		}
	} // class Utils
} // namespace Utils

