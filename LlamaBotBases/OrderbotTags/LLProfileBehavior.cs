using System;
using System.Linq;
using System.Windows.Media;
using Clio.XmlEngine;
using ff14bot.NeoProfiles;
using LlamaLibrary.Logging;

namespace LlamaBotBases.OrderbotTags
{
    /// <summary>
    /// Adds extra functionality common to all OrderBot tags.
    /// </summary>
    public abstract class LLProfileBehavior : ProfileBehavior
    {
        /// <summary>
        /// <see cref="LLogger"/> instance.
        /// </summary>
        protected new readonly LLogger Log;

        protected virtual string Name => ((XmlElementAttribute)Attribute.GetCustomAttributes(GetType(), typeof(XmlElementAttribute)).FirstOrDefault()).Name;
        protected virtual Color LogColor => Colors.White;

        /// <summary>
        /// Initializes a new instance of the <see cref="LLProfileBehavior"/> class.
        /// </summary>
        /// <param name="log"><see cref="LLogger"/> used by this OrderBot tag.</param>
        public LLProfileBehavior()
        {
            Log = new LLogger(Name, LogColor);
        }
    }
}