.PHONY: verify test install

# Runs every check locally; the steps live in scripts/Verify.cs.
verify:
	@dotnet run scripts/Verify.cs

# Runs the pure-logic tests.
test:
	dotnet test --project tests/SilksongCoop.Tests -c Release

# Builds the plugin and copies it into the game's BepInEx/plugins/SilksongCoop.
install:
	dotnet build src/SilksongCoop -c Release -t:Build,InstallPlugin
