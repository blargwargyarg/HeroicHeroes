using System;
using System.Windows.Forms;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace HeroicHeroes
{
    [HarmonyPatch(typeof(Mission))]
    internal static class HeroicHeroesPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("GetStaggerThresholdMultiplier")]
        public static bool Prefix1(Agent victimAgent, ref float __result)
        {
            try
            {
                if (victimAgent != null)
                {
                    if (victimAgent.IsMount)
                    {
                        if (HeroicHeroesSettings.Instance.MountProjectiles)
                            if (victimAgent.RiderAgent != null)
                                if ((victimAgent.RiderAgent.IsMainAgent && HeroicHeroesSettings.Instance.SPlayerMultiplier > 1 ||
                                    (victimAgent.RiderAgent.IsHero && HeroicHeroesSettings.Instance.SHeroMultiplier > 1) ||
                                    HeroicHeroesSettings.Instance.SAIMultiplier > 1) &&
                                    checkFriendly(HeroicHeroesSettings.Instance.FriendlyProjectiles, victimAgent.RiderAgent)
                                    ) return false;
                    }
                    else if (victimAgent.IsMainAgent)
                    {
                        __result = HeroicHeroesSettings.Instance.SPlayerMultiplier * MissionGameModels.Current.AgentApplyDamageModel.CalculateStaggerThresholdMultiplier(victimAgent);
                        return false;
                    }
                    else if (victimAgent.IsHero)
                    {
                        if (true && checkFriendly(HeroicHeroesSettings.Instance.SFriendlyOnly, victimAgent))
                        {
                            __result = HeroicHeroesSettings.Instance.SHeroMultiplier * MissionGameModels.Current.AgentApplyDamageModel.CalculateStaggerThresholdMultiplier(victimAgent);
                            return false;
                        }
                        else if (true && checkFriendly(HeroicHeroesSettings.Instance.SFriendlyOnly, victimAgent))
                        {
                            __result = HeroicHeroesSettings.Instance.SAIMultiplier * MissionGameModels.Current.AgentApplyDamageModel.CalculateStaggerThresholdMultiplier(victimAgent);
                            return false;
                        }
                    }
                }
            }
            catch (Exception arg)
            {
                MessageBox.Show(string.Format("Error with GetStaggerThresholdMultiplier:\n\n{0}", arg));
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("HandleMissileCollisionReaction")]
        public static bool Prefix2(int missileIndex, ref Mission.MissileCollisionReaction collisionReaction,
            MatrixFrame attachLocalFrame, Agent attackerAgent, Agent attachedAgent, bool attachedToShield,
            sbyte attachedBoneIndex, MissionObject attachedMissionObject, Vec3 bounceBackVelocity, Vec3 bounceBackAngularVelocity, int forcedSpawnIndex)
        {
            try
            {
                if (attachedAgent != null && attackerAgent != null && 
                    (!HeroicHeroesSettings.Instance.HeroProjectilesExemption || (attackerAgent.IsHuman && attackerAgent.IsHero)))
                {
                    if (attachedAgent.IsMount)
                    {
                        if (HeroicHeroesSettings.Instance.MountProjectiles)
                            if (attachedAgent.RiderAgent != null)
                                if ((attachedAgent.RiderAgent.IsMainAgent && HeroicHeroesSettings.Instance.MainProjectiles ||
                                    (attachedAgent.RiderAgent.IsHero && HeroicHeroesSettings.Instance.HeroProjectiles) ||
                                    HeroicHeroesSettings.Instance.AIProjectiles) &&
                                    checkFriendly(HeroicHeroesSettings.Instance.FriendlyProjectiles, attachedAgent.RiderAgent)
                                    ) collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
                    }
                    else if (attachedAgent.IsMainAgent)
                    {
                        if (HeroicHeroesSettings.Instance.MainProjectiles || HeroicHeroesSettings.Instance.MainStick)
                            collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
                    }
                    else if (attachedAgent.IsHero)
                    {
                        if (HeroicHeroesSettings.Instance.HeroProjectiles)
                            if (checkFriendly(HeroicHeroesSettings.Instance.FriendlyOnly, attachedAgent))
                                collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;

                    }
                    else if (HeroicHeroesSettings.Instance.AIProjectiles && checkFriendly(HeroicHeroesSettings.Instance.FriendlyOnly, attachedAgent))
                        collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
                }
            }
            catch (Exception arg)
            {
                MessageBox.Show(string.Format("Error with HandleMissileCollisionReaction:\n\n{0}", arg));
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("RegisterBlow")]
        public static bool Prefix3(Agent attacker, Agent victim, GameEntity realHitEntity, ref Blow b, ref AttackCollisionData collisionData)
        {
            try
            {
                if (attacker == null || victim == null) return true;
                if (!(!collisionData.AttackBlockedWithShield && b.SelfInflictedDamage < 0 && attacker.IsEnemyOf(victim))) return true;
                if (!HeroicHeroesSettings.Instance.DHeroExemption || (victim.IsHuman && victim.IsHero))
                    if (attacker.IsMainAgent)
                    {
                        collisionData.InflictedDamage *= HeroicHeroesSettings.Instance.DPlayerMultiplier;
                        b.InflictedDamage *= HeroicHeroesSettings.Instance.DPlayerMultiplier;
                    }
                    else if (attacker.IsHero)
                    {
                        if (checkFriendly(HeroicHeroesSettings.Instance.FriendlyOnly, attacker))
                            b.InflictedDamage *= HeroicHeroesSettings.Instance.DHeroMultiplier;
                    }
                    else if (checkFriendly(HeroicHeroesSettings.Instance.FriendlyOnly, attacker))
                        b.InflictedDamage *= HeroicHeroesSettings.Instance.DAIMultiplier;
                if (b.IsMissile())
                {
                    if (!HeroicHeroesSettings.Instance.HeroProjectilesExemption || (attacker.IsHuman && attacker.IsHero))
                        if (victim.IsMount)
                        {
                            if (HeroicHeroesSettings.Instance.MountProjectiles)
                                if (victim.RiderAgent != null)
                                    if ((victim.RiderAgent.IsMainAgent && HeroicHeroesSettings.Instance.MainProjectiles ||
                                        (victim.RiderAgent.IsHero && HeroicHeroesSettings.Instance.HeroProjectiles) ||
                                        HeroicHeroesSettings.Instance.AIProjectiles) &&
                                        checkFriendly(HeroicHeroesSettings.Instance.FriendlyProjectiles, victim.RiderAgent)
                                        ) return false;
                        }
                        else if (victim.IsMainAgent)
                        {
                            if (HeroicHeroesSettings.Instance.MainProjectiles)
                                return false;
                        }
                        else if (victim.IsHero)
                        {
                            if (HeroicHeroesSettings.Instance.HeroProjectiles)
                                if (checkFriendly(HeroicHeroesSettings.Instance.FriendlyProjectiles, victim))
                                    return false;
                        }
                        else if (HeroicHeroesSettings.Instance.AIProjectiles && checkFriendly(HeroicHeroesSettings.Instance.FriendlyProjectiles, victim))
                            return false;
                }
            }
            catch (Exception arg)
            {
                MessageBox.Show(string.Format("Error with RegisterBlow Prefix:\n\n{0}", arg));
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch("RegisterBlow")]
        public static void Postfix(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData)
        {
            try
            {
                if (attacker == null || victim == null)
                {
                    return;
                }
                if (!BlowWillDoDamageToVictim(attacker, victim, b, ref collisionData) || victim.Health > 0f)
                {
                    return;
                }
                float toAdd = 0f;
                if (attacker.IsMainAgent)
                    toAdd = HeroicHeroesSettings.Instance.HealthOnKillAmount * HeroicHeroesSettings.Instance.PlayerMultiplier;
                else if (attacker.IsHero)
                {
                    if (checkFriendly(HeroicHeroesSettings.Instance.FriendlyOnly, attacker))
                        toAdd = HeroicHeroesSettings.Instance.HealthOnKillAmount * HeroicHeroesSettings.Instance.HeroMultiplier;
                }
                else if (checkFriendly(HeroicHeroesSettings.Instance.FriendlyOnly, attacker))
                    toAdd = HeroicHeroesSettings.Instance.HealthOnKillAmount * HeroicHeroesSettings.Instance.AIMultiplier;
                if (toAdd != 0f)
                {
                    attacker.Health = ((attacker.Health + toAdd < attacker.HealthLimit) ? (attacker.Health + toAdd) : attacker.HealthLimit);
                    if (attacker.HasMount)
                    {
                        toAdd = HeroicHeroesSettings.Instance.HealthOnKillAmount * HeroicHeroesSettings.Instance.MountMultiplier;
                        attacker.MountAgent.Health = ((attacker.MountAgent.Health + toAdd < attacker.MountAgent.HealthLimit) ? (attacker.MountAgent.Health + toAdd) : attacker.MountAgent.HealthLimit);
                    }
                }
            }
            catch (Exception arg)
            {
                MessageBox.Show(string.Format("Error with RegisterBlow Postfix:\n\n{0}", arg));
            }

        }
        private static bool checkFriendly(bool b, Agent a)//checks if b (setting whether to check friendly or not)
        {
            return !b || (Agent.Main != null && a.IsFriendOf(Agent.Main));
        }
        private static bool BlowWillDoDamageToVictim(Agent attacker, Agent victim, Blow b, ref AttackCollisionData collisionData)
        {
            return !collisionData.AttackBlockedWithShield && b.SelfInflictedDamage < 0 && attacker.IsEnemyOf(victim) && !victim.IsMount;
        }
    }
}
