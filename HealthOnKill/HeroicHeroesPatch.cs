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
                                    checkFriendly(HeroicHeroesSettings.Instance.SFriendlyOnly, victimAgent.RiderAgent)
                                    )
                                {
                                    __result = HeroicHeroesSettings.Instance.SPlayerMultiplier * MissionGameModels.Current.AgentApplyDamageModel.CalculateStaggerThresholdMultiplier(victimAgent);
                                    return false;
                                }
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
                    }
                    else if (checkFriendly(HeroicHeroesSettings.Instance.SFriendlyOnly, victimAgent))
                    {
                        __result = HeroicHeroesSettings.Instance.SAIMultiplier * MissionGameModels.Current.AgentApplyDamageModel.CalculateStaggerThresholdMultiplier(victimAgent);
                        return false;
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
                    (!HeroicHeroesSettings.Instance.HeroProjectilesExemption || (attackerAgent.IsHuman && !attackerAgent.IsHero)))
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
                        if (HeroicHeroesSettings.Instance.MainProjectiles)
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
                if (!HeroicHeroesSettings.Instance.DHeroExemption || (victim.IsHuman && !victim.IsHero))
                {
                    float dam = b.InflictedDamage;
                    if (attacker.IsMainAgent)
                        dam *= HeroicHeroesSettings.Instance.DPlayerMultiplier;
                    else if (attacker.IsHero)
                    {
                        if (checkFriendly(HeroicHeroesSettings.Instance.FriendlyOnly, attacker))
                            dam *= HeroicHeroesSettings.Instance.DHeroMultiplier;
                    }
                    else if (checkFriendly(HeroicHeroesSettings.Instance.FriendlyOnly, attacker))
                        dam *= HeroicHeroesSettings.Instance.DAIMultiplier;
                    collisionData.InflictedDamage = (int)dam;
                    b.InflictedDamage = (int)dam;
                }
                if (b.IsMissile())
                {
                    if (!HeroicHeroesSettings.Instance.HeroProjectilesExemption || (attacker.IsHuman && !attacker.IsHero))
                    {
                        float dam = b.InflictedDamage;
                        if (victim.IsMount)
                        {
                            if (HeroicHeroesSettings.Instance.MountProjectiles)
                                if (victim.RiderAgent != null)
                                    if ((victim.RiderAgent.IsMainAgent && HeroicHeroesSettings.Instance.PPlayerMultiplier < 1 ||
                                        (victim.RiderAgent.IsHero && HeroicHeroesSettings.Instance.PHeroMultiplier < 1) ||
                                        HeroicHeroesSettings.Instance.PAIMultiplier < 1) &&
                                        checkFriendly(HeroicHeroesSettings.Instance.FriendlyProjectiles, victim.RiderAgent)
                                        ) dam *=HeroicHeroesSettings.Instance.PMountMultiplier;
                        }
                        else if (victim.IsMainAgent)
                        {
                            if (HeroicHeroesSettings.Instance.MainProjectiles)
                                dam *= HeroicHeroesSettings.Instance.PPlayerMultiplier;
                        }
                        else if (victim.IsHero)
                        {
                            if (HeroicHeroesSettings.Instance.HeroProjectiles)
                                if (checkFriendly(HeroicHeroesSettings.Instance.FriendlyProjectiles, victim))
                                    dam *= HeroicHeroesSettings.Instance.PHeroMultiplier;
                        }
                        else if (HeroicHeroesSettings.Instance.AIProjectiles && checkFriendly(HeroicHeroesSettings.Instance.FriendlyProjectiles, victim))
                            dam *= HeroicHeroesSettings.Instance.PAIMultiplier;
                        if ((int)dam == 0) return false;
                        else
                        {
                            b.InflictedDamage = (int)dam;
                            collisionData.InflictedDamage = (int)dam;
                        }
                    }
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
