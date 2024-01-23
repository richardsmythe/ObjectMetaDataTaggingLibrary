using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Exceptions
{
    [Serializable]
    public class ObjectNotFoundException : Exception
    {
        public string Name{ get; }
        public ObjectNotFoundException() { }

        public ObjectNotFoundException(string message)
            : base(message) { }

        public ObjectNotFoundException(string message, Exception inner)
            : base(message, inner) { }

        public ObjectNotFoundException(string message, string name)
        : this(message)
        {
            Name = name;
        }
    }
}
