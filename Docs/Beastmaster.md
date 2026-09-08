# Beastmaster battlehorn integration

This foundation gives callers named windows and a read-only evidence capture while
the assignment protocol is being mapped. It does **not** assign a familiar yet.
Keeping discovery separate prevents an unverified callback from modifying the wrong
horn or confusing ordinary assignments with Crucible team composition.

## Sources and migration boundary

Checked FFXIVClientStructs commit `bb007e6fcab551dfdb53ad777f4324a9246c9a01` on
2026-09-08. Its `ida/data.yml` identifies client build `2026.09.01.0000.0000`.

- [Native symbol database](https://github.com/aers/FFXIVClientStructs/blob/bb007e6fcab551dfdb53ad777f4324a9246c9a01/ida/data.yml):
  notebook, notebook detail, active-pet, and pet-party addon constructors/vtables.
- [Agent IDs](https://github.com/aers/FFXIVClientStructs/blob/bb007e6fcab551dfdb53ad777f4324a9246c9a01/FFXIVClientStructs/FFXIV/Client/UI/Agent/AgentModule.cs):
  notebook 500 and pet-party 501 are research leads, not portable registrations.
- `XBMModule` and `XBMNoteModule` only expose their inherited user-file event and
  instance accessors. They have no mapped assignment fields or methods to migrate.
- [Official 7.56 notes](https://eu.finalfantasyxiv.com/lodestone/topics/detail/d979aeaccd188ce59ea4eef8c51c26037fd5b06a):
  `/bestiary` toggles the book; its subcommand assigns familiars to battlehorns.

The wrappers reuse `RemoteWindow<T>`, its `TwoInt` reader, existing ATK offsets,
and normal timeout behavior. No native layouts, signatures, absolute addresses,
or fixed agent registrations were introduced. `XBMMonsterNotebook.Open()` requires
an RB coroutine when closed and uses `/bestiary`; the other wrappers' `Open()`
methods only report existing visibility because independent opening is not mapped.
Inherited `SendAction` remains a low-level API, not verified Beastmaster support.

The supplied name `XBMMonsterBookDetail` differs from the upstream symbol
`XBMMonsterNotebookDetail`. Capture discovers all visible XBM names rather than
silently treating those names as equivalent. The purpose of `XBMActivePet` relative
to the Battlehorn Settings screen also remains to be confirmed.

## Live evidence required

With a current Global 7.56 client attached to RB, stop the bot and open the bestiary.
Run this complete statement in RebornConsole after loading this library build:

```csharp
Log(LlamaLibrary.Helpers.BeastmasterDiagnostics.Capture());
```

Capture each of these states and retain which familiar/horn was selected:

1. A captured familiar selected in the book.
2. Its assignment context menu open, including disabled horn choices.
3. Battlehorn Settings before and after manually assigning that familiar.
4. A second familiar assigned to the same horn, and assignment to each unlocked horn.

Also record the character's Beastmaster level and which horns are unlocked. These
snapshots establish candidate value fields and actual addon/agent names, but **do not
capture callbacks**. A UI callback trace or disassembly of the notebook/context-menu
handler is still required to establish the callback value types, argument order,
creature identifier (row ID versus display index), and horn indexing convention.
Do not guess that an enabled menu row index is a fixed horn index.

Before exposing an assignment API, verify success by reading the resulting slot,
confirm behavior for uncaptured creatures and unavailable horns, and determine the
client's combat/reassignment restrictions. If native signatures become necessary,
derive operand wildcards from Ghidra instruction semantics and verify unique matches
and extraction results on every available supported regional/prior build. The
upstream addresses above are only navigation hints, not runtime offsets.

## Validation boundary

Compile both target frameworks. Runtime opening, addon spelling, typed value
capture, and all assignment behavior require live validation; a successful build
does not prove those client behaviors. No CN/TC Beastmaster behavior is claimed.