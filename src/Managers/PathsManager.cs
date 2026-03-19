using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OutwardModsCommunicatorMenu.Managers
{
    public class PathsManager
    {
        private static PathsManager _instance;

        private PathsManager()
        {
            this.configPath = Path.Combine(OutwardModsCommunicator.Managers.PathsManager.ConfigPath, "Mods_Communicator_Menu");
            this.xmlFilePath = Path.Combine(this.configPath, "MyDocument.xml");
        }

        public static PathsManager Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new PathsManager();

                return _instance;
            }
        }

        public string configPath = "";
        public string xmlFilePath = "";
    }
}
