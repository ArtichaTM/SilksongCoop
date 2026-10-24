using System.Linq;
using SilksongCoop.Net;
using UnityEngine;

namespace SilksongCoop.Game;

/// <summary>
/// One shop stock per world (3.4 a): when a player buys, the partner's open shop in that room closes, so its
/// list never offers what is already sold. Reopening builds the list from the shared save.
/// </summary>
internal sealed class ShopSync(Coop coop) : Module(coop)
{
    private const string ShopFsm = "shop_control";
    private const string OpenState = "Open";
    private const float CloseFor = 1f;

    private float _closeUntil;

    public override void Register(Session session) => session.On(MessageType.ShopBought, OnBought);

    public void OnLocalPurchase()
    {
        Coop.Spending.OnShopPurchase();
        if (Coop.Rooms.PartnerHere)
        {
            Session?.Send(MessageType.ShopBought);
        }
    }

    /// <summary>A confirm dialog takes the first close; the shop itself the next one.</summary>
    public override void Update()
    {
        if (Time.unscaledTime >= _closeUntil)
        {
            return;
        }
        PlayMakerFSM? shop = Object.FindObjectsByType<PlayMakerFSM>(FindObjectsSortMode.None)
            .FirstOrDefault(fsm => fsm.FsmName == ShopFsm && fsm.ActiveStateName == OpenState);
        if (shop)
        {
            shop!.SendEvent("CLOSE");
        }
    }

    private void OnBought(PacketReader reader)
    {
        if (Coop.Rooms.PartnerHere)
        {
            _closeUntil = Time.unscaledTime + CloseFor;
            Coop.Hints.Show($"{Coop.Partner.Name} bought something: shop closed");
        }
    }
}
