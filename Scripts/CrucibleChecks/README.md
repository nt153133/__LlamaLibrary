# Crucible checks

Run `dotnet run --project Scripts/CrucibleChecks` with .NET 10. No game or RB
installation is required. The project links the production snapshot decoders
and uses a small host double to check visibility, stale roster rows, callback
guards and invalid progression data. It does not validate native signatures or
prove that the game accepts a callback.

Test sources use `.csx` so RB's recursive `.cs` loader does not compile the host
doubles into LlamaLibrary. Both SDK targets should also be built before submission.