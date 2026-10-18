using SilksongCoop.Net;

namespace SilksongCoop.Game;

/// <summary>One coop feature. The hub calls these on the main thread in a fixed order.</summary>
internal abstract class Module(Coop coop)
{
    protected Coop Coop { get; } = coop;

    protected Session? Session => Coop.Session;

    /// <summary>Hooks this module's message handlers into a new session.</summary>
    public virtual void Register(Session session)
    {
    }

    public virtual void Update()
    {
    }

    public virtual void LateUpdate()
    {
    }

    /// <summary>The partner passed the handshake.</summary>
    public virtual void OnJoined()
    {
    }

    /// <summary>The partner is gone (left, dropped or the session ended).</summary>
    public virtual void OnLeft()
    {
    }

    /// <summary>Plugin unload (C2): undo everything this module created.</summary>
    public virtual void Dispose()
    {
    }
}
