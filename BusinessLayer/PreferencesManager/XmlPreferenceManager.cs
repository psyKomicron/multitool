using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BusinessLayer.PreferencesManager
{
    public class XmlPreferenceManager : IPreferenceManager<XmlWindowPreferenceManager>
    {
        private List<XmlWindowPreferenceManager> managers = new List<XmlWindowPreferenceManager>(3);

        public string Path { get; set; }

        public void SerializePreferenceManager()
        {
            throw new NotImplementedException();
        }

        public void DeserializePreferenceManager()
        {
            throw new NotImplementedException();
        }

        public XmlWindowPreferenceManager AddPreferenceManager(XmlWindowPreferenceManager manager)
        {
            if (managers.Contains(manager)) // WPM implements IQuatable
            {
                XmlWindowPreferenceManager currentManager = null;
                foreach (var child in managers)
                {
                    if (child.ItemName.Equals(manager.ItemName))
                    {
                        currentManager = child;
                    }
                }
                if (currentManager != null && !currentManager.IsEquivalentTo(manager))
                {
                    managers.Remove(currentManager);
                    managers.Add(manager);
                }
            }
            else
            {
                managers.Add(manager);
            }
            return manager;
        }

        public XmlWindowPreferenceManager GetWindowManager(string name)
        {
            throw new NotImplementedException();
        }
  
    }
}
