﻿using System.Collections.Generic;
using SWLOR.Game.Server.Core.NWScript.Enum;
using SWLOR.Game.Server.Core.NWScript.Enum.VisualEffect;
using SWLOR.Game.Server.Service;
using SWLOR.Game.Server.Service.SkillService;
using SWLOR.Game.Server.Service.StatusEffectService;
using static SWLOR.Game.Server.Core.NWScript.NWScript;
namespace SWLOR.Game.Server.Feature.StatusEffectDefinition
{
    public class ForceDrainStatusEffectDefinition : IStatusEffectListDefinition
    {
        public Dictionary<StatusEffectType, StatusEffectDetail> BuildStatusEffects()
        {
            var builder = new StatusEffectBuilder();
            ForceDrain1(builder);
            ForceDrain2(builder);
            ForceDrain3(builder);
            ForceDrain4(builder);
            ForceDrain5(builder);

            return builder.Build();
        }
        private void ForceDrain1(StatusEffectBuilder builder)
        {
            builder.Create(StatusEffectType.ForceDrain1)
                .Name("Force Drain I")
                .EffectIcon(EffectIconType.LevelDrain)
                .GrantAction((source, target, length, effectData) =>
                {
                    ProcessForceDrainTick(VisualEffect.Vfx_Beam_Drain, 10, 10, target, source);
                    Enmity.ModifyEnmityOnAll(source, 1);

                    if (!CombatPoint.AddCombatPointToAllTagged(source, SkillType.Force, 3))
                    {
                        CombatPoint.AddCombatPoint(source, target, SkillType.Force, 3);
                    }
                })
                .TickAction((source, target, effectData) =>
                {
                    ProcessForceDrainTick(VisualEffect.Vfx_Beam_Drain, 10, 10, target, source);
                    Enmity.ModifyEnmityOnAll(source, 1);

                    if (!CombatPoint.AddCombatPointToAllTagged(source, SkillType.Force, 3))
                    {
                        CombatPoint.AddCombatPoint(source, target, SkillType.Force, 3);
                    }
                });
        }
        private void ForceDrain2(StatusEffectBuilder builder)
        {
            builder.Create(StatusEffectType.ForceDrain2)
                .Name("Force Drain II")
                .EffectIcon(EffectIconType.LevelDrain)
                .GrantAction((source, target, length, effectData) =>
                {
                    ProcessForceDrainTick(VisualEffect.Vfx_Beam_Drain, 15, 15, target, source);
                    Enmity.ModifyEnmityOnAll(source, 1);

                    if (!CombatPoint.AddCombatPointToAllTagged(source, SkillType.Force, 3))
                    {
                        CombatPoint.AddCombatPoint(source, target, SkillType.Force, 3);
                    }
                })
                .TickAction((source, target, effectData) =>
                {
                    ProcessForceDrainTick(VisualEffect.Vfx_Beam_Drain, 15, 15, target, source);
                    Enmity.ModifyEnmityOnAll(source, 2);

                    if (!CombatPoint.AddCombatPointToAllTagged(source, SkillType.Force, 3))
                    {
                        CombatPoint.AddCombatPoint(source, target, SkillType.Force, 3);
                    }
                });
        }
        private void ForceDrain3(StatusEffectBuilder builder)
        {
            builder.Create(StatusEffectType.ForceDrain3)
                .Name("Force Drain III")
                .EffectIcon(EffectIconType.LevelDrain)
                .GrantAction((source, target, length, effectData) =>
                {
                    ProcessForceDrainTick(VisualEffect.Vfx_Beam_Drain, 20, 20, target, source);
                    Enmity.ModifyEnmityOnAll(source, 1);

                    if (!CombatPoint.AddCombatPointToAllTagged(source, SkillType.Force, 3))
                    {
                        CombatPoint.AddCombatPoint(source, target, SkillType.Force, 3);
                    }
                })
                .TickAction((source, target, effectData) =>
                {
                    ProcessForceDrainTick(VisualEffect.Vfx_Beam_Drain, 20, 20, target, source);
                    Enmity.ModifyEnmityOnAll(source, 3);

                    if (!CombatPoint.AddCombatPointToAllTagged(source, SkillType.Force, 3))
                    {
                        CombatPoint.AddCombatPoint(source, target, SkillType.Force, 3);
                    }
                });
        }
        private void ForceDrain4(StatusEffectBuilder builder)
        {
            builder.Create(StatusEffectType.ForceDrain4)
                .Name("Force Drain IV")
                .EffectIcon(EffectIconType.LevelDrain)
                .GrantAction((source, target, length, effectData) =>
                {
                    ProcessForceDrainTick(VisualEffect.Vfx_Beam_Drain, 25, 25, target, source);
                    Enmity.ModifyEnmityOnAll(source, 1);

                    if (!CombatPoint.AddCombatPointToAllTagged(source, SkillType.Force, 3))
                    {
                        CombatPoint.AddCombatPoint(source, target, SkillType.Force, 3);
                    }
                })
                .TickAction((source, target, effectData) =>
                {
                    ProcessForceDrainTick(VisualEffect.Vfx_Beam_Drain, 25, 25, target, source);
                    Enmity.ModifyEnmityOnAll(source, 4);

                    if (!CombatPoint.AddCombatPointToAllTagged(source, SkillType.Force, 3))
                    {
                        CombatPoint.AddCombatPoint(source, target, SkillType.Force, 3);
                    }
                });
        }
        private void ForceDrain5(StatusEffectBuilder builder)
        {
            builder.Create(StatusEffectType.ForceDrain5)
                .Name("Force Drain V")
                .EffectIcon(EffectIconType.LevelDrain)
                .TickAction((source, target, effectData) =>
                {
                    ProcessForceDrainTick(VisualEffect.Vfx_Beam_Drain, 30, 30, target, source);
                    Enmity.ModifyEnmityOnAll(source, 5);

                    if (!CombatPoint.AddCombatPointToAllTagged(source, SkillType.Force, 3))
                    {
                        CombatPoint.AddCombatPoint(source, target, SkillType.Force, 3);
                    }
                });
        }

        private void ProcessForceDrainTick(VisualEffect vfx1, int damageAmt, int healAmt, uint target, uint source)
        {
            if (!Ability.GetAbilityResisted(source, target))
            {
                PlaySound("plr_force_absorb");
                ApplyEffectToObject(DurationType.Temporary, EffectBeam(vfx1, target, BodyNode.Hand), source, 2.0F);
                ApplyEffectToObject(DurationType.Temporary, EffectBeam(vfx1, source, BodyNode.Hand), target, 2.0F);
                ApplyEffectToObject(DurationType.Instant, EffectVisualEffect(VisualEffect.Vfx_Imp_Negative_Energy), target);
                ApplyEffectToObject(DurationType.Instant, EffectVisualEffect(VisualEffect.Vfx_Imp_Reduce_Ability_Score), target);
                ApplyEffectToObject(DurationType.Instant, EffectDamage(damageAmt), target);
                ApplyEffectToObject(DurationType.Instant, EffectHeal(healAmt), source);
                ApplyEffectToObject(DurationType.Instant, EffectVisualEffect(VisualEffect.Vfx_Imp_Pulse_Negative), source);
            }
        }
    }
}
