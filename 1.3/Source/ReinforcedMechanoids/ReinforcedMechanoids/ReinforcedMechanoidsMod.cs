﻿using RimWorld;
using System.Linq;
using UnityEngine;
using Verse;
using VFEMech;

namespace ReinforcedMechanoids
{
    internal class ReinforcedMechanoidsMod : Mod
    {
        public static ReinforcedMechanoidsSettings settings;
        public ReinforcedMechanoidsMod(ModContentPack mcp)
            : base(mcp)
        {
            settings = GetSettings<ReinforcedMechanoidsSettings>();
        }

        public override void WriteSettings()
        {
            base.WriteSettings();
            ApplySettings();
        }
        public static void ApplySettings()
        {
            RM_DefOf.RM_VanometricMechanoidCell.SetStatBaseValue(StatDefOf.MarketValue, ReinforcedMechanoidsSettings.marketValue);
            RM_DefOf.RM_VanometricGenerator.GetCompProperties<CompProperties_Power>().basePowerConsumption = 0f - ReinforcedMechanoidsSettings.powerOutput;
            if (ReinforcedMechanoidsSettings.dropWeaponOnDeath)
            {
                foreach (var mechanoid in DefDatabase<PawnKindDef>.AllDefs.Where(x => x.RaceProps.IsMechanoid))
                {
                    mechanoid.destroyGearOnDrop = false;
                }
            }
            foreach (var mechanoid in ReinforcedMechanoidsSettings.disabledMechanoids)
            {
                var pawnKind = DefDatabase<PawnKindDef>.GetNamedSilentFail(mechanoid);
                if (pawnKind != null)
                {
                    foreach (var faction in DefDatabase<FactionDef>.AllDefs)
                    {
                        if (faction.pawnGroupMakers != null)
                        {
                            foreach (var groupMaker in faction.pawnGroupMakers)
                            {
                                groupMaker.traders?.RemoveAll(x => x.kind == pawnKind);
                                groupMaker.carriers?.RemoveAll(x => x.kind == pawnKind);
                                groupMaker.guards?.RemoveAll(x => x.kind == pawnKind);
                                groupMaker.options?.RemoveAll(x => x.kind == pawnKind);
                            }
                            var removed = faction.pawnGroupMakers.RemoveAll(x => x.carriers?.Count() == 0 && x.traders?.Count() == 0 
                                && x.guards?.Count() == 0 && x.options?.Count() == 0);
                        }
                    }
                }
            }
        }

        public override string SettingsCategory()
        {
            return this.Content.Name;
        }

        public float scrollHeight;
        public Vector2 scrollPosition;
        public override void DoSettingsWindowContents(Rect rect)
        {
            var inRect = new Rect(rect.x, rect.y, rect.width - 16, 10000);
            var totalRect = new Rect(rect.x, rect.y, rect.width - 16, scrollHeight);
            Widgets.BeginScrollView(rect, ref scrollPosition, totalRect);
            scrollHeight = 0;
            Listing_Standard listing_Standard = new Listing_Standard();
            listing_Standard.Begin(inRect);
            listing_Standard.Gap(10f);
            Rect rect2 = listing_Standard.GetRect(Text.LineHeight);
            Rect rect3 = rect2.LeftHalf().Rounded();
            Rect rect4 = rect2.RightHalf().Rounded();
            Rect rect5 = rect3.LeftHalf().Rounded();
            Rect rect6 = rect3.RightHalf().Rounded();
            Widgets.Label(rect5, "<b>Power Cell</b> market value");
            Widgets.Label(rect6, $"<b>{ReinforcedMechanoidsSettings.marketValue:00}</b> <color=#ababab>(Influence on difficulty)</color>");
            if (Widgets.ButtonText(new Rect(rect4.xMin, rect4.y, rect4.height, rect4.height), "-", drawBackground: true, doMouseoverSound: false) && ReinforcedMechanoidsSettings.marketValue >= 500f)
            {
                ReinforcedMechanoidsSettings.marketValue -= 50f;
            }
            ReinforcedMechanoidsSettings.marketValue = Widgets.HorizontalSlider(new Rect(rect4.xMin + rect4.height + 10f, rect4.y, rect4.width - (rect4.height * 2f + 20f), rect4.height), ReinforcedMechanoidsSettings.marketValue, 500f, 4000f, middleAlignment: true);
            if (Widgets.ButtonText(new Rect(rect4.xMax - rect4.height, rect4.y, rect4.height, rect4.height), "+", drawBackground: true, doMouseoverSound: false) && ReinforcedMechanoidsSettings.marketValue < 4000f)
            {
                ReinforcedMechanoidsSettings.marketValue += 50f;
            }
            listing_Standard.Gap(10f);
            Rect rect7 = listing_Standard.GetRect(Text.LineHeight);
            Rect rect8 = rect7.LeftHalf().Rounded();
            Rect rect9 = rect7.RightHalf().Rounded();
            Rect rect10 = rect8.LeftHalf().Rounded();
            Rect rect11 = rect8.RightHalf().Rounded();
            Widgets.Label(rect10, "<b>Power Cell</b> power output (W)");
            Widgets.Label(rect11, $"<b>{ReinforcedMechanoidsSettings.powerOutput:00}W</b> <color=#ababab>(recommended: 5000W)</color>");
            if (Widgets.ButtonText(new Rect(rect9.xMin, rect9.y, rect9.height, rect9.height), "-", drawBackground: true, doMouseoverSound: false) && ReinforcedMechanoidsSettings.powerOutput >= 2000f)
            {
                ReinforcedMechanoidsSettings.powerOutput -= 500f;
            }
            ReinforcedMechanoidsSettings.powerOutput = Widgets.HorizontalSlider(new Rect(rect9.xMin + rect9.height + 10f, rect9.y, rect9.width - (rect9.height * 2f + 20f), rect9.height), ReinforcedMechanoidsSettings.powerOutput, 2000f, 20000f, middleAlignment: true);
            if (Widgets.ButtonText(new Rect(rect9.xMax - rect9.height, rect9.y, rect9.height, rect9.height), "+", drawBackground: true, doMouseoverSound: false) && ReinforcedMechanoidsSettings.powerOutput < 20000f)
            {
                ReinforcedMechanoidsSettings.powerOutput += 500f;
            }
            listing_Standard.Gap();
            listing_Standard.CheckboxLabeled("Mechanoids will drop weapons upon death", ref ReinforcedMechanoidsSettings.dropWeaponOnDeath);
            listing_Standard.GapLine();
            listing_Standard.Label("Disable mechanoids from spawning in the game");
            foreach (var pawn in DefDatabase<PawnKindDef>.AllDefs)
            {
                if (pawn.RaceProps.IsMechanoid && typeof(Machine).IsAssignableFrom(pawn.race.thingClass) is false)
                {
                    var enabled = ReinforcedMechanoidsSettings.disabledMechanoids.Contains(pawn.defName) is false;
                    listing_Standard.CheckboxLabeled(pawn.LabelCap, ref enabled);
                    if (enabled && ReinforcedMechanoidsSettings.disabledMechanoids.Contains(pawn.defName))
                    {
                        ReinforcedMechanoidsSettings.disabledMechanoids.Remove(pawn.defName);
                    }
                    else if (!enabled && !ReinforcedMechanoidsSettings.disabledMechanoids.Contains(pawn.defName))
                    {
                        ReinforcedMechanoidsSettings.disabledMechanoids.Add(pawn.defName);
                    }
                }
            }
            scrollHeight = listing_Standard.CurHeight;
            listing_Standard.End();
            Widgets.EndScrollView();
        }
    }
}
