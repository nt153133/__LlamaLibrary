# __LlamaLibrary
A robust library of functions for RebornBuddy botbases/plugins for FFXIV

## Installation

### Automatic Setup

The easiest way to install LlamaLibrary is to install the [updateBuddy](https://loader.updatebuddy.net/UpdateBuddy.zip) plugin. It would be installed in the **/plugins** folder of your RebornBuddy folder as such:
```
RebornBuddy
└── Plugins
    └── updateBuddy
        ├── git2-a2bde63.dll
        ├── LibGit2Sharp.dll
        ├── Loader.cs
        └── UpdateBuddy.dll
```

It will automatically install the files into the correct folders and keep them up to date.

### Manual Setup

For those of you that don't want to use [updateBuddy](https://loader.updatebuddy.net/UpdateBuddy.zip) here's the manual installation method.

First off, make sure you remove any previous versions of LlamaLibrary you may have in the **/BotBases** folder.

Download the zip from [__LlamaLibrary](https://github.com/nt153133/__LlamaLibrary) and create a folder in **/QuestBehaviors** called **__LlamaLibrary**(Two underscores) and either unzip the contents of the zip into that folder, or check out using a SVN client to that folder.

Download the zip from [LlamaUtilities](https://github.com/nt153133/LlamaUtilities) and create a folder in **/BotBases** called **LlamaUtilities** and either unzip the contents of the zip into that folder, or check out using a SVN client to that folder.

(Optional)
Download the zip from [ExtraBotbases](https://github.com/nt153133/ExtraBotbases) and create a folder in **/BotBases** called **ExtraBotbases** and either unzip the contents of the zip into that folder, or check out using a SVN client to that folder. These are a few extra botbases that most users will not use.

After Install your folder structure should look something like this.
```
RebornBuddy
|── BotBases
|    └── LlamaUtilities
|    └── ExtraBotbases*
└── QuestBehaviors
     └── __LlamaLibrary
```

## Contents
For a description of what's all included with LlamaLibrary, check out the [wiki](https://github.com/nt153133/__LlamaLibrary/wiki)!
