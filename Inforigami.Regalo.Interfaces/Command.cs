using System;

namespace Inforigami.Regalo.Interfaces
{
    public abstract class Command : ICommand
    {
        public ICommandHeaders Headers { get; private set; }

        protected Command()
        {
            Headers = new CommandHeaders();
        }

        public void OverwriteHeaders(ICommandHeaders headers)
        {
            if (headers == null) throw new ArgumentNullException("headers");

            Headers = headers;
        }
    }
}