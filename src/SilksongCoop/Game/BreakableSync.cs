using System.Collections.Generic;
using System.Linq;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Breakable props (bone piles, cairns, grave markers): most keep no save flag, so the game rebuilds them on
/// every room load and only the copies in one room need to match. A break in either game breaks the
/// partner's copy, loot included (items one-take, rosaries per player), and a player arriving in a room
/// gets the pieces already broken there, silently.
/// </summary>
internal sealed class BreakableSync : Module
{
    private const float ReindexCooldown = 1f;
    private const int IdsPerPacket = 64;

    private readonly HashSet<int> _broken = [];
    private readonly Dictionary<int, Breakable> _pieces = [];
    private float _lastIndex = -ReindexCooldown;
    private bool _applying;

    public BreakableSync(Coop coop)
        : base(coop) => coop.Rooms.Entered += OnEntered;

    public override void Register(Session session) => session.On(MessageType.BreakablesBroken, OnBroken);

    public override void Dispose() => Coop.Rooms.Entered -= OnEntered;

    /// <summary>The piece's id before it breaks: some detach from their parent as they do.</summary>
    public int? IdBeforeBreak(Breakable piece)
    {
        bool ours = !_applying && !piece.IsBroken && Coop.Rooms.Holds(piece.gameObject.scene);
        return ours ? EnemySync.IdOf(piece.gameObject) : null;
    }

    public void OnLocalBreak(Breakable piece, int id)
    {
        if (!piece.IsBroken || !_broken.Add(id))
        {
            return;
        }
        if (Coop.Rooms.PartnerHere)
        {
            Send(true, [id]);
        }
    }

    /// <summary>Called by the game that was in the room first when the partner arrives.</summary>
    public void SendSnapshot()
    {
        List<int> broken = _broken.ToList();
        for (int start = 0; start < broken.Count; start += IdsPerPacket)
        {
            Send(false, broken.GetRange(start, Mathf.Min(IdsPerPacket, broken.Count - start)));
        }
    }

    private void Send(bool live, List<int> ids)
    {
        int room = Coop.Rooms.CurrentHash;
        Session?.Send(MessageType.BreakablesBroken, w =>
        {
            w.Int(room).Bool(live).Int(ids.Count);
            ids.ForEach(id => w.Int(id));
        });
    }

    private void OnEntered(string scene)
    {
        _broken.Clear();
        _pieces.Clear();
        _lastIndex = -ReindexCooldown;
    }

    private void OnBroken(PacketReader reader)
    {
        bool forThisRoom = reader.Int() == Coop.Rooms.CurrentHash;
        bool live = reader.Bool();
        int count = reader.Int();
        for (int i = 0; i < count; i++)
        {
            int id = reader.Int();
            if (forThisRoom && _broken.Add(id) && Find(id) is { IsBroken: false } piece)
            {
                Break(piece, live);
            }
        }
    }

    /// <summary>A live break plays out with debris and loot; an earlier one is just shown broken.</summary>
    private void Break(Breakable piece, bool live)
    {
        _applying = true;
        try
        {
            if (live)
            {
                piece.BreakSelf();
            }
            else
            {
                piece.isBroken = true;
                piece.SetAlreadyBroken();
            }
        }
        finally
        {
            _applying = false;
        }
    }

    private Breakable? Find(int id)
    {
        if (!_pieces.TryGetValue(id, out Breakable? piece) || !piece)
        {
            Reindex();
            _pieces.TryGetValue(id, out piece);
        }
        return piece ? piece : null;
    }

    private void Reindex()
    {
        if (Time.unscaledTime - _lastIndex < ReindexCooldown || Coop.Rooms.Current is null)
        {
            return;
        }
        _lastIndex = Time.unscaledTime;
        foreach (Breakable piece in Object.FindObjectsByType<Breakable>(FindObjectsSortMode.None))
        {
            if (!piece.IsBroken && Coop.Rooms.Holds(piece.gameObject.scene))
            {
                _pieces[EnemySync.IdOf(piece.gameObject)] = piece;
            }
        }
    }
}
