using BaseLib.Patches.Content;
using MegaCrit.Sts2.Core.Entities.Cards;
using MegaCrit.Sts2.Core.HoverTips;
using MegaCrit.Sts2.Core.Localization;

namespace Wanderer.WandererCode.Keywords;

public static class WandererKeywords
{
    [CustomEnum, KeywordProperties(AutoKeywordPosition.Before)]
    public static CardKeyword Enshrined;

    public static IHoverTip ShiftHoverTip => new HoverTip(new LocString("card_keywords", "WANDERER-SHIFT.title"), new LocString("card_keywords", "WANDERER-SHIFT.description"));
}