using UnityEngine;
using System;
using System.Xml;
using System.IO;
using System.Collections.Generic;


namespace Studio.Common.Editor
{
    public class AndroidLocalization
    {
        public static string SetLocalizedValues(string[] languages, string baseLanguage, string postfix)
        {
            var defaultAppName = "Magic Coin";

            if (null != languages && !string.IsNullOrEmpty(postfix))
            {
                var rootPath = Path.Combine(Application.dataPath, "Plugins/Android/appres.androidlib/res/values");
                var locales = new List<Tuple<string, bool>>(languages.Length);

                foreach (var language in languages)
                {
                    locales.Add(new Tuple<string, bool>($"{rootPath}-{language}", language == baseLanguage));
                }

                foreach (var locale in locales)
                {
                    var xml = new XmlDocument();
                    xml.Load(Path.Combine(locale.Item1, "strings.xml"));

                    var root = xml.SelectNodes("resources")[0];
                    var nodes = root.ChildNodes;

                    for (var i = 0; i < nodes.Count; ++i)
                    {
                        for (var k = 0; k < nodes[i].Attributes.Count; ++k)
                        {
                            if (nodes[i].Attributes[k].Name.Equals("name") && nodes[i].Attributes[k].Value.Equals("app_name"))
                            {
                                var str = nodes[i].ChildNodes[k].InnerText;
                                if (str.EndsWith(postfix) == false)
                                {
                                    nodes[i].ChildNodes[k].InnerText += $" {postfix}";
                                }

                                if (locale.Item2)
                                {
                                    defaultAppName = nodes[i].ChildNodes[k].InnerText;
                                }
                                break;
                            }
                        }
                    }

                    xml.Save(Path.Combine(locale.Item1, "strings.xml"));
                }
            }

            return defaultAppName;
        }
    }
}

