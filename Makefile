.PHONY: verify test install install-dev loader teleports merge-main

# Game root folders to install into, separated by `;`; set in the untracked local.mk (see local.mk.example).
-include local.mk
export SILKSONG_GAMES

# Runs every check locally (the steps live in scripts/Verify.cs), then installs when a game is set.
verify:
	@dotnet run scripts/Verify.cs
	@if [ -n "$$SILKSONG_GAMES" ]; then $(MAKE) --no-print-directory install-dev; \
	else echo "SILKSONG_GAMES is not set, install skipped"; fi

# Runs the pure-logic tests.
test:
	dotnet test --project tests/SilksongCoop.Tests -c Release

# Rebuilds the dev teleport table from the verdicts, audit and dump; keeps it when the dump is missing.
teleports:
	uv run docs/sync-audit/tools/teleports.py

# Builds the plugin into every game's hot-reloaded BepInEx/scripts.
install: teleports
	dotnet build src/SilksongCoop -c Release -t:Build,InstallPlugin

# Also installs the dev tools: the FSM dump spike.
install-dev: install
	dotnet build spikes/FsmDump -c Release -t:Build,InstallPlugin

# Builds the auto-updating loader that players put in BepInEx/plugins/SilksongCoop instead of the plugin.
loader:
	dotnet build src/SilksongCoop.Loader -c Release

# Fast-forwards main to dev, runs `make verify` on it and pushes; rolls back on failure.
merge-main:
	bash scripts/merge-main.sh
