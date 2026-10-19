using SilksongCoop.Net;

namespace SilksongCoop.Game;

/// <summary>
/// D29: by default the world keeps running while one player sits in the pause menu; with
/// <c>PauseFreezesBoth</c> a pause on either side stops both games.
/// </summary>
internal sealed class PauseSync(Coop coop) : Module(coop)
{
    private bool _paused;
    private bool _partnerPaused;

    private bool FreezeBoth => Coop.Config.PauseFreezesBoth.Value;

    public override void Register(Session session) => session.On(MessageType.Pause, OnPartnerPause);

    /// <summary>Called after the game paused or unpaused itself.</summary>
    public void OnLocalPause(bool paused)
    {
        _paused = paused;
        if (!Coop.InCoop || GameRefs.Manager is not { } gm)
        {
            return;
        }
        Session?.Send(MessageType.Pause, w => w.Bool(paused));
        if (paused && !FreezeBoth)
        {
            gm.SetTimeScale(1f);
        }
        else if (!paused && FreezeBoth && _partnerPaused)
        {
            Freeze(gm, true);
        }
    }

    public override void OnLeft()
    {
        if (_partnerPaused && !_paused && FreezeBoth && GameRefs.Manager is { } gm)
        {
            Freeze(gm, false);
        }
        _partnerPaused = false;
    }

    private void OnPartnerPause(PacketReader reader)
    {
        _partnerPaused = reader.Bool();
        if (FreezeBoth && !_paused && GameRefs.Manager is { } gm)
        {
            Freeze(gm, _partnerPaused);
        }
    }

    /// <summary>The game's own pause minus its menu: time stops and actors and music go quiet.</summary>
    private static void Freeze(GameManager gm, bool frozen)
    {
        gm.SetTimeScale(frozen ? 0f : 1f);
        if (frozen)
        {
            gm.actorSnapshotPaused.TransitionToSafe(0f);
            gm.ui.AudioGoToPauseMenu(0.2f);
        }
        else
        {
            gm.actorSnapshotUnpaused.TransitionToSafe(0f);
            gm.ui.AudioGoToGameplay(0.2f);
        }
    }
}
