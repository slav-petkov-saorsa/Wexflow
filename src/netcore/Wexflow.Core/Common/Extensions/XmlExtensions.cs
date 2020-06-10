using System.Linq;
using System.Xml.Linq;

namespace Wexflow.Core.Common.Extensions
{
    public static class XmlExtensions
    {
        public static string MarkTasksNotStarted(this string workflowXmlString)
        {
            var document = XDocument.Parse(workflowXmlString);
            var tasksElement = document.Root.Elements().Single(e => e.Name.LocalName == "Tasks");
            foreach (var task in tasksElement.Elements())
            {
                task.Add(new XAttribute("status", 0));
            }
            
            return document.ToString();
        }
    }
}
