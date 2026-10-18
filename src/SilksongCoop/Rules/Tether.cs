using System.Collections.Generic;
using System.Linq;

namespace SilksongCoop.Rules;

internal enum TetherVerdict
{
    Allowed,
    PartnerTooFar,
    PartnerDown,
}

/// <summary>Where the partner is, as far as the tether cares.</summary>
internal sealed record PartnerRoom(string Scene, IReadOnlyCollection<string> Neighbours, bool Downed);

/// <summary>D7, D11, D19: same or neighbouring rooms only; nobody leaves a downed partner.</summary>
internal static class Tether
{
    public static TetherVerdict Check(string myScene, string targetScene, PartnerRoom? partner)
    {
        if (partner is null || targetScene == myScene)
        {
            return TetherVerdict.Allowed;
        }
        if (partner.Downed && partner.Scene == myScene)
        {
            return TetherVerdict.PartnerDown;
        }
        bool nearPartner = targetScene == partner.Scene || partner.Neighbours.Contains(targetScene);
        return nearPartner ? TetherVerdict.Allowed : TetherVerdict.PartnerTooFar;
    }

    public static string Hint(TetherVerdict verdict)
    {
        return verdict switch
        {
            TetherVerdict.PartnerTooFar => "Partner too far",
            TetherVerdict.PartnerDown => "Partner is down",
            _ => "",
        };
    }
}
