namespace LlamaLibrary.Structs
{
    /// <summary>Permanent familiar progression, independent of a duty's synchronized rank.</summary>
    public sealed class BeastmasterProgress
    {
        /// <summary>Permanent rank cap for the validated 50-species collection.</summary>
        public const int MaximumRank = 25;

        /// <summary>Permanent rank, from one through MaximumRank.</summary>
        public int Rank { get; }

        /// <summary>Raw progress toward the next rank, from zero through 100.</summary>
        public int Experience { get; }

        /// <summary>Whether the familiar is finished; capped EXP may be zero.</summary>
        public bool IsCapped => Rank == MaximumRank;

        internal BeastmasterProgress(int rank, int experience)
        {
            Rank = rank;
            Experience = experience;
        }
    }
}