using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.Enums;
using ff14bot.Managers;
using LlamaLibrary.Enums;
using LlamaLibrary.Extensions;
using Newtonsoft.Json;

namespace LlamaLibrary.Helpers
{
    //TODO Wtf did I make this class for? and why is it in Helpers
    /// <summary>
    /// A <see cref="Conditional"/> that filters bag slots by their <see cref="MyItemRole"/>,
    /// for example selecting only tank weapons or healer accessories.
    /// </summary>
    public class ItemRoleConditional : Conditional
    {
        /// <summary>
        /// Initialises a new <see cref="ItemRoleConditional"/> with an explicit name, action and parameter list.
        /// </summary>
        /// <param name="name">Display name for this conditional rule.</param>
        /// <param name="action">The action to perform on matched slots (Sell / Discard / Desynth).</param>
        /// <param name="parameters">List of <see cref="MyItemRole"/> names as strings.</param>
        public ItemRoleConditional(string name, ActionType action, List<string> parameters) : base(name, action, parameters)
        {
            type = ConditionalType.ItemRole;
        }

        /// <summary>
        /// Initialises an <see cref="ItemRoleConditional"/> by copying name, action and parameters from an existing <see cref="Conditional"/>.
        /// </summary>
        /// <param name="conditional">Source conditional to copy base fields from.</param>
        public ItemRoleConditional(Conditional conditional)
        {
            type = ConditionalType.ItemRole;
            Name = conditional.Name;
            Action = conditional.Action;
            Parameters = conditional.Parameters;
        }

        private List<MyItemRole> Roles
        {
            get
            {
                var temp = new List<MyItemRole>();

                foreach (var cat in Parameters)
                {
                    if (Enum.TryParse(cat, out MyItemRole enumOut))
                    {
                        temp.Add(enumOut);
                    }
                }

                return temp;
            }
        }

        /// <summary>
        /// Filters <paramref name="slots"/> to those whose item's <see cref="MyItemRole"/> is in the configured roles list.
        /// </summary>
        /// <param name="slots">Bag slots to evaluate.</param>
        /// <returns>Matching bag slots.</returns>
        public override IEnumerable<BagSlot> CheckCondition(IEnumerable<BagSlot> slots)
        {
            return slots.Where(i => Roles.Contains(i.Item.MyItemRole()));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Where(i=> _roles.Contains(i.MyItemRole()))";
        }
    }

    /// <summary>
    /// A <see cref="Conditional"/> that filters bag slots by their <see cref="ItemUiCategory"/>,
    /// for example selecting only "Gladiator's Arm" or "Medicine" items.
    /// </summary>
    public class ItemCategoryConditional : Conditional
    {
        /// <summary>
        /// Initialises a new <see cref="ItemCategoryConditional"/> with an explicit name, action and parameter list.
        /// </summary>
        /// <param name="name">Display name for this conditional rule.</param>
        /// <param name="action">The action to perform on matched slots.</param>
        /// <param name="parameters">List of <see cref="ItemUiCategory"/> names as strings.</param>
        public ItemCategoryConditional(string name, ActionType action, List<string> parameters) : base(name, action, parameters)
        {
            type = ConditionalType.ItemCategory;
        }

        /// <summary>
        /// Initialises an <see cref="ItemCategoryConditional"/> by copying fields from an existing <see cref="Conditional"/>.
        /// </summary>
        /// <param name="conditional">Source conditional to copy base fields from.</param>
        public ItemCategoryConditional(Conditional conditional)
        {
            type = ConditionalType.ItemCategory;
            Name = conditional.Name;
            Action = conditional.Action;
            Parameters = conditional.Parameters;
        }

        private List<ItemUiCategory> Categories
        {
            get
            {
                var temp = new List<ItemUiCategory>();

                foreach (var cat in Parameters)
                {
                    if (Enum.TryParse(cat, out ItemUiCategory enumOut))
                    {
                        temp.Add(enumOut);
                    }
                }

                return temp;
            }
        }

        /// <summary>
        /// Filters <paramref name="slots"/> to those whose item's <see cref="ItemUiCategory"/> is in the configured categories list.
        /// </summary>
        /// <param name="slots">Bag slots to evaluate.</param>
        /// <returns>Matching bag slots.</returns>
        public override IEnumerable<BagSlot> CheckCondition(IEnumerable<BagSlot> slots)
        {
            return slots.Where(i => Categories.Contains(i.Item.EquipmentCatagory));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Where(i=> _categories.Contains(i.EquipmentCatagory))";
        }
    }

    /// <summary>
    /// A <see cref="Conditional"/> that filters bag slots where the item name contains any of the configured substrings.
    /// </summary>
    public class NameConditional : Conditional
    {
        /// <summary>
        /// Initialises a new <see cref="NameConditional"/> with an explicit name, action and parameter list.
        /// </summary>
        /// <param name="name">Display name for this conditional rule.</param>
        /// <param name="action">The action to perform on matched slots.</param>
        /// <param name="parameters">List of substrings to match against item names.</param>
        public NameConditional(string name, ActionType action, List<string> parameters) : base(name, action, parameters)
        {
            type = ConditionalType.Name;
        }

        /// <summary>
        /// Initialises a <see cref="NameConditional"/> by copying fields from an existing <see cref="Conditional"/>.
        /// </summary>
        /// <param name="conditional">Source conditional to copy base fields from.</param>
        public NameConditional(Conditional conditional)
        {
            type = ConditionalType.Name;
            Name = conditional.Name;
            Action = conditional.Action;
            Parameters = conditional.Parameters;
        }

        private bool HasMatch(string inString)
        {
            return Parameters.Any(inString.Contains);
        }

        /// <summary>
        /// Filters <paramref name="slots"/> to those whose item name contains at least one of the configured substrings.
        /// </summary>
        /// <param name="slots">Bag slots to evaluate.</param>
        /// <returns>Matching bag slots.</returns>
        public override IEnumerable<BagSlot> CheckCondition(IEnumerable<BagSlot> slots)
        {
            return slots.Where(i => HasMatch(i.Name));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Where(i=> hasMatch(i.Name))";
        }
    }

    /// <summary>
    /// A <see cref="Conditional"/> that filters bag slots by matching raw item IDs.
    /// </summary>
    public class IdConditional : Conditional
    {
        /// <summary>
        /// Initialises a new <see cref="IdConditional"/> with an explicit name, action and parameter list.
        /// </summary>
        /// <param name="name">Display name for this conditional rule.</param>
        /// <param name="action">The action to perform on matched slots.</param>
        /// <param name="parameters">List of item IDs as decimal strings.</param>
        public IdConditional(string name, ActionType action, List<string> parameters) : base(name, action, parameters)
        {
            type = ConditionalType.Id;
        }

        /// <summary>
        /// Initialises an <see cref="IdConditional"/> by copying fields from an existing <see cref="Conditional"/>.
        /// </summary>
        /// <param name="conditional">Source conditional to copy base fields from.</param>
        public IdConditional(Conditional conditional)
        {
            type = ConditionalType.Id;
            Name = conditional.Name;
            Action = conditional.Action;
            Parameters = conditional.Parameters;
        }

        private List<uint> Ids => Parameters.Select(cat => (uint)int.Parse(cat)).ToList();

        /// <summary>
        /// Filters <paramref name="slots"/> to those whose <see cref="BagSlot.RawItemId"/> is in the configured ID list.
        /// </summary>
        /// <param name="slots">Bag slots to evaluate.</param>
        /// <returns>Matching bag slots.</returns>
        public override IEnumerable<BagSlot> CheckCondition(IEnumerable<BagSlot> slots)
        {
            return slots.Where(i => Ids.Contains(i.RawItemId));
        }

        /// <inheritdoc/>
        public override string ToString()
        {
            return "Where(i=> _ids.Contain(i.RawItemID))";
        }
    }

    /// <summary>
    /// Base class for all item-filter conditionals. Stores the rule name, action, conditional type,
    /// and the list of parameters used by subclasses to evaluate bag slots.
    /// Serialisable via <see cref="Newtonsoft.Json.JsonConstructorAttribute"/>.
    /// </summary>
    public class Conditional
    {
        /// <summary>The action to perform on slots matched by this conditional (Sell, Discard, or Desynth).</summary>
        public ActionType Action;

        /// <summary>Human-readable display name for this conditional rule.</summary>
        public string Name;

        /// <summary>The list of string parameters (role names, category names, item IDs, etc.) used by this conditional.</summary>
        public List<string> Parameters;

        /// <summary>The discriminator value used by the JSON deserialiser to reconstruct the correct subclass.</summary>
        protected ConditionalType type;

        /// <summary>
        /// Initialises a new <see cref="Conditional"/> with a name, type and parameter list.
        /// The action defaults to <see cref="ActionType.Sell"/>.
        /// </summary>
        /// <param name="name">Display name for this rule.</param>
        /// <param name="type">The type of condition check performed.</param>
        /// <param name="parameters">Condition parameter strings.</param>
        public Conditional(string name, ConditionalType type, List<string> parameters)
        {
            Name = name;
            this.type = type;
            Parameters = parameters;
            Action = ActionType.Sell;
        }

        /// <summary>
        /// Initialises a <see cref="Conditional"/> from JSON with all fields explicitly specified.
        /// </summary>
        /// <param name="name">Display name.</param>
        /// <param name="type">Condition type.</param>
        /// <param name="action">Action to apply.</param>
        /// <param name="parameters">Condition parameters.</param>
        [JsonConstructor]
        public Conditional(string name, ConditionalType type, ActionType action, List<string> parameters)
        {
            Name = name;
            this.type = type;
            Action = action;
            Parameters = parameters;
        }

        /// <summary>
        /// Protected constructor used by subclasses that supply their own type discriminator.
        /// </summary>
        /// <param name="name">Display name.</param>
        /// <param name="action">Action to apply.</param>
        /// <param name="parameters">Condition parameters.</param>
        protected Conditional(string name, ActionType action, List<string> parameters)
        {
            Name = name;
            Action = action;
            Parameters = parameters;
        }

        /// <summary>Default protected constructor for subclass use; fields are set by the caller.</summary>
        protected Conditional()
        {
        }

        /// <summary>
        /// When overridden in a subclass, filters <paramref name="slots"/> to those matching this conditional's criteria.
        /// The base implementation returns all slots unfiltered.
        /// </summary>
        /// <param name="slots">Bag slots to evaluate.</param>
        /// <returns>Filtered bag slots.</returns>
        public virtual IEnumerable<BagSlot> CheckCondition(IEnumerable<BagSlot> slots)
        {
            return slots;
        }
    }

    /// <summary>
    /// Identifies the type of matching logic used by a <see cref="Conditional"/>.
    /// </summary>
    public enum ConditionalType
    {
        /// <summary>Match by item role (tank/healer/DPS/DoH/DoL).</summary>
        ItemRole,
        /// <summary>Match by <see cref="ItemUiCategory"/>.</summary>
        ItemCategory,
        /// <summary>Match by item name substring.</summary>
        Name,
        /// <summary>Match by raw item ID.</summary>
        Id
    }

    /// <summary>
    /// Specifies what should happen to items that match a <see cref="Conditional"/> filter.
    /// </summary>
    public enum ActionType
    {
        /// <summary>Sell the item to an NPC vendor.</summary>
        Sell,
        /// <summary>Discard (trash) the item.</summary>
        Discard,
        /// <summary>Desynthesize the item.</summary>
        Desynth
    }
}