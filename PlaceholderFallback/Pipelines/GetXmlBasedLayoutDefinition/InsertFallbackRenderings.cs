using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Xml.Linq;
using System.Xml.XPath;
using Sitecore;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Mvc.Pipelines.Response.GetXmlBasedLayoutDefinition;

namespace HedgehogDevelopment.PlaceholderFallback.Pipelines.GetXmlBasedLayoutDefinition
{
    public class InsertFallbackRenderings : GetXmlBasedLayoutDefinitionProcessor
    {
        private const string FALLBACK_RENDERING_ID = "{95DED30F-F309-47A9-8505-F553123DD5DF}";
        public readonly static string FALLBACK_LOCATIONS_PROPERTY = "FallbackLocations";

        public override void Process(GetXmlBasedLayoutDefinitionArgs args)
        {
            ProcessFallbackRenderings(args);
        }

        /// <summary>
        /// Process fallback messages
        /// </summary>
        /// <param name="args"></param>
        private void ProcessFallbackRenderings(GetXmlBasedLayoutDefinitionArgs args)
        {
            XElement currentLayout = args.Result;

            XElement fallbackRendering = currentLayout.XPathSelectElement("d/r[@id='" + FALLBACK_RENDERING_ID + "' and not(@FallbackLocations)]");

            //Find the fallback rendering
            while (fallbackRendering != null)
            {
                XAttribute par = fallbackRendering.Attribute("par");

                if (par != null)
                {
                    string placeholdersParameter = HttpUtility.UrlDecode(StringUtil.ExtractParameter("FallbackPlaceholders", par.Value));

                    if (!string.IsNullOrEmpty(placeholdersParameter))
                    {
                        string[] placeholders = placeholdersParameter.Split(new string[] { "|" }, StringSplitOptions.RemoveEmptyEntries);

                        //If page editor, only store the parent path
                        if (Sitecore.Context.PageMode.IsPageEditor)
                        {
                            List<string> parents = new List<string>();

                            FindPlaceholderReplacement(args, placeholders, (parentItem, layout, placeholder) =>
                            {
                                if (GetFallbackRenderings(layout, placeholder).Any())
                                {
                                    parents.Add(parentItem.Paths.FullPath);

                                    return true;
                                }

                                return false;
                            });

                            //Add parent fallback locations for the content editor
                            string parentString = string.Join(", ", parents.Distinct());

                            fallbackRendering.Add(new XAttribute(FALLBACK_LOCATIONS_PROPERTY, parentString));
                        }
                        else
                        {
                            List<XElement> replacements = new List<XElement>();

                            FindPlaceholderReplacement(args, placeholders, (parentItem, layout, placeholder) =>
                            {
                                if (GetFallbackRenderings(layout, placeholder).Any())
                                {
                                    replacements.AddRange(GetFallbackRenderings(layout, placeholder));

                                    return true;
                                }

                                return false;
                            });

                            if (replacements.Any())
                            {
                                //Replace the contents with the renderings found in the parent
                                fallbackRendering.AddAfterSelf(replacements);
                            }

                            fallbackRendering.Remove();
                        }
                    }
                }

                fallbackRendering = currentLayout.XPathSelectElement("d/r[@id='" + FALLBACK_RENDERING_ID + "' and not(@FallbackLocations)]");
            }
        }

        /// <summary>
        /// Processes the placeholder replacements 
        /// </summary>
        /// <param name="args"></param>
        /// <param name="currentLayout"></param>
        /// <param name="placeholders"></param>
        private void FindPlaceholderReplacement(GetXmlBasedLayoutDefinitionArgs args,
            string[] placeholders,
            Func<Item, XDocument, string, bool> placeholderReplacementAction)
        {
            Item currentItem = args.ContextItem ?? Sitecore.Mvc.Presentation.PageContext.Current.Item;
            Item parentItem = currentItem.Parent;

            List<string> placeholdersToFind = new List<string>(placeholders);

            //Walk up parents
            while (parentItem != null && placeholdersToFind.Any())
            {
                XDocument layout = GetLayoutXml(parentItem);

                if (layout == null)
                {
                    break;
                }

                //Duplicate the list so we can remove placeholders we have found in parents
                List<string> currentPlaceholdersNeeded = new List<string>(placeholdersToFind);

                //Find any content in the placeholder
                foreach (string placeholder in currentPlaceholdersNeeded)
                {
                    if (placeholderReplacementAction(parentItem, layout, placeholder.Trim()))
                    {
                        //If something for the placeholder was found, remove it from the list of placeholders we are looking for
                        placeholdersToFind.Remove(placeholder);
                    }
                }

                parentItem = parentItem.Parent;
            }
        }

        /// <summary>
        /// Returns any renderings that are eligible for fallback
        /// </summary>
        /// <param name="layout"></param>
        /// <param name="placeholder"></param>
        /// <returns></returns>
        private IEnumerable<XElement> GetFallbackRenderings(XDocument layout, string placeholder)
        {
            foreach (XElement renderingInPlaceholder in layout.XPathSelectElements("r/d/r[contains(@ph, '" + placeholder + "')]"))
            {
                string ph = renderingInPlaceholder.Attribute("ph").Value;

                if (ph == placeholder || ph.EndsWith("/" + placeholder))
                {
                    XAttribute renderingIdAttribute = renderingInPlaceholder.Attribute("id");

                    if (renderingIdAttribute != null)
                    {
                        if (renderingIdAttribute.Value != FALLBACK_RENDERING_ID)
                        {
                            yield return new XElement(renderingInPlaceholder);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns a parsed layout document
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        private XDocument GetLayoutXml(Item item)
        {
            LayoutField lf = new LayoutField(item);

            Field field = new LayoutField(item).InnerField;
            if (field == null)
            {
                return null;
            }

            string layoutXml = LayoutField.GetFieldValue(field);
            if (string.IsNullOrWhiteSpace(layoutXml))
            {
                return null;
            }

            try
            {
                return XDocument.Parse(layoutXml);
            }
            catch (Exception ex)
            {
                Log.Error("Could not parse layout for " + item.Paths.FullPath, ex, this);

                return null;
            }
        }
    }
}