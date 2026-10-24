using System.Linq;
using HutongGames.PlayMaker;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// Pay-to-unlock machines (bell bench tolls, Bellway stations, tubes, map machines; marks 2/3, 5), 3.2 a:
/// only the payer pays. Their <c>PlayMakerNPC</c> keeps them out of <see cref="EnemySync"/>, so the payment
/// is sent here and the partner's machine jumps to the state a room load gives an unlocked one.
/// </summary>
internal sealed class TollSync(Coop coop) : Module(coop)
{
    public const string UnlockFsm = "Unlock Behaviour";
    private const string PromptState = "Prompt";
    private const string IdleState = "Inert";
    private const string UnlockedEvent = "ACTIVATED";

    public override void Register(Session session) => session.On(MessageType.TollPaid, OnPartnerPaid);

    /// <summary>Paid here: the prompt's FINISHED leads out of it (CANCEL is a refusal).</summary>
    public void OnSwitch(Fsm fsm, FsmState toState)
    {
        if (fsm.Name != UnlockFsm || fsm.ActiveStateName != PromptState || !Coop.Rooms.PartnerHere)
        {
            return;
        }
        bool paid = Target(fsm, PromptState, FsmEvent.Finished.Name) == toState.Name;
        if (!paid || !Coop.Rooms.Holds(fsm.GameObject.scene))
        {
            return;
        }
        int room = Coop.Rooms.CurrentHash;
        int id = EnemySync.IdOf(fsm.GameObject);
        Session?.Send(MessageType.TollPaid, w => w.Int(room).Int(id));
        Coop.Log.LogInfo($"toll paid at {fsm.GameObject.name}; unlocking the partner's");
    }

    private void OnPartnerPaid(PacketReader reader)
    {
        int room = reader.Int();
        int id = reader.Int();
        if (room != Coop.Rooms.CurrentHash)
        {
            return;
        }
        PlayMakerFSM? machine = Object.FindObjectsByType<PlayMakerFSM>(FindObjectsSortMode.None)
            .FirstOrDefault(fsm => fsm.FsmName == UnlockFsm && EnemySync.IdOf(fsm.gameObject) == id);
        if (machine is null)
        {
            Coop.Log.LogWarning($"partner paid a toll ({id:x8}) not found here");
            return;
        }
        string? unlocked = Target(machine.Fsm, IdleState, UnlockedEvent);
        if (machine.ActiveStateName != IdleState || unlocked is null)
        {
            // Mid-prompt here too: the save flag arrives anyway and the next room load shows it unlocked.
            string state = machine.ActiveStateName;
            Coop.Log.LogWarning($"partner paid {machine.name}; ours is in {state}, left alone");
            return;
        }
        machine.SetState(unlocked);
        Coop.Hints.Show($"{Coop.Partner.Name} paid the toll");
    }

    private static string? Target(Fsm fsm, string state, string eventName) =>
        fsm.GetState(state)?.Transitions.FirstOrDefault(t => t.EventName == eventName)?.ToState;
}
