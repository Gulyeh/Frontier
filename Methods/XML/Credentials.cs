using Frontier.Methods.Invoices;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Xml.Linq;

namespace Frontier.Methods.XML
{
    internal class Credentials
    {
        public static async Task CheckXML()
        {
            if (!File.Exists(Directory.GetCurrentDirectory() + @"\Config.xml"))
            {
                try
                {
                    new XDocument(
                    new XElement("Config",
                        new XElement("LastDB", "")
                    )
                ).Save(Directory.GetCurrentDirectory() + @"\Config.xml");
                }
                catch (Exception)
                {
                    await PermaDelete.Delete(Directory.GetCurrentDirectory(), "Config.xml");
                }
            }
        }
        public static Dictionary<string, string> ReadXML(string DBName)
        {
            try
            {
                XDocument doc = XDocument.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
                Dictionary<string, string> data = null;
                string lastDB = string.Empty;
                if (DBName != string.Empty)
                {
                    lastDB = DBName;
                }
                else
                {
                    lastDB = doc.Descendants("LastDB").FirstOrDefault().Value;
                    data = new Dictionary<string, string>
                    {
                        { "Database", lastDB }
                    };
                    return data;
                }

                if (lastDB != string.Empty)
                {
                    var findCredentials = doc.Root.Descendants("Credentials").Where(x => x.Attribute("Database").Value == lastDB).FirstOrDefault();
                    if (findCredentials != null)
                    {
                        if (findCredentials.Attribute("KeepLogged").Value == "True")
                        {
                            data = new Dictionary<string, string>
                            {
                                { "Login", Encoding.UTF8.GetString(Convert.FromBase64String(findCredentials.Attribute("Login").Value)) },
                                { "Password", Encoding.UTF8.GetString(Convert.FromBase64String(findCredentials.Attribute("Password").Value)) },
                                { "Database", lastDB }
                            };
                        }
                    }
                }
                return data;
            }
            catch (Exception) { return null; }
        }
        public static void SaveXML(Dictionary<string, string> credits)
        {
            try
            {
                XDocument doc = XDocument.Load(Directory.GetCurrentDirectory() + @"\Config.xml");
                var lastdb = doc.Root.Descendants("LastDB").FirstOrDefault();
                var data = doc.Root.Descendants("Credentials").Where(x => x.Attribute("Database").Value == credits["Database"]).FirstOrDefault();

                if (credits["Logged"].ToLower() == "true")
                {
                    if (data != null)
                    {
                        data.SetAttributeValue("Login", credits["Login"]);
                        data.SetAttributeValue("Password", credits["Password"]);
                        data.SetAttributeValue("Logged", "True");
                    }
                    else
                    {
                        doc.Element("Config").Add(new XElement("Credentials",
                             new XAttribute("Database", credits["Database"]),
                             new XAttribute("Login", credits["Login"]),
                             new XAttribute("Password", credits["Password"]),
                             new XAttribute("KeepLogged", "True")
                         ));
                    }
                }
                else
                {
                    if (data != null)
                    {
                        data.Remove();
                    }
                }

                lastdb.SetValue(credits["Database"]);
                doc.Save(Directory.GetCurrentDirectory() + @"\Config.xml");
            }
            catch (Exception)
            {
                MessageBox.Show("Nie udało się zapisać danych logowania");
            }
        }
    }
}
