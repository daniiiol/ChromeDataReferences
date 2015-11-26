using System.Linq;
using System.Text;
using Sitecore;
using Sitecore.Data;
using Sitecore.Data.Fields;
using Sitecore.Data.Items;
using Sitecore.Diagnostics;
using Sitecore.Layouts;
using Sitecore.Pipelines.GetChromeData;

namespace Namics.Opensource.ChromeDataReferences.Processor.Pipelines.getChromeData
{
    public class ChromeDataReferenceInfo : GetChromeDataProcessor
    {
        public override void Process(GetChromeDataArgs args)
        {
            Assert.ArgumentNotNull(args, "args");
            Assert.IsNotNull(args.ChromeData, "Chrome Data");

            switch (args.ChromeType.ToLower())
            {
                case "rendering":
                    RenderingElement(args);
                    break;
                case "field":
                    FieldElement(args);
                    break;
            }
        }

        /// <summary>
        /// Checks the references to this field and the publishing information of the field-item
        /// </summary>
        /// <param name="args">Sitecore Chrome Data Arguments</param>
        private void FieldElement(GetChromeDataArgs args)
        {
            Field argument = args.CustomData["field"] as Field;
            Item item = args.Item;
            var refBuilder = new StringBuilder();

            if (item != null && argument != null)
            {
                var format = "{0} (" + item.DisplayName + ")";
                args.ChromeData.DisplayName = string.Format(format, argument.DisplayName);
                if (!string.IsNullOrEmpty(argument.ToolTip))
                {
                    refBuilder.Append(string.Format(format, argument.ToolTip));
                }

                args.ChromeData.ExpandedDisplayName = refBuilder.ToString();
            }
        }

        /// <summary>
        /// Checks the references and the publishing information of the rendering datasource
        /// </summary>
        /// <param name="args">Sitecore Chrome Data Arguments</param>
        private void RenderingElement(GetChromeDataArgs args)
        {
            var argument = args.CustomData["renderingReference"] as RenderingReference;
            string format = string.Empty;
            Item item = args.Item;
            Item datasourceItem = argument != null && (!string.IsNullOrEmpty(argument.Settings.DataSource))
                ? argument.RenderingItem.Database.GetItem(new ID(argument.Settings.DataSource))
                : null;
            item = datasourceItem ?? item;
            var refBuilder = new StringBuilder();

            if (item != null)
            {
                var referenceItems = Globals.LinkDatabase.GetReferrers(item);
                if (referenceItems.Any())
                {
                    format = string.Format("{0} ({1}) - {2} References", item.DisplayName, args.ChromeData.DisplayName,
                        referenceItems.Count());

                    refBuilder.Append(string.Format("{0} \n ------------", format));

                    foreach (var referenceItem in referenceItems)
                    {
                        var sourceItem = referenceItem.GetSourceItem();
                        if (sourceItem != null)
                        {
                            refBuilder.Append(string.Format("\n - {0} ({1} - {2})", sourceItem.DisplayName,
                                sourceItem.Paths.FullPath, sourceItem.ID));
                        }
                    }
                    args.ChromeData.ExpandedDisplayName = refBuilder.ToString();
                }

                args.ChromeData.DisplayName = (string.IsNullOrEmpty(format)) ? args.ChromeData.DisplayName : format;
            }

            if (argument != null && !string.IsNullOrEmpty(argument.RenderingItem.InnerItem.Appearance.ShortDescription))
            {
                refBuilder.Append(string.Format("{0} \n\n", argument.RenderingItem.InnerItem.Appearance.ShortDescription));
            }

            args.ChromeData.ExpandedDisplayName = refBuilder.ToString();
        }
    }
}
