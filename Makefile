.PHONY: verify test install dev-install

# Runs every check locally; the steps live in scripts/Verify.cs.
verify:
	@dotnet run scripts/Verify.cs

# Runs the pure-logic tests.
test:
	dotnet test --project tests/SilksongCoop.Tests -c Release

# Builds the plugin and copies it into the game's BepInEx/plugins/SilksongCoop.
install:
	dotnet build src/SilksongCoop -c Release -t:Build,InstallPlugin

# Builds the plugin and the FSM dump spike into BepInEx/scripts; ScriptEngine reloads them live.
dev-install:
	dotnet build src/SilksongCoop -c Release -t:Build,InstallPlugin -p:DevInstall=true
	dotnet build spikes/FsmDump -c Release -t:Build,InstallPlugin -p:DevInstall=true
