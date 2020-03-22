using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace MookEditor.MOD
{
    public class ModConfig
    {
        public string name;
        public string version;
        public string path;
        public List<string> tags;
        public string supportedVersion;
        public string remoteFileId;
        public ModConfig(string modConfig)
        {
            name = Regex.Match(modConfig, "name *= *\".+\" *\\n").Value.Trim().Replace("name=", "");
            version = Regex.Match(modConfig, "version *= *\".+\" *\\n").Value.Trim().Replace("version=", "");
            path = Regex.Match(modConfig, "path *= *\".+\" *\\n").Value.Trim().Replace("path=", "");
            supportedVersion = Regex.Match(modConfig, "supported_version *= *\".+\" *\\n").Value.Trim().Replace("supported_version=", "");
            remoteFileId = Regex.Match(modConfig, "remote_file_id *= *\".+\" *\\n").Value.Trim().Replace("remote_file_id=", "");
            
            string tempTags = Regex.Match(modConfig, "tags *= *{.+} *\\n").Value;
            tempTags.Replace("tags","").Replace("=", "").Replace("{", "").Replace("}", "");
            string[] tempts = tempTags.Split('\n');
            tempts = tempts.Where(s => !string.IsNullOrEmpty(s.Trim())).ToArray();
            foreach (string t in tempts)
            {
                tags.Add(t);
            }
            
        }


    }
}
