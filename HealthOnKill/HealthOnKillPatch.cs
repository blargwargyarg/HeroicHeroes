using System;
using System.Runtime.CompilerServices;
using HarmonyLib;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;

namespace HealthOnKill
{
    [HarmonyPatch(typeof(Mission))]
    internal static class HealthOnKillPatch
    {
        [HarmonyPrefix]
        [HarmonyPatch("HandleMissileCollisionReaction")]
        public static bool Prefix1(int missileIndex, ref Mission.MissileCollisionReaction collisionReaction,
            MatrixFrame attachLocalFrame, Agent attackerAgent, Agent attachedAgent, bool attachedToShield,
            sbyte attachedBoneIndex, MissionObject attachedMissionObject, Vec3 bounceBackVelocity, Vec3 bounceBackAngularVelocity, int forcedSpawnIndex)
        {
            if (attachedAgent != null)
            {
                if (attachedAgent.IsMount && HealthOnKillSettings.Instance.MountProjectiles && attachedAgent.RiderAgent != null)
                {
                    if ((attachedAgent.RiderAgent.IsMainAgent && HealthOnKillSettings.Instance.MainProjectiles ||
                        (attachedAgent.RiderAgent.IsHero && HealthOnKillSettings.Instance.HeroProjectiles) ||
                        HealthOnKillSettings.Instance.AIProjectiles) &&
                        (!HealthOnKillSettings.Instance.FriendlyProjectiles || (Agent.Main != null && attachedAgent.RiderAgent.IsFriendOf(Agent.Main)))
                        ) collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
                }
                else if (attachedAgent.IsMainAgent && (HealthOnKillSettings.Instance.MainProjectiles || HealthOnKillSettings.Instance.MainStick))
                    collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
                else if (attachedAgent.IsHero && HealthOnKillSettings.Instance.HeroProjectiles)
                {
                    if (!HealthOnKillSettings.Instance.FriendlyOnly || (Agent.Main != null && attachedAgent.IsFriendOf(Agent.Main)))
                        collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
                }
                else if (HealthOnKillSettings.Instance.AIProjectiles && !HealthOnKillSettings.Instance.FriendlyOnly || (Agent.Main != null && attachedAgent.IsFriendOf(Agent.Main)))
                    collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
            }
            return true;
        }
        [HarmonyPrefix]
        [HarmonyPatch("RegisterBlow")]
        private static bool Prefix2(Agent attacker, Agent victim, GameEntity realHitEntity, ref Blow b, ref AttackCollisionData collisionData)
        {
            if (attacker == null || victim == null) return true;
            if (attacker.IsMainAgent)
                b.InflictedDamage *= HealthOnKillSettings.Instance.DPlayerMultiplier;
            else if (attacker.IsHero)
            {
                if (!HealthOnKillSettings.Instance.FriendlyOnly || (Agent.Main != null && attacker.IsFriendOf(Agent.Main)))
                    b.InflictedDamage *= HealthOnKillSettings.Instance.DHeroMultiplier;
            }
            else if (!HealthOnKillSettings.Instance.FriendlyOnly || (Agent.Main != null && attacker.IsFriendOf(Agent.Main)))
                b.InflictedDamage *= HealthOnKillSettings.Instance.DAIMultiplier;
            if (b.IsMissile())
            {
                if (victim.IsMount && HealthOnKillSettings.Instance.MountProjectiles && victim.RiderAgent != null)
                {
                    if ((victim.RiderAgent.IsMainAgent && HealthOnKillSettings.Instance.MainProjectiles ||
                        (victim.RiderAgent.IsHero && HealthOnKillSettings.Instance.HeroProjectiles) ||
                        HealthOnKillSettings.Instance.AIProjectiles) &&
                        (!HealthOnKillSettings.Instance.FriendlyProjectiles || (Agent.Main != null && victim.RiderAgent.IsFriendOf(Agent.Main)))
                        ) return false;
                }
                else if (victim.IsMainAgent && HealthOnKillSettings.Instance.MainProjectiles)
                    return false;
                else if (victim.IsHero && HealthOnKillSettings.Instance.HeroProjectiles)
                {
                    if (!HealthOnKillSettings.Instance.FriendlyProjectiles || (Agent.Main != null && victim.IsFriendOf(Agent.Main)))
                        return false;
                }
                else if (HealthOnKillSettings.Instance.AIProjectiles && !HealthOnKillSettings.Instance.FriendlyProjectiles || (Agent.Main != null && victim.IsFriendOf(Agent.Main)))
                    return false;
            }
            return true;
        }
        [HarmonyPostfix]
        [HarmonyPatch("RegisterBlow")]
        private static void Postfix(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData)
        {
            if (attacker == null || victim == null)
            {
                return;
            }
            if (!HealthOnKillPatch.BlowWillDoDamageToVictim(attacker, victim, b, ref collisionData) || victim.Health > 0f)
            {
                return;
            }
            float toAdd = 0f;
            if (attacker.IsMainAgent)
                toAdd = HealthOnKillSettings.Instance.HealthOnKillAmount * HealthOnKillSettings.Instance.PlayerMultiplier;
            else if (attacker.IsHero)
            {
                if (!HealthOnKillSettings.Instance.FriendlyOnly || (Agent.Main != null && attacker.IsFriendOf(Agent.Main)))
                    toAdd = HealthOnKillSettings.Instance.HealthOnKillAmount * HealthOnKillSettings.Instance.HeroMultiplier;
            }
            else if (!HealthOnKillSettings.Instance.FriendlyOnly || (Agent.Main != null && attacker.IsFriendOf(Agent.Main)))
                toAdd = HealthOnKillSettings.Instance.HealthOnKillAmount * HealthOnKillSettings.Instance.AIMultiplier;
            if (toAdd != 0f)
            {
                attacker.Health = ((attacker.Health + toAdd < attacker.HealthLimit) ? (attacker.Health + toAdd) : attacker.HealthLimit);
                if (attacker.HasMount)
                {
                    toAdd = HealthOnKillSettings.Instance.HealthOnKillAmount * HealthOnKillSettings.Instance.MountMultiplier;
                    attacker.MountAgent.Health = ((attacker.MountAgent.Health + toAdd < attacker.MountAgent.HealthLimit) ? (attacker.MountAgent.Health + toAdd) : attacker.MountAgent.HealthLimit);
                }
            }

        }
        private static bool BlowWillDoDamageToVictim(Agent attacker, Agent victim, Blow b, ref AttackCollisionData collisionData)
        {
            return !collisionData.AttackBlockedWithShield && b.SelfInflictedDamage < 0 && attacker.IsEnemyOf(victim) && !victim.IsMount;
        }
    }
}
