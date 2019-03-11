using System;

namespace MogglesClient
{
    internal class MogglesClientException: Exception
    {
        internal MogglesClientException() { }

        internal MogglesClientException(string message): base(message) { }
    }
}
