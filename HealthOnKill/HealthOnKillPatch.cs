using System;
using System.Windows.Forms;
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
            try
            {
                if (attachedAgent != null)
                {
                    if (attachedAgent.IsMount)
                    {
                        if (HealthOnKillSettings.Instance.MountProjectiles)
                            if (attachedAgent.RiderAgent != null)
                                if ((attachedAgent.RiderAgent.IsMainAgent && HealthOnKillSettings.Instance.MainProjectiles ||
                                    (attachedAgent.RiderAgent.IsHero && HealthOnKillSettings.Instance.HeroProjectiles) ||
                                    HealthOnKillSettings.Instance.AIProjectiles) &&
                                    checkFriendly(HealthOnKillSettings.Instance.FriendlyProjectiles, attachedAgent.RiderAgent)
                                    ) collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
                    }
                    else if (attachedAgent.IsMainAgent)
                    {
                        if (HealthOnKillSettings.Instance.MainProjectiles || HealthOnKillSettings.Instance.MainStick)
                            collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;
                    }
                    else if (attachedAgent.IsHero)
                    {
                        if (HealthOnKillSettings.Instance.HeroProjectiles)
                            if (checkFriendly(HealthOnKillSettings.Instance.FriendlyOnly, attachedAgent))
                                collisionReaction = Mission.MissileCollisionReaction.BecomeInvisible;

                    }
                    else if (HealthOnKillSettings.Instance.AIProjectiles && checkFriendly(HealthOnKillSettings.Instance.FriendlyOnly, attachedAgent))
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
        private static bool Prefix2(Agent attacker, Agent victim, GameEntity realHitEntity, ref Blow b, ref AttackCollisionData collisionData)
        {
            try
            {
                if (attacker == null || victim == null) return true;
                if (!(!collisionData.AttackBlockedWithShield && b.SelfInflictedDamage < 0 && attacker.IsEnemyOf(victim))) return true;
                if (attacker.IsMainAgent)
                    b.InflictedDamage *= HealthOnKillSettings.Instance.DPlayerMultiplier;
                else if (attacker.IsHero)
                {
                    if (checkFriendly(HealthOnKillSettings.Instance.FriendlyOnly, attacker))
                        b.InflictedDamage *= HealthOnKillSettings.Instance.DHeroMultiplier;
                }
                else if (checkFriendly(HealthOnKillSettings.Instance.FriendlyOnly, attacker))
                    b.InflictedDamage *= HealthOnKillSettings.Instance.DAIMultiplier;
                if (b.IsMissile())
                {
                    if (victim.IsMount)
                    {
                        if (HealthOnKillSettings.Instance.MountProjectiles)
                            if (victim.RiderAgent != null)
                                if ((victim.RiderAgent.IsMainAgent && HealthOnKillSettings.Instance.MainProjectiles ||
                                    (victim.RiderAgent.IsHero && HealthOnKillSettings.Instance.HeroProjectiles) ||
                                    HealthOnKillSettings.Instance.AIProjectiles) &&
                                    checkFriendly(HealthOnKillSettings.Instance.FriendlyProjectiles, victim.RiderAgent)
                                    ) return false;
                    }
                    else if (victim.IsMainAgent)
                    {
                        if (HealthOnKillSettings.Instance.MainProjectiles)
                            return false;
                    }
                    else if (victim.IsHero)
                    {
                        if (HealthOnKillSettings.Instance.HeroProjectiles)
                            if (!HealthOnKillSettings.Instance.FriendlyProjectiles || (Agent.Main != null && victim.IsFriendOf(Agent.Main)))
                                return false;
                    }
                    else if (HealthOnKillSettings.Instance.AIProjectiles && !HealthOnKillSettings.Instance.FriendlyProjectiles || (Agent.Main != null && victim.IsFriendOf(Agent.Main)))
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
        private static void Postfix(Agent attacker, Agent victim, GameEntity realHitEntity, Blow b, ref AttackCollisionData collisionData)
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
                    toAdd = HealthOnKillSettings.Instance.HealthOnKillAmount * HealthOnKillSettings.Instance.PlayerMultiplier;
                else if (attacker.IsHero)
                {
                    if (checkFriendly(HealthOnKillSettings.Instance.FriendlyOnly, attacker))
                        toAdd = HealthOnKillSettings.Instance.HealthOnKillAmount * HealthOnKillSettings.Instance.HeroMultiplier;
                }
                else if (checkFriendly(HealthOnKillSettings.Instance.FriendlyOnly, attacker))
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
