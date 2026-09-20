using System;
using System.Collections.Generic;
using System.Linq;
using ff14bot.RemoteWindows;
using LlamaLibrary.Helpers;
using LlamaLibrary.RemoteWindows;
using LlamaLibrary.Structs;

internal static class Program
{
    private static int passed;

    private static void Main()
    {
        // A partial read must not make a caller drop trainees or treat missing pets as capped.
        var bytes = new byte[100];
        bytes[0] = 25;
        bytes[49] = 19;
        bytes[99] = 30;
        Check(BeastmasterProgressDecoder.TryDecode(bytes, new uint[] { 1, 50 }, out var progress, out _), "valid owned collection");
        Check(progress![1].IsCapped && progress[1].Experience == 0, "rank cap with zero EXP");
        Check(progress[50].Rank == 19 && progress[50].Experience == 30, "last species indexing");
        bytes[49] = 20;
        Check(progress[50].Rank == 19, "snapshot detached from input buffer");
        Reject(null, new uint[] { 1 }, "absent read");
        Reject(new byte[99], new uint[] { 1 }, "short read");
        Reject(new byte[101], new uint[] { 1 }, "unexpected layout");
        Reject(bytes, null, "absent ownership");
        Reject(bytes, Array.Empty<uint>(), "empty ownership");
        Reject(bytes, new uint[] { 1, 1 }, "duplicate ownership");
        Reject(bytes, new uint[] { 0 }, "zero species");
        Reject(bytes, new uint[] { 51 }, "out-of-range species");
        Reject(bytes, new uint[] { 1, 2 }, "uninitialized owned rank");
        bytes[0] = 26;
        Reject(bytes, new uint[] { 1 }, "rank above cap");
        bytes[0] = 24;
        bytes[50] = 101;
        Reject(bytes, new uint[] { 1 }, "EXP above maximum");
        bytes[50] = 100;
        Check(BeastmasterProgressDecoder.TryDecode(bytes, new uint[] { 1 }, out progress, out _) && !progress![1].IsCapped, "full EXP does not imply rank cap");
        Throws<NotSupportedException>(() => ((IDictionary<uint, BeastmasterProgress>)progress!).Clear(), "collection is read-only");

        var board = new XBMStageList();
        Check(!board.IsOpen && !board.ReadSnapshot().IsOpen && !board.SelectBoard(1), "closed board list");
        var boardWindow = Open("XBMStageList", 10);
        boardWindow.Values[1].UInt = 4;
        boardWindow.Values[2] = Text("First Board of the Unbroken");
        boardWindow.Values[3].UInt = 1;
        Check(board.ReadSnapshot().Boards[0].Id == 1, "board ID is distinct from row index");
        Check(board.SelectBoard(1) && boardWindow.LastAction!.SequenceEqual(new ulong[] { 3, 1, 3, 1 }), "board callback uses Int board ID");
        Throws<ArgumentOutOfRangeException>(() => board.SelectBoard(0), "invalid board ID");
        boardWindow.IsVisible = false;
        boardWindow.LastAction = null;
        Check(!board.IsOpen && !board.ReadSnapshot().IsOpen && !board.SelectBoard(1) && boardWindow.LastAction == null, "hidden addon is not actionable");
        boardWindow.IsVisible = true;
        boardWindow.Values[1].UInt = 5;
        Throws<InvalidOperationException>(() => board.ReadSnapshot(), "truncated board rows");

        // Removed party rows retain data in the client; only the live count is valid.
        var partyWindow = Open("XBMPetParty", 1188);
        partyWindow.Values[5].UInt = 1;
        partyWindow.Values[82].UInt = 1;
        partyWindow.Values[159].UInt = 50;
        partyWindow.Values[9] = Text("Cu Sith");
        var party = new XBMPetParty();
        var roster = party.ReadSnapshot();
        Check(roster.Pets.Length == 1 && roster.Pets[0].Id == 1, "stale roster tail ignored");
        partyWindow.Values[9] = Text("Changed");
        Check(roster.Pets[0].Name == "Cu Sith", "roster text detached");
        partyWindow.Values[5].UInt = 16;
        Throws<InvalidOperationException>(() => party.ReadSnapshot(), "invalid roster count");
        Throws<ArgumentOutOfRangeException>(() => party.ToggleRow(15), "invalid roster action row");
        Open("XBMPetParty", 1187);
        Throws<InvalidOperationException>(() => party.ReadSnapshot(), "changed roster layout");

        // A missing or changed menu label must never dispatch the old selection.
        var contextWindow = Open("ContextMenu", 9);
        contextWindow.Values[8] = Text("Use");
        var context = new DialogWindow("ContextMenu");
        Check(!context.SelectFirstContextEntry("Remove from Team"), "wrong context rejected");
        Check(context.SelectFirstContextEntry("Use"), "matching context accepted");
        contextWindow.Values[8] = default;
        Check(!context.SelectFirstContextEntry(string.Empty), "missing label rejected");
        contextWindow.Values[8] = Text("Use");
        contextWindow.IsVisible = false;
        Check(!context.IsOpen && !context.SelectFirstContextEntry("Use"), "hidden context rejected");

        Check(!new XBMContentsBooty.Snapshot(Array.Empty<TwoInt>()).IsOpen, "closed spoils");
        Check(!new XBMContentsTreasure.Snapshot(Array.Empty<TwoInt>()).IsOpen, "closed treasure");
        Check(!new XBMContentsItemShop.Snapshot(Array.Empty<TwoInt>()).IsOpen, "closed shop");
        Check(!new XBMContentsMainHUD.Snapshot(Array.Empty<TwoInt>()).IsOpen, "closed HUD");
        Check(!new XBMMonsterNotebook.Snapshot(Array.Empty<TwoInt>()).IsOpen, "closed notebook");
        Check(!new XBMStageDetailList.Snapshot(Array.Empty<TwoInt>()).IsOpen, "closed details");
        Check(!new XBMResult.Snapshot(Array.Empty<TwoInt>()).IsOpen, "closed results");
        var shop = new TwoInt[269];
        shop[2].UInt = 31;
        Throws<InvalidOperationException>(() => new XBMContentsItemShop.Snapshot(shop), "shop count overflow");
        var details = new TwoInt[40058];
        details[5].UInt = 1001;
        Throws<InvalidOperationException>(() => new XBMStageDetailList.Snapshot(details), "detail count overflow");
        Throws<ArgumentOutOfRangeException>(() => new XBMMonsterNotebook().ToggleTeamEntry(25), "notebook cell overflow");
        Console.WriteLine($"{passed} Crucible checks passed.");
    }

    private static AtkAddonControl Open(string name, int count)
    {
        var window = new AtkAddonControl { Values = new TwoInt[count] };
        HostWindows.Windows[name] = window;
        return window;
    }

    private static TwoInt Text(string value) => new TwoInt { AtkValueType = 8, AsUTF8String = value };

    private static void Reject(byte[]? bytes, uint[]? owned, string name)
    {
        Check(!BeastmasterProgressDecoder.TryDecode(bytes, owned, out var progress, out var reason) && progress == null && reason.Length != 0, name);
    }

    private static void Check(bool condition, string name)
    {
        if (!condition) throw new Exception(name);
        passed++;
    }

    private static void Throws<T>(Action action, string name) where T : Exception
    {
        try { action(); }
        catch (T) { passed++; return; }
        throw new Exception(name);
    }
}