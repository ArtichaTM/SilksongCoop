using System;
using System.IO;
using System.IO.Compression;
using System.Text;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// D39, G6, C1: the host sends its live save, the friend's game loads it from memory and never touches its
/// own save slots. The friend then walks in next to the host (D31) with its own loadout (D20, D28).
/// </summary>
internal sealed class JoinFlow(Coop coop) : Module(coop)
{
    // Out of range for the platform (0-4), so even a write that slipped past the guards fails.
    private const int NoSlot = -1;
    private const int ChunkSize = 1000;
    private const float LoadoutSaveInterval = 10f;

    private byte[][]? _chunks;
    private int _received;
    private bool _loading;
    private float _nextLoadoutSave;
    private string? _loadoutFile;
    private bool _inHostWorld;

    /// <summary>The host's save is loading on the friend's side; that scene change pulls nobody.</summary>
    public bool Loading => _loading;

    /// <summary>Set from the join request until the friend is back in the main menu.</summary>
    public bool PlayingHostSave { get; private set; }

    public override void Register(Session session)
    {
        if (!session.IsHost)
        {
            PlayingHostSave = true;
            string host = Coop.Config.HostAddress.Value.Trim();
            _loadoutFile = LoadoutStore.FileFor(host, Coop.Config.JoinPort.Value);
            session.On(MessageType.SaveChunk, OnChunk);
        }
    }

    public override void OnJoined()
    {
        if (Coop.IsHost)
        {
            SendSave();
        }
        else
        {
            Coop.Hints.Show("Connected, receiving the host's save...");
        }
    }

    public override void Update()
    {
        if (PlayingHostSave && Session is null && GameRefs.InMenu)
        {
            PlayingHostSave = false;
            _loading = false;
            _inHostWorld = false;
        }
        if (_inHostWorld && Session is not null && GameRefs.InMenu)
        {
            _inHostWorld = false;
            Coop.EndSession("partner quit to the menu");
        }
        if (_loading && GameRefs.GameplayScene is not null && GameRefs.HeroReady)
        {
            _loading = false;
            _inHostWorld = true;
            Coop.World.Begin();
            Coop.Pulls.JoinPartnerWhenFree();
        }
        if (!Coop.IsHost && Coop.World.Running && Time.unscaledTime >= _nextLoadoutSave)
        {
            _nextLoadoutSave = Time.unscaledTime + LoadoutSaveInterval;
            SaveLoadout();
        }
    }

    public override void OnLeft()
    {
        if (!Coop.IsHost && Coop.World.Running)
        {
            SaveLoadout();
        }
    }

    /// <summary>D31: the host quit or dropped; the friend goes back to the menu without saving.</summary>
    public void HostGone(string reason)
    {
        Coop.Hints.Show($"Back to menu: {reason}", 8f);
        _chunks = null;
        if (GameRefs.Manager is { } gm && !gm.IsMenuScene())
        {
            gm.ReturnToMainMenuNoSave();
        }
    }

    private void SaveLoadout()
    {
        if (_loadoutFile is null)
        {
            return;
        }
        try
        {
            LoadoutStore.Save(_loadoutFile, PlayerData.instance);
        }
        catch (IOException exception)
        {
            Coop.Log.LogWarning($"loadout not saved: {exception.Message}");
        }
    }

    private void SendSave()
    {
        GameManager? gm = GameRefs.Manager;
        if (gm is null)
        {
            return;
        }
        // Objects of the current room write their state into SceneData only when asked.
        gm.SaveLevelState();
        SaveGameData data = new(PlayerData.instance, SceneData.instance);
        byte[] packed = Compress(SaveDataUtility.SerializeSaveData(data));
        int count = (packed.Length + ChunkSize - 1) / ChunkSize;
        for (int index = 0; index < count; index++)
        {
            int start = index * ChunkSize;
            byte[] chunk = new byte[Math.Min(ChunkSize, packed.Length - start)];
            Array.Copy(packed, start, chunk, 0, chunk.Length);
            int at = index;
            Session?.Send(MessageType.SaveChunk, w => w.Int(at).Int(count).Bytes(chunk));
        }
        Coop.World.Begin();
        Coop.Log.LogInfo($"sent save: {packed.Length} bytes in {count} chunks");
    }

    private void OnChunk(PacketReader reader)
    {
        int index = reader.Int();
        int count = reader.Int();
        byte[] bytes = reader.Bytes();
        if (_chunks is null || _chunks.Length != count)
        {
            _chunks = new byte[count][];
            _received = 0;
        }
        if (_chunks[index] is null)
        {
            _chunks[index] = bytes;
            _received++;
        }
        if (_received == count)
        {
            LoadHostSave(_chunks);
            _chunks = null;
        }
    }

    private void LoadHostSave(byte[][] chunks)
    {
        using MemoryStream packed = new();
        foreach (byte[] chunk in chunks)
        {
            packed.Write(chunk, 0, chunk.Length);
        }
        SaveGameData data = SaveDataUtility.DeserializeSaveData<SaveGameData>(Decompress(packed.ToArray()));
        string? mismatch = null;
        bool restored = _loadoutFile is not null
            && LoadoutStore.Restore(_loadoutFile, data.playerData, out mismatch);
        if (mismatch is not null)
        {
            Coop.Log.LogWarning($"stored loadout reset, {mismatch} in the host's save");
            Coop.Hints.Show("Your loadout was reset: it doesn't match the host's save", 8f);
        }
        Coop.Log.LogInfo(restored ? "restored own loadout" : "using the host's loadout");
        _loading = true;
        GameRefs.Manager?.LoadGameFromUI(NoSlot, data);
    }

    private static byte[] Compress(string json)
    {
        using MemoryStream output = new();
        using (GZipStream zip = new(output, System.IO.Compression.CompressionLevel.Optimal))
        {
            byte[] raw = Encoding.UTF8.GetBytes(json);
            zip.Write(raw, 0, raw.Length);
        }
        return output.ToArray();
    }

    private static string Decompress(byte[] packed)
    {
        using GZipStream zip = new(new MemoryStream(packed), CompressionMode.Decompress);
        using StreamReader reader = new(zip, Encoding.UTF8);
        return reader.ReadToEnd();
    }
}
