using System;
using System.Collections;
using System.Collections.Generic;

namespace Inforigami.Regalo.Interfaces
{
    /// <summary>
    /// Basic contract for all messages.
    /// </summary>
    public interface IMessage
    {
        /// <summary>
        /// Unique ID for any this specific message.
        /// </summary>
        Guid MessageId { get; set; }
    
        /// <summary>
        /// The timestamp for this specific message.
        /// </summary>
        DateTimeOffset Timestamp { get; set; }

        /// <summary>
        /// The ID of the message that caused this message to be generated. Not carried forward.
        /// </summary>
        Guid CausationId { get; set; }

        /// <summary>
        /// The MessageId of the originating message, carried forward to all messages in a chain.
        /// </summary>
        Guid CorrelationId { get; set; }

        /// <summary>
        /// The local date/time of the originating message, carried forward to all messages in a chain.
        /// </summary>
        DateTimeOffset CorrelationTimestamp { get; set; }

        /// <summary>
        /// The user ID of the originating message, carried forward to all messages in a chain.
        /// </summary>
        string UserId { get; set; }

        /// <summary>
        /// 
        /// </summary>
        IDictionary<string, string> Tags { get; set; }
    }
}

