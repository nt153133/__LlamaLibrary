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

The live capture confirms `XBMMonsterBookDetail`, differing from the upstream
`XBMMonsterNotebookDetail` symbol. The wrapper uses the observed runtime name.
The purpose of `XBMActivePet` relative to Battlehorn Settings remains unconfirmed.

## First live capture (2026-09-08)

Chris supplied a diagnostic capture with Cu Sith selected (attachment
`ce87cae4-6969-4a74-af30-612b9a2614d6/pasted-text.txt`). Both book addons report
agent 500. The detail addon exposes zero ATK values; `XBMMonsterNotebook` exposes
284, including the selected familiar's label/name at 229/230 and habitat text at
257. A candidate grid occupies values 24 through 223, with 25 repeated groups of
eight values and labels No. 1 through No. 25. These are observations of one state,
not validated public field mappings or creature IDs.

The capture contains no ContextMenu or active-pet addon. It cannot establish
the three horn assignments. A stored "Team Composition" label also does not prove
that the current UI is in Crucible mode; hidden controls can retain their labels.

Scalar union storage contains unrelated high bytes, for example UInt
`0x7FF600000001` represents UInt 1. Diagnostics now include the existing TwoInt
typed Bool/Int/UInt interpretation alongside raw storage so before/after comparison
does not mistake unused bytes for creature IDs or state changes.

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

Compile both target frameworks. The first capture validates runtime addon spelling
and the basic capture path. Runtime opening, the updated typed scalar display, and
all assignment behavior still require live validation; a successful build does not
prove those client behaviors. No CN/TC Beastmaster behavior is claimed.