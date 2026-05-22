# __LlamaLibrary

LlamaLibrary is a robust support library for RebornBuddy botbases, plugins, combat routines, and quest behaviors for Final Fantasy XIV.

It provides functionality that RebornBuddy does not expose directly, including helper functions, template botbases and plugins, offset and client-function access, remote window and agent wrappers, retainer helpers, script conditions, and shared game data/resources.

## Runtime vs Development

For users, LlamaLibrary is installed into the RebornBuddy folder at:

```text
RebornBuddy
+-- QuestBehaviors
    +-- __LlamaLibrary
```

That installed `QuestBehaviors\__LlamaLibrary` copy is the runtime source of truth. When a RebornBuddy project runs, it uses the user's locally installed LlamaLibrary folder.

The [LlamaLibrary NuGet package](https://www.nuget.org/packages/LlamaLibrary) is for developers. It lets RB-based projects reference LL while coding and building, but the NuGet package is not shipped as the runtime copy with the consuming project.

## Updates

UpdateBuddy is now part of RebornBuddy and does not need to be installed separately. No separate UpdateBuddy plugin download or `Plugins` folder setup is required for LlamaLibrary.

UpdateBuddy keeps LlamaLibrary current in RebornBuddy's `QuestBehaviors\__LlamaLibrary` folder:

```text
RebornBuddy
+-- QuestBehaviors
    +-- __LlamaLibrary
```

### Manual Setup

If you do not want to use updateBuddy:

1. Remove any previous versions of LlamaLibrary from the `BotBases` folder.
2. Download or clone [__LlamaLibrary](https://github.com/nt153133/__LlamaLibrary).
3. Create `RebornBuddy\QuestBehaviors\__LlamaLibrary`.
4. Place the contents of this repository in that `__LlamaLibrary` folder.
5. Download or clone [LlamaUtilities](https://github.com/nt153133/LlamaUtilities).
6. Create `RebornBuddy\BotBases\LlamaUtilities`.
7. Place the contents of LlamaUtilities in that folder.

Optional:

1. Download or clone [ExtraBotbases](https://github.com/nt153133/ExtraBotbases).
2. Create `RebornBuddy\BotBases\ExtraBotbases`.
3. Place the contents of ExtraBotbases in that folder.

After installation, your folder structure should look similar to:

## Developer Reference

When developing a RebornBuddy-based project, reference the NuGet package:

```text
LlamaLibrary
```

NuGet package: [LlamaLibrary](https://www.nuget.org/packages/LlamaLibrary)

Use the package for compile-time references and IDE support. Do not assume the version referenced by a developer project is what users will run. Runtime behavior comes from the user's installed `QuestBehaviors\__LlamaLibrary` folder.

## Contents

For a description of what is included with LlamaLibrary, check the [wiki](https://github.com/nt153133/__LlamaLibrary/wiki).
