using SilksongCoop.Net;

namespace SilksongCoop.Game;

/// <summary>
/// A cutscene conversation both players see moves together: a skip on either side speeds up the partner's
/// text or turns its page. A talk a player starts with an NPC is theirs alone (D30) and is never synced.
/// </summary>
internal sealed class DialogueSync(Coop coop) : Module(coop)
{
    private int _pendingPages;
    private NPCControlBase? _localNpc;

    /// <summary>Advancing because the partner did; the patch must not echo it back.</summary>
    public static bool Applying { get; private set; }

    public override void Register(Session session) => session.On(MessageType.DialogueSkip, OnSkip);

    public override void Update()
    {
        if (DialogueBox._instance is not { } box || !box || !box.isDialogueRunning)
        {
            _pendingPages = 0;
            return;
        }
        if (_pendingPages > 0 && box.waitingToAdvance)
        {
            _pendingPages--;
            Advance(box);
        }
    }

    public override void OnLeft() => _pendingPages = 0;

    /// <summary>The running conversation is one this player started with an NPC.</summary>
    private bool InOwnTalk => _localNpc && _localNpc!.isActiveAndEnabled;

    public void OnLocalInteract(NPCControlBase npc) => _localNpc = npc;

    public void OnDialogueEnded(NPCControlBase npc)
    {
        if (npc == _localNpc)
        {
            _localNpc = null;
        }
    }

    /// <summary>Called before the local box advances on a skip press.</summary>
    public void OnLocalSkip(DialogueBox box)
    {
        if (!Applying && !InOwnTalk && Coop.Rooms.PartnerHere)
        {
            bool nextPage = box.waitingToAdvance;
            Session?.Send(MessageType.DialogueSkip, w => w.Bool(nextPage));
        }
    }

    private void OnSkip(PacketReader reader)
    {
        bool nextPage = reader.Bool();
        if (!Coop.Rooms.PartnerHere || DialogueBox._instance is not { } box || !box || !box.isDialogueRunning)
        {
            return;
        }
        if (InOwnTalk)
        {
            return;
        }
        if (nextPage && box.waitingToAdvance)
        {
            Advance(box);
            return;
        }
        _pendingPages += nextPage ? 1 : 0;
        if (box.isPrintingText)
        {
            Advance(box);
        }
    }

    private static void Advance(DialogueBox box)
    {
        Applying = true;
        try
        {
            box.AdvanceConversation();
        }
        finally
        {
            Applying = false;
        }
    }
}
