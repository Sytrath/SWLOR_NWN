﻿using System;
using System.Data.SqlClient;
using System.Linq;
using SWLOR.Game.Server.Data.Contracts;
using SWLOR.Game.Server.Data.Entities;
using SWLOR.Game.Server.Enumeration;
using SWLOR.Game.Server.GameObject;

using NWN;
using SWLOR.Game.Server.NWNX.Contracts;
using SWLOR.Game.Server.Service.Contracts;
using static NWN.NWScript;
using Object = NWN.Object;

namespace SWLOR.Game.Server.Service
{
    public class PlayerService : IPlayerService
    {
        private readonly INWScript _;
        private readonly IDataContext _db;
        private readonly IColorTokenService _color;
        private readonly INWNXCreature _nwnxCreature;
        private readonly INWNXPlayer _player;
        private readonly INWNXPlayerQuickBarSlot _qbs;
        private readonly IDialogService _dialog;
        private readonly INWNXEvents _nwnxEvents;
        private readonly IBackgroundService _background;
        private readonly IRaceService _race;
        private readonly IDurabilityService _durability;
        private readonly IPlayerStatService _stat;

        public PlayerService(
            INWScript script, 
            IDataContext db, 
            IColorTokenService color,
            INWNXCreature nwnxCreature,
            INWNXPlayer player,
            INWNXPlayerQuickBarSlot qbs,
            IDialogService dialog,
            INWNXEvents nwnxEvents,
            IBackgroundService background,
            IRaceService race,
            IDurabilityService durability,
            IPlayerStatService stat)
        {
            _ = script;
            _db = db;
            _color = color;
            _nwnxCreature = nwnxCreature;
            _player = player;
            _qbs = qbs;
            _dialog = dialog;
            _nwnxEvents = nwnxEvents;
            _background = background;
            _race = race;
            _durability = durability;
            _stat = stat;
        }

        public void InitializePlayer(NWPlayer player)
        {
            if (player == null) throw new ArgumentNullException(nameof(player));
            if (player.Object == null) throw new ArgumentNullException(nameof(player.Object));
            if (!player.IsPlayer) return;

            if (!player.IsInitializedAsPlayer)
            {
                player.DestroyAllInventoryItems();
                player.InitializePlayer();
                _.AssignCommand(player, () => _.TakeGoldFromCreature(_.GetGold(player), player, 1));

                _.DelayCommand(0.5f, () =>
                {
                    _.GiveGoldToCreature(player, 100);
                });
                

                NWItem knife = (_.CreateItemOnObject("survival_knife", player));
                knife.Name = player.Name + "'s Survival Knife";
                knife.IsCursed = true;
                _durability.SetMaxDurability(knife, 5);
                _durability.SetDurability(knife, 5);
                
                NWItem darts = (_.CreateItemOnObject("nw_wthdt001", player, 50)); // 50x Dart
                darts.Name = "Starting Darts";
                darts.IsCursed = true;

                NWItem book = (_.CreateItemOnObject("player_guide", player));
                book.Name = player.Name + "'s Player Guide";
                book.IsCursed = true;

                NWItem dyeKit = (_.CreateItemOnObject("tk_omnidye", player));
                dyeKit.IsCursed = true;
                
                int numberOfFeats = _nwnxCreature.GetFeatCount(player);
                for (int currentFeat = numberOfFeats; currentFeat >= 0; currentFeat--)
                {
                    _nwnxCreature.RemoveFeat(player, _nwnxCreature.GetFeatByIndex(player, currentFeat - 1));
                }

                _nwnxCreature.AddFeatByLevel(player, FEAT_ARMOR_PROFICIENCY_LIGHT, 1);
                _nwnxCreature.AddFeatByLevel(player, FEAT_ARMOR_PROFICIENCY_MEDIUM, 1);
                _nwnxCreature.AddFeatByLevel(player, FEAT_ARMOR_PROFICIENCY_HEAVY, 1);
                _nwnxCreature.AddFeatByLevel(player, FEAT_SHIELD_PROFICIENCY, 1);
                _nwnxCreature.AddFeatByLevel(player, FEAT_WEAPON_PROFICIENCY_EXOTIC, 1);
                _nwnxCreature.AddFeatByLevel(player, FEAT_WEAPON_PROFICIENCY_MARTIAL, 1);
                _nwnxCreature.AddFeatByLevel(player, FEAT_WEAPON_PROFICIENCY_SIMPLE, 1);
                _nwnxCreature.AddFeatByLevel(player, (int) CustomFeatType.BaseManagementTool, 1);
                _nwnxCreature.AddFeatByLevel(player, (int) CustomFeatType.OpenRestMenu, 1);

                for (int iCurSkill = 1; iCurSkill <= 27; iCurSkill++)
                {
                    _nwnxCreature.SetSkillRank(player, iCurSkill - 1, 0);
                }
                _.SetFortitudeSavingThrow(player, 0);
                _.SetReflexSavingThrow(player, 0);
                _.SetWillSavingThrow(player, 0);

                int classID = _.GetClassByPosition(1, player);

                for (int index = 0; index <= 255; index++)
                {
                    _nwnxCreature.RemoveKnownSpell(player, classID, 0, index);
                }

                PlayerCharacter entity = CreateDBPCEntity(player);
                _db.PlayerCharacters.Add(entity);
                _db.SaveChanges();

                _db.StoredProcedure("InsertAllPCSkillsByID",
                    new SqlParameter("PlayerID", player.GlobalID));

                _race.ApplyDefaultAppearance(player);

                _background.ApplyBackgroundBonuses(player);

                _stat.ApplyStatChanges(player, null, true);

                _.DelayCommand(1.0f, () => _.ApplyEffectToObject(DURATION_TYPE_INSTANT, _.EffectHeal(999), player));

                InitializeHotBar(player);
            }

        }

        private PlayerCharacter CreateDBPCEntity(NWPlayer player)
        {
            PlayerCharacter entity = new PlayerCharacter
            {
                PlayerID = player.GlobalID,
                CharacterName = player.Name,
                HitPoints = player.CurrentHP,
                LocationAreaResref = _.GetResRef(_.GetAreaFromLocation(player.Location)),
                LocationX = player.Position.m_X,
                LocationY = player.Position.m_Y,
                LocationZ = player.Position.m_Z,
                LocationOrientation = player.Facing,
                CreateTimestamp = DateTime.UtcNow,
                UnallocatedSP = 5,
                HPRegenerationAmount = 1,
                RegenerationTick = 20,
                RegenerationRate = 0,
                VersionNumber = 1,
                MaxFP = 0,
                CurrentFP = 0,
                CurrentFPTick = 20,
                RespawnAreaResref = string.Empty,
                RespawnLocationX = 0.0f,
                RespawnLocationY = 0.0f,
                RespawnLocationZ = 0.0f,
                RespawnLocationOrientation = 0.0f,
                DateSanctuaryEnds = DateTime.UtcNow + TimeSpan.FromDays(3),
                IsSanctuaryOverrideEnabled = false,
                STRBase = _nwnxCreature.GetRawAbilityScore(player, ABILITY_STRENGTH),
                DEXBase = _nwnxCreature.GetRawAbilityScore(player, ABILITY_DEXTERITY),
                CONBase = _nwnxCreature.GetRawAbilityScore(player, ABILITY_CONSTITUTION),
                INTBase = _nwnxCreature.GetRawAbilityScore(player, ABILITY_INTELLIGENCE),
                WISBase = _nwnxCreature.GetRawAbilityScore(player, ABILITY_WISDOM),
                CHABase = _nwnxCreature.GetRawAbilityScore(player, ABILITY_CHARISMA),
                TotalSPAcquired = 0,
                DisplayHelmet = true,
                PrimaryResidencePCBaseStructureID = null
            };

            return entity;
        }

        public PlayerCharacter GetPlayerEntity(NWPlayer player)
        {
            if(player == null) throw new ArgumentNullException(nameof(player));
            if(!player.IsPlayer) throw new ArgumentException(nameof(player) + " must be a player.", nameof(player));

            return _db.PlayerCharacters.Single(x => x.PlayerID == player.GlobalID);
        }

        public PlayerCharacter GetPlayerEntity(string playerID)
        {
            if (string.IsNullOrWhiteSpace(playerID)) throw new ArgumentException("Invalid player ID.", nameof(playerID));

            return _db.PlayerCharacters.Single(x => x.PlayerID == playerID);
        }

        public void OnAreaEnter()
        {
            NWPlayer player = (_.GetEnteringObject());

            LoadLocation(player);
            SaveLocation(player);
            if(player.IsPlayer)
                _.ExportSingleCharacter(player);
        }

        public void LoadCharacter(NWPlayer player)
        {
            if (!player.IsPlayer) return;

            PlayerCharacter entity = GetPlayerEntity(player.GlobalID);

            if (entity == null) return;

            int hp = player.CurrentHP;
            int damage;
            if (entity.HitPoints < 0)
            {
                damage = hp + Math.Abs(entity.HitPoints);
            }
            else
            {
                damage = hp - entity.HitPoints;
            }

            if (damage != 0)
            {
                _.ApplyEffectToObject(DURATION_TYPE_INSTANT, _.EffectDamage(damage), player);
            }

            player.IsBusy = false; // Just in case player logged out in the middle of an action.
        }

        public void ShowMOTD(NWPlayer player)
        {
            ServerConfiguration config = _db.ServerConfigurations.First();
            string message = _color.Green("Welcome to " + config.ServerName + "!\n\nMOTD: ") + _color.White(config.MessageOfTheDay);

            _.DelayCommand(6.5f, () =>
            {
                player.SendMessage(message);
            });
        }

        public void SaveCharacter(NWPlayer player)
        {
            if (!player.IsPlayer) return;
            PlayerCharacter entity = GetPlayerEntity(player);
            entity.CharacterName = player.Name;
            entity.HitPoints = player.CurrentHP;

            _db.SaveChanges();
        }

        public void SaveLocation(NWPlayer player)
        {
            if (!player.IsPlayer) return;

            NWArea area = player.Area;
            if (area.Tag != "ooc_area" && area.Tag != "tutorial" && !area.IsInstance)
            {
                PlayerCharacter entity = GetPlayerEntity(player.GlobalID);
                entity.LocationAreaResref = area.Resref;
                entity.LocationX = player.Position.m_X;
                entity.LocationY = player.Position.m_Y;
                entity.LocationZ = player.Position.m_Z;
                entity.LocationOrientation = (player.Facing);

                if (string.IsNullOrWhiteSpace(entity.RespawnAreaResref))
                {
                    NWObject waypoint = _.GetWaypointByTag("DTH_DEFAULT_RESPAWN_POINT");
                    entity.RespawnAreaResref = waypoint.Resref;
                    entity.RespawnLocationOrientation = waypoint.Facing;
                    entity.RespawnLocationX = waypoint.Position.m_X;
                    entity.RespawnLocationY = waypoint.Position.m_Y;
                    entity.RespawnLocationZ = waypoint.Position.m_Z;
                }

                _db.SaveChanges();
            }
        }

        private void LoadLocation(NWPlayer player)
        {
            if (!player.IsPlayer) return;

            if (player.Area.Tag == "ooc_area")
            {
                PlayerCharacter entity = GetPlayerEntity(player.GlobalID);
                NWArea area = NWModule.Get().Areas.SingleOrDefault(x => x.Resref == entity.LocationAreaResref);
                if (area == null) return;

                Vector position = _.Vector((float)entity.LocationX, (float)entity.LocationY, (float)entity.LocationZ);
                Location location = _.Location(area.Object,
                    position,
                    (float)entity.LocationOrientation);

                player.AssignCommand(() => _.ActionJumpToLocation(location));
            }
        }


        private void CheckForMovement(NWPlayer oPC, Location location)
        {
            if (!oPC.IsValid || oPC.IsDead) return;
            
            string areaResref = oPC.Area.Resref;
            Vector position = _.GetPositionFromLocation(location);

            if (areaResref != _.GetResRef(_.GetAreaFromLocation(location)) ||
                oPC.Facing != _.GetFacingFromLocation(location) ||
                oPC.Position.m_X != position.m_X ||
                oPC.Position.m_Y != position.m_Y ||
                oPC.Position.m_Z != position.m_Z)
            {
                foreach (Effect effect in oPC.Effects)
                {
                    int type = _.GetEffectType(effect);
                    if (type == EFFECT_TYPE_DAMAGE_REDUCTION || type == EFFECT_TYPE_SANCTUARY)
                    {
                        _.RemoveEffect(oPC.Object, effect);
                    }
                }
                return;
            }
            
            _.DelayCommand(1.0f, () =>
            {
                CheckForMovement(oPC, location);
            });
        }
        
        private void InitializeHotBar(NWPlayer player)
        {
            var openRestMenu = _qbs.UseFeat((int)CustomFeatType.OpenRestMenu);
            var structure = _qbs.UseFeat((int) CustomFeatType.BaseManagementTool);
            
            _player.SetQuickBarSlot(player, 0, openRestMenu);
            _player.SetQuickBarSlot(player, 1, structure);
        }

        public void OnModuleUseFeat()
        {
            NWPlayer pc = (Object.OBJECT_SELF);
            int featID = _nwnxEvents.OnFeatUsed_GetFeatID();

            if (featID != (int)CustomFeatType.OpenRestMenu) return;
            pc.ClearAllActions();
            _dialog.StartConversation(pc, pc, "RestMenu");
        }

    }
}
