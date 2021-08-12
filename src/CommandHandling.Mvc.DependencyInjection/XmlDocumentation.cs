using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Xml;

namespace CommandHandling.Mvc.DependencyInjection
{
    public class XmlDocumentation
    {
        private HashSet<Assembly> loadedAssemblies = new HashSet<Assembly>();
        private Dictionary<string, string> loadedXmlDocumentation = new Dictionary<string, string>();

        public void LoadXmlDocumentation(Assembly assembly)
        {
            string directoryPath = GetDirectoryPath(assembly);
            string xmlFilePath = Path.Combine(directoryPath, assembly.GetName().Name + ".xml");
            if (File.Exists(xmlFilePath)) {
                LoadXmlDocumentation(System.IO.File.ReadAllText(xmlFilePath));
            }
        }

        public string GetDocumentation(MethodInfo methodInfo)
        {
            int genericParameterCounts = methodInfo.GetGenericArguments().Length;
            ParameterInfo[] parameterInfos = methodInfo.GetParameters();
            string key = "M:" +
                Regex.Replace(methodInfo.DeclaringType.FullName, @"\[.*\]", string.Empty).Replace('+', '.') + "." + methodInfo.Name +
                (genericParameterCounts > 0 ? "`" + genericParameterCounts : string.Empty) +
                (parameterInfos.Length > 0 ? "(" + string.Join(",", parameterInfos.Select(x => x.ParameterType.ToString())) + ")" : string.Empty);
            loadedXmlDocumentation.TryGetValue(key, out string documentation);
            return documentation;
        }

        private string XmlDocumentationKeyHelper(
            string typeFullNameString,
            string memberNameString)
        {
            string key = Regex.Replace(
            typeFullNameString, @"\[.*\]",
            string.Empty).Replace('+', '.');
            if (memberNameString != null)
            {
                key += "." + memberNameString;
            }
            return key;
        }
        private string GetDirectoryPath(Assembly assembly)
        {
            string codeBase = assembly.Location;
            UriBuilder uri = new UriBuilder(codeBase);
            string path = Uri.UnescapeDataString(uri.Path);
            return Path.GetDirectoryName(path);
        }
        private void LoadXmlDocumentation(string xmlDocumentation)
        {
            using (XmlReader xmlReader = XmlReader.Create(new StringReader(xmlDocumentation)))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType == XmlNodeType.Element && xmlReader.Name == "member")
                    {
                        string raw_name = xmlReader["name"];
                        var innerXml = xmlReader.ReadInnerXml();
                        loadedXmlDocumentation[raw_name] = innerXml.Replace($"{innerXml[0]}", Environment.NewLine);
                    }
                }
            }
        }
    }
}
