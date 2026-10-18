using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// A conversation both players see moves together: a skip on either side speeds up the partner's text or
/// turns its page. Only the player who started the talk is held in place (11.1 a).
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

    public void OnLocalInteract(NPCControlBase npc) => _localNpc = npc;

    public void OnDialogueEnded(NPCControlBase npc)
    {
        if (npc == _localNpc)
        {
            _localNpc = null;
        }
    }

    /// <summary>A lock the partner's conversation would put on our hero; logged when kept off.</summary>
    public bool BlocksLock(NPCControlBase? npc, string what)
    {
        if (npc is null || !npc || npc == _localNpc || !Coop.Rooms.PartnerHere)
        {
            return false;
        }
        EnemySync rooms = Coop.Enemies;
        bool synced = npc.GetComponents<PlayMakerFSM>().Any(fsm => rooms.ModeOf(fsm.Fsm) != FsmMode.None);
        if (synced)
        {
            Coop.Log.LogInfo($"kept the hero free during the partner's talk: {what} from {npc.name}");
        }
        return synced;
    }

    /// <summary>The NPC whose FSM is running right now, if any.</summary>
    public static NPCControlBase? RunningNpc()
    {
        GameObject? running = FsmExecutionStack.ExecutingFsm?.GameObject;
        return running ? running!.GetComponentInParent<NPCControlBase>() : null;
    }

    /// <summary>Called before the local box advances on a skip press.</summary>
    public void OnLocalSkip(DialogueBox box)
    {
        if (!Applying && Coop.Rooms.PartnerHere)
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
