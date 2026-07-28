using System;
using ff14bot;
using LlamaLibrary.Memory;
using LlamaLibrary.Utilities;

namespace LlamaLibrary.RemoteWindows
{
    /// <summary>
    /// Interaction interface for exchanging gil for MGP.
    /// </summary>
    public class ShopExchangeCoin : RemoteWindow<ShopExchangeCoin>
    {
        private const int UldManagerOffset = 0x28;
        private const int NodeListCountOffset = 0x42;
        private const int NodeListOffset = 0x50;
        private const int NodeTypeOffset = 0x40;
        private const int ComponentOffset = 0xC0;
        private const ushort ComponentNodeTypeStart = 1000;
        private const int NumericInputComponentType = 8;

        public const int MinimumAmount = 1;
        public const int MaximumAmount = 500;

        /// <summary>
        /// Initializes a new instance of the <see cref="ShopExchangeCoin"/> class.
        /// </summary>
        public ShopExchangeCoin() : base("ShopExchangeCoin")
        {
        }

        /// <summary>
        /// Sets the requested MGP amount and refreshes the required gil.
        /// </summary>
        /// <param name="amount">The amount to request, clamped to the supported range of 1 through 500.</param>
        public void SetAmount(int amount)
        {
            var clampedAmount = ClampAmount(amount);
            var numericInput = FindNumericInputComponent();
            if (numericInput == IntPtr.Zero)
            {
                throw new InvalidOperationException("Could not find the ShopExchangeCoin numeric input component.");
            }

            Core.Memory.CallInjectedWraper<IntPtr>(
                ShopExchangeCoinOffsets.NumericInputSetValue,
                numericInput,
                clampedAmount);
            SendAction(2, 3, 3, 3, (ulong)clampedAmount);
        }

        /// <summary>
        /// Proceeds with the exchange for the requested MGP amount.
        /// </summary>
        /// <param name="amount">The amount to exchange, clamped to the supported range of 1 through 500.</param>
        public void Proceed(int amount)
        {
            SendAction(2, 3, 0, 3, (ulong)ClampAmount(amount));
        }

        /// <summary>
        /// Cancels the exchange.
        /// </summary>
        public void Cancel()
        {
            Close();
        }

        /// <inheritdoc/>
        public override void Close()
        {
            if (IsOpen)
            {
                SendAction(1, 3, ulong.MaxValue);
            }
        }

        private static int ClampAmount(int amount)
        {
            return Math.Max(MinimumAmount, Math.Min(MaximumAmount, amount));
        }

        private IntPtr FindNumericInputComponent()
        {
            var window = WindowByName;
            if (window == null || window.Pointer == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            var uldManager = window.Pointer + UldManagerOffset;
            var nodeCount = Core.Memory.Read<ushort>(uldManager + NodeListCountOffset);
            var nodeList = Core.Memory.Read<IntPtr>(uldManager + NodeListOffset);
            if (nodeList == IntPtr.Zero)
            {
                return IntPtr.Zero;
            }

            for (var index = 0; index < nodeCount; index++)
            {
                var node = Core.Memory.Read<IntPtr>(nodeList + (index * IntPtr.Size));
                if (node == IntPtr.Zero ||
                    Core.Memory.Read<ushort>(node + NodeTypeOffset) < ComponentNodeTypeStart)
                {
                    continue;
                }

                var component = Core.Memory.Read<IntPtr>(node + ComponentOffset);
                if (component == IntPtr.Zero)
                {
                    continue;
                }

                var componentType = Core.Memory.CallInjectedWraper<int>(
                    ShopExchangeCoinOffsets.ComponentGetType,
                    component);
                if (componentType == NumericInputComponentType)
                {
                    return component;
                }
            }

            return IntPtr.Zero;
        }
    }
}