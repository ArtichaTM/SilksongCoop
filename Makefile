.PHONY: verify test install install-local loader teleports merge-main

# Where builds go: both games' hot-reloaded ScriptEngine folders.
SCRIPTS := $(HOME)/.local/share/Steam/steamapps/common/Hollow Knight Silksong/BepInEx/scripts
GOG_SCRIPTS := /mnt/nvme1/Temp/Games/Hollow Knight Silksong/BepInEx/scripts

# Runs every check locally (the steps live in scripts/Verify.cs), then installs into both games.
verify:
	@dotnet run scripts/Verify.cs
	@$(MAKE) --no-print-directory install-local

# Runs the pure-logic tests.
test:
	dotnet test --project tests/SilksongCoop.Tests -c Release

# Installs into both games; the post-commit hook publishes the plugin for the loader.
install: install-local

# Rebuilds the dev teleport table from the verdicts, audit and dump; keeps it when the dump is missing.
teleports:
	uv run docs/sync-audit/tools/teleports.py

# Builds the plugin and the FSM dump spike into both games' BepInEx/scripts.
install-local: teleports
	dotnet build src/SilksongCoop -c Release -t:Build,InstallPlugin "-p:PluginDir=$(SCRIPTS)/SilksongCoop"
	dotnet build src/SilksongCoop -c Release -t:InstallPlugin "-p:PluginDir=$(GOG_SCRIPTS)/SilksongCoop"
	dotnet build spikes/FsmDump -c Release -t:Build,InstallPlugin "-p:PluginDir=$(SCRIPTS)/FsmDump"
	dotnet build spikes/FsmDump -c Release -t:InstallPlugin "-p:PluginDir=$(GOG_SCRIPTS)/FsmDump"

# Builds the auto-updating loader that players put in BepInEx/plugins/SilksongCoop instead of the plugin.
loader:
	dotnet build src/SilksongCoop.Loader -c Release

# Fast-forwards main to dev, runs `make verify` on it and pushes; rolls back on failure.
merge-main:
	bash scripts/merge-main.sh
