using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MookEditor.MOD
{
    public class ModClass
    {
        public int level;
        public int id;
        public Type type;
        public string name;
        public string op;
        public string val;
        public List<ModClass> child = new List<ModClass>();
    }

    public enum Type
    {
        variable,
        function
    }
}
