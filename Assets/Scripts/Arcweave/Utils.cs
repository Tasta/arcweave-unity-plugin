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
		public static string ParseHTML(string content, ref int linkedBoardId)
		{
			if (string.IsNullOrEmpty(content) || content == "null")
				return content;

			// Add the &nbsp HTML entity, and put everything inside a <root> element
			string nbspRef = "<!DOCTYPE doctypeName [\n   <!ENTITY nbsp \"&#160;\">\n]>";
			string rootedContent = nbspRef + " <root>" + content + "</root>";
			string parsedContent = "";

			// Create the XML reader
			XmlReaderSettings settings = new XmlReaderSettings();
			settings.ProhibitDtd = false;
			XmlReader reader = XmlReader.Create(new System.IO.StringReader(rootedContent), settings);

			// Values for parsing
			int paragraphCount = 0;
			bool inColorTag = false;

			// Start reading
			while (reader.Read()) {
				switch(reader.NodeType) {
					case XmlNodeType.DocumentType:
					case XmlNodeType.Whitespace:
					//parsedContent += " ";
						break;
					case XmlNodeType.Element:
						if (reader.Name == "root")
							break;
						else if (reader.Name == "b")
							parsedContent += "<b>";
						else if (reader.Name == "i")
							parsedContent += "<i>";
						else if (reader.Name == "p" && paragraphCount > 0)
							parsedContent += "\n\n";
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
									linkedBoardId = int.Parse(dataId);
//									// Fetch next element (which should be text)
//									reader.Read();
//									if (reader.NodeType != XmlNodeType.Text) {
//										Debug.LogWarning("[Arcweave] Unexpected node inside board reference: " + reader.NodeType);
//										break;
//									}
//
//									// Save reference to board name
//									string boardName = reader.Value;
//									linkedBoard = boardName;
								}

								// Color if reader has a data-type attribute
								if (!string.IsNullOrEmpty(dataType)) {
									parsedContent += "<color=orange>";
									inColorTag = true;
								}

								// Append the content
								parsedContent += reader.Value;
							}
						}
						break;
					case XmlNodeType.EndElement:
						if (reader.Name == "b")
							parsedContent += "</b>";
						else if (reader.Name == "i")
							parsedContent += "</i>";
						else if (reader.Name == "p")
							paragraphCount++;
						else if (reader.Name == "span") {
							if (inColorTag) {
								parsedContent += "</color>";
								inColorTag = false;
							}
						} else if (reader.Name == "root")
							break;
						else
							Debug.LogWarning("[Arcweave] Unhandled EndElement: " + reader.Name + " in " + content);
						break;
					case XmlNodeType.Text:
						parsedContent += reader.Value;
						break;
					default:
						Debug.LogWarning("[Arcweave] Unhandled node: " + reader.NodeType);
						break;
				}
			}

			// Return parsed content
			return parsedContent;
		}
	} // class Utils
} // namespace Utils

