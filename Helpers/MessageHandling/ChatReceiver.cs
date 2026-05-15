using System;
using System.Collections.Generic;
using ff14bot.Enums;

namespace LlamaLibrary.Helpers.MessageHandling
{
    /// <summary>
    /// Abstract base class for components that listen to specific FFXIV chat message types.
    /// Subclasses implement <see cref="ParseMessage"/> to convert the raw string into a
    /// strongly-typed value and <see cref="ProcessMessage(T)"/> to act on it.
    /// </summary>
    /// <typeparam name="T">The type of parsed message this receiver produces and handles.</typeparam>
    public abstract class ChatReceiver<T> : IChatReceiver
    {
        /// <summary>
        /// Gets the collection of <see cref="MessageType"/> values this receiver is interested in.
        /// Only chat entries whose <see cref="MessageType"/> is present in this collection will be
        /// dispatched to <see cref="ProcessMessage(string, DateTime)"/>.
        /// </summary>
        public virtual IEnumerable<MessageType> TypesToReceive { get; } = new List<MessageType>();

        /// <summary>
        /// Parses the raw chat message string into a strongly-typed <typeparamref name="T"/> value.
        /// Override this method to provide custom parsing logic.
        /// </summary>
        /// <param name="message">The raw chat message text.</param>
        /// <param name="timeStamp">The timestamp at which the message was received.</param>
        /// <returns>The parsed message value of type <typeparamref name="T"/>.</returns>
        public virtual T ParseMessage(string message, DateTime timeStamp)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Handles a parsed message of type <typeparamref name="T"/>.
        /// Override this method to react to received and parsed chat messages.
        /// </summary>
        /// <param name="message">The parsed message to handle.</param>
        public virtual void ProcessMessage(T message)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Called by <see cref="ChatHandler"/> when a matching chat message is received.
        /// Parses the raw string via <see cref="ParseMessage"/> and forwards the result to
        /// <see cref="ProcessMessage(T)"/>.
        /// </summary>
        /// <param name="message">The raw chat message text.</param>
        /// <param name="timeStamp">The timestamp at which the message was received.</param>
        public void ProcessMessage(string message, DateTime timeStamp)
        {
            ProcessMessage(ParseMessage(message, timeStamp));
        }
    }

    /// <summary>
    /// Defines the contract for a chat message receiver that can be registered with <see cref="ChatHandler"/>.
    /// </summary>
    public interface IChatReceiver
    {
        /// <summary>
        /// Gets the message types that this receiver wants to be notified about.
        /// </summary>
        IEnumerable<MessageType> TypesToReceive { get; }

        /// <summary>
        /// Called by <see cref="ChatHandler"/> when an incoming chat entry matches one of the
        /// types in <see cref="TypesToReceive"/>.
        /// </summary>
        /// <param name="message">The raw chat message text.</param>
        /// <param name="timeStamp">The timestamp at which the message was received.</param>
        void ProcessMessage(string message, DateTime timeStamp);
    }
}