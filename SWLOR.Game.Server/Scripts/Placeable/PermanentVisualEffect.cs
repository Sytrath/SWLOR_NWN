﻿using NWN;
using SWLOR.Game.Server.GameObject;

namespace SWLOR.Game.Server.Scripts.Placeable
{
    public class PermanentVisualEffect: IScript
    {
        public void SubscribeEvents()
        {
        }

        public void UnsubscribeEvents()
        {
        }

        public void Main()
        {
            NWPlaceable self = _.OBJECT_SELF;

            int vfxID = self.GetLocalInt("PERMANENT_VFX_ID");
            
            if (vfxID > 0)
            {
                _.ApplyEffectToObject(DurationType.Permanent, _.EffectVisualEffect(vfxID), self);
            }

            _.SetEventScript(self, _.EVENT_SCRIPT_PLACEABLE_ON_HEARTBEAT, string.Empty);
        }
    }
}
