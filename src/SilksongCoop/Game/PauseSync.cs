using System;
using GlobalEnums;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

internal enum PauseMode
{
    Off,
    Freeze,
    Menu,
}

/// <summary>
/// D29: with <c>Off</c> the world keeps running while one player sits in the pause menu; with <c>Freeze</c> a
/// pause on either side stops both games; with <c>Menu</c> Esc opens and closes the pause menu for both.
/// </summary>
internal sealed class PauseSync(Coop coop) : Module(coop)
{
    [Flags]
    private enum State : byte
    {
        None = 0,
        Paused = 1,
        CanPause = 2,
        Submenu = 4,

        /// <summary>One-off: the partner's pause could not be followed, so they close it again.</summary>
        Refused = 8,
    }

    private enum Want
    {
        None,
        Open,
        Close,
    }

    /// <summary>How long a partner's pause may wait for this game to allow pausing.</summary>
    private const float OpenGrace = 0.5f;

    private bool _paused;
    private State _partner;
    private bool _partnerKnown;
    private State? _sent;
    private Want _want;
    private float _wantSince;

    /// <summary>Inside a toggle this module started, which the partner's state already allows.</summary>
    private bool _remote;

    private PauseMode Mode => Coop.Config.PauseMode.Value;

    private bool PartnerPaused => (_partner & State.Paused) != 0;

    public override void Register(Session session) => session.On(MessageType.Pause, OnPartnerState);

    /// <summary>Called after the game paused or unpaused itself.</summary>
    public void OnLocalPause(bool paused)
    {
        _paused = paused;
        if (!Coop.InCoop || GameRefs.Manager is not { } gm)
        {
            return;
        }
        if (paused && Mode == PauseMode.Off)
        {
            gm.SetTimeScale(1f);
        }
        else if (!paused && Mode == PauseMode.Freeze && PartnerPaused)
        {
            Freeze(gm, true);
        }
    }

    /// <summary>Menu mode: no pause the partner can't follow, no unpause while they're in submenus.</summary>
    public bool AllowToggle(GameManager gm)
    {
        if (_remote || !_partnerKnown || Mode != PauseMode.Menu || !Coop.InCoop)
        {
            return true;
        }
        if (gm.GameState == GameState.PLAYING && !PartnerPaused && (_partner & State.CanPause) == 0)
        {
            Coop.Hints.Show($"{Coop.Partner.Name} can't pause right now");
            return false;
        }
        if (gm.GameState == GameState.PAUSED && PartnerPaused && (_partner & State.Submenu) != 0)
        {
            Coop.Hints.Show($"{Coop.Partner.Name} is in settings");
            return false;
        }
        return true;
    }

    public override void Update()
    {
        if (!Coop.InCoop || GameRefs.Manager is not { } gm)
        {
            return;
        }
        State state = Current(gm);
        if (state != _sent)
        {
            Send(state);
        }
        if (Mode == PauseMode.Menu)
        {
            Follow(gm);
        }
    }

    public override void OnJoined()
    {
        _sent = null;
        _partnerKnown = false;
        _want = Want.None;
    }

    public override void OnLeft()
    {
        if (PartnerPaused && !_paused && Mode == PauseMode.Freeze && GameRefs.Manager is { } gm)
        {
            Freeze(gm, false);
        }
        _partner = State.None;
        _partnerKnown = false;
        _want = Want.None;
    }

    private void OnPartnerState(PacketReader reader)
    {
        State state = (State)reader.Byte();
        bool was = PartnerPaused;
        _partner = state & ~State.Refused;
        _partnerKnown = true;
        if (GameRefs.Manager is not { } gm)
        {
            return;
        }
        if (Mode == PauseMode.Freeze && was != PartnerPaused && !_paused)
        {
            Freeze(gm, PartnerPaused);
        }
        else if (Mode == PauseMode.Menu)
        {
            if ((state & State.Refused) != 0)
            {
                Coop.Hints.Show($"{Coop.Partner.Name} can't pause right now");
                Request(Want.Close);
            }
            else if (was != PartnerPaused)
            {
                Request(PartnerPaused ? Want.Open : Want.Close);
            }
        }
    }

    private void Request(Want want)
    {
        _want = want;
        _wantSince = Time.unscaledTime;
    }

    /// <summary>Menu mode: opens or closes this menu after the partner's, once the game lets it.</summary>
    private void Follow(GameManager gm)
    {
        if (_want == Want.Open)
        {
            if (_paused)
            {
                _want = Want.None;
            }
            else if (CanPause(gm))
            {
                _want = Want.None;
                Toggle(gm);
            }
            else if (Time.unscaledTime - _wantSince > OpenGrace)
            {
                _want = Want.None;
                Send(Current(gm) | State.Refused);
            }
        }
        else if (_want == Want.Close)
        {
            // From a submenu the menu closes only once the player backs out to the pause screen.
            if (!_paused)
            {
                _want = Want.None;
            }
            else if (gm.ui.menuState == MainMenuState.PAUSE_MENU && gm.inputHandler.PauseAllowed)
            {
                _want = Want.None;
                Toggle(gm);
            }
        }
    }

    private void Toggle(GameManager gm)
    {
        _remote = true;
        try
        {
            gm.StartCoroutine(gm.PauseGameToggle(playSound: true));
        }
        finally
        {
            _remote = false;
        }
    }

    private void Send(State state)
    {
        _sent = state & ~State.Refused;
        Session?.Send(MessageType.Pause, w => w.Byte((byte)state));
    }

    private State Current(GameManager gm)
    {
        State state = State.None;
        if (_paused)
        {
            state |= State.Paused;
            if (gm.ui.menuState != MainMenuState.PAUSE_MENU)
            {
                state |= State.Submenu;
            }
        }
        if (CanPause(gm))
        {
            state |= State.CanPause;
        }
        return state;
    }

    /// <summary>The checks the game's Esc handling and <c>PauseGameToggle</c> make before opening.</summary>
    private static bool CanPause(GameManager gm)
    {
        bool interacting = InteractManager.BlockingInteractable != null && !gm.playerData.atBench;
        return gm.GameState == GameState.PLAYING
            && gm.inputHandler.PauseAllowed
            && !gm.playerData.disablePause
            && !gm.TimeSlowed
            && !GenericMessageCanvas.IsActive
            && !interacting;
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
