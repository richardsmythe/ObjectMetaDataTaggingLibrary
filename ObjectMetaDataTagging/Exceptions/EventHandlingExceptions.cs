using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ObjectMetaDataTagging.Exceptions
{
    public class EventHandlingExceptions
    {
        public class TagHandlingException : Exception
        {
            public TagHandlingException(string message) : base(message)
            {
            }

            public TagHandlingException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        public class TagAdditionException : TagHandlingException
        {
            public TagAdditionException(string message) : base(message)
            {
            }

            public TagAdditionException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        public class TagRemovalException : TagHandlingException
        {
            public TagRemovalException(string message) : base(message)
            {
            }

            public TagRemovalException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }

        public class TagUpdateException : TagHandlingException
        {
            public TagUpdateException(string message) : base(message)
            {
            }

            public TagUpdateException(string message, Exception innerException) : base(message, innerException)
            {
            }
        }
    }
}
