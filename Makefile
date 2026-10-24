.PHONY: verify test install loader

# Where `make install` puts builds: Steam's hot-reloaded ScriptEngine folder and the site the loader reads.
SCRIPTS := $(HOME)/.local/share/Steam/steamapps/common/Hollow Knight Silksong/BepInEx/scripts
SITE_DLL := raspi:/home/site/articha_site/static/shared/SilksongCoop.dll
# The GOG copy plays the friend, so it gets SilksongCoop from the site; FsmDump is its only debug extra.
GOG_PLUGINS := /mnt/nvme1/Temp/Games/Hollow Knight Silksong/BepInEx/plugins

# Runs every check locally; the steps live in scripts/Verify.cs.
verify:
	@dotnet run scripts/Verify.cs

# Runs the pure-logic tests.
test:
	dotnet test --project tests/SilksongCoop.Tests -c Release

# Builds the plugin and the FSM dump spike into Steam's BepInEx/scripts and FsmDump into GOG's plugins,
# then publishes the plugin.
install:
	dotnet build src/SilksongCoop -c Release -t:Build,InstallPlugin "-p:PluginDir=$(SCRIPTS)/SilksongCoop"
	dotnet build spikes/FsmDump -c Release -t:Build,InstallPlugin "-p:PluginDir=$(SCRIPTS)/FsmDump"
	dotnet build spikes/FsmDump -c Release -t:InstallPlugin "-p:PluginDir=$(GOG_PLUGINS)/FsmDump"
	scp src/SilksongCoop/bin/Release/netstandard2.1/SilksongCoop.dll $(SITE_DLL)

# Builds the auto-updating loader that players put in BepInEx/plugins/SilksongCoop instead of the plugin.
loader:
	dotnet build src/SilksongCoop.Loader -c Release
