using System.Collections.Generic;

namespace Inforigami.Regalo.Interfaces
{
    public static class HeaderHelper
    {
        public static void CopyHeaders(ICommandHeaders sourceHeaders, IEventHeaders destinationHeaders)
        {
            destinationHeaders.CausationId = sourceHeaders.MessageId;
            destinationHeaders.CorrelationId = sourceHeaders.MessageId;

            if (destinationHeaders.CustomHeaders == null && sourceHeaders.CustomHeaders != null)
            {
                destinationHeaders.CustomHeaders = new Dictionary<string, string>();

                foreach (var customHeader in sourceHeaders.CustomHeaders)
                {
                    destinationHeaders.CustomHeaders.Add(customHeader.Key, customHeader.Value);
                }
            }
        }

        public static void CopyHeaders(IEventHeaders sourceHeaders, IEventHeaders destinationHeaders)
        {
            destinationHeaders.CausationId = sourceHeaders.MessageId;
            destinationHeaders.CorrelationId = sourceHeaders.CorrelationId;

            if (destinationHeaders.CustomHeaders == null && sourceHeaders.CustomHeaders != null)
            {
                destinationHeaders.CustomHeaders = new Dictionary<string, string>();

                foreach (var customHeader in sourceHeaders.CustomHeaders)
                {
                    destinationHeaders.CustomHeaders.Add(customHeader.Key, customHeader.Value);
                }
            }

            if (destinationHeaders.Topics == null && sourceHeaders.Topics != null)
            {
                destinationHeaders.Topics = new Dictionary<string, string>();

                foreach (var customHeader in sourceHeaders.Topics)
                {
                    destinationHeaders.Topics.Add(customHeader.Key, customHeader.Value);
                }
            }
        }
    }
}