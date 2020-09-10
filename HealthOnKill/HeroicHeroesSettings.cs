using System;
using System.Xml.Serialization;
using ModLib.Definitions;
using ModLib.Definitions.Attributes;

namespace HeroicHeroes
{
	public class HeroicHeroesSettings : SettingsBase
	{

		public override string ModName
		{
			get
			{
				return "Heroic Heroes";
			}
		}


		public override string ModuleFolderName
		{
			get
			{
				return "HeroicHeroes";
			}
		}

		[XmlElement]
		public override string ID { get; set; } = "HeroicHeroesSettings";


		public static HeroicHeroesSettings Instance
		{
			get
			{
				return (HeroicHeroesSettings)SettingsDatabase.GetSettings<HeroicHeroesSettings>();
			}
		}
		[XmlElement]
		[SettingProperty("Projectiles don't show on Player", "Projectiles will stick to player with this enabled")]
		public bool MainStick { get; set; } = true;
		[XmlElement]
		[SettingProperty("Health on Kill Base Amount", 1f, 20f, "The base amount of health gained on kill.")]
		public float HealthOnKillAmount { get; set; } = 8f;
        [XmlElement]
        [SettingProperty("Health on Kill Multiplier for Player", 0f, 5f, "Multiplies base health on kill for player.")]
        public float PlayerMultiplier { get; set; } = 1f;
        [XmlElement]
        [SettingProperty("Health on Kill Multiplier for Heores",0f,5f, "Multiplies base health on kill for heroes .")]
        public float HeroMultiplier{ get; set; } = 1f;
        [XmlElement]
        [SettingProperty("Health on Kill Multiplier for AI", 0f, 5f, "Multiplies base health on kill for AI")]
        public float AIMultiplier { get; set; } = 0f;
		[XmlElement]
		[SettingProperty("Health on Kill Multiplier for Mounts", 0f, 5f, "Multiplies base health on kill for mount (only applies to characters with over 0 multiplier)")]
		public float MountMultiplier { get; set; } = 0f;
		[XmlElement]
        [SettingProperty("Health on Kill For Only Friendly troops", "With this enabled only friendly troops will heal on kills")]
        public bool FriendlyOnly { get; set; } = false;
		[XmlElement]
		[SettingProperty("Damage Multiplier for Player", 1, 10, "Multiplies damage dealt by player.")]
		public int DPlayerMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Damage Multiplier for Heroes", 1, 10, "Multiplies damage dealt by heroes .")]
		public int DHeroMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Damage Multiplier for AI", 1, 10, "Multiplies damage dealt by AI")]
		public int DAIMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Damage Multiplier Hero Exemption", "Heroes are exempt from additional damage multipliers")]
		public bool DHeroExemption { get; set; } = true;

		[XmlElement]
		[SettingProperty("Damage Multiplier Only For Friendly troops", "With this enabled only friendly troops will multiply damage dealt")]
		public bool DFriendlyOnly { get; set; } = false;
		[SettingProperty("Stagger Threshold Multiplier for Player", 1, 20, "Multiplies stagger thresholddealt by player.")]
		public int SPlayerMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Stagger Threshold Multiplier for Heroes", 1, 20, "Multiplies stagger threshold dealt by heroes .")]
		public int SHeroMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Stagger Threshold Multiplier for AI", 1, 20, "Multiplies stagger threshold for AI")]
		public int SAIMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Stagger Threshold Multiplier for Mounts", 1, 20, "Multiplies stagger threshold for Mounts whose riders have stagger threshold multipliers")]
		public int SMountMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Stagger Threshold Multiplier Only For Friendly troops", "With this enabled only friendly troops will multiply damage dealt")]
		public bool SFriendlyOnly { get; set; } = false;
		[XmlElement]
		[SettingProperty("Projectiles don't damage For Only Friendly troops", "With this enabled only friendly troops will not be damaged from projectiles")]
		public bool FriendlyProjectiles { get; set; } = false;
		[XmlElement]
		[SettingProperty("Projectiles don't damage setting for Player", "Projectiles will not damage or stick to player with this enabled")]
		public bool MainProjectiles{ get; set; } = true;
		[XmlElement]
		[SettingProperty("Projectiles don't damage setting for Heroes", "Projectiles will not damage or stick to heroes with this enabled")]
		public bool HeroProjectiles { get; set; } = true;
		[XmlElement]
		[SettingProperty("Projectiles don't damage setting for AI", "Projectiles will not damage or stick to AI with this enabled")]
		public bool AIProjectiles { get; set; } = false;
		[XmlElement]
		[SettingProperty("Projectiles don't damage Mounts", "Projectiles will not damage or stick to Mounts whose riders aren't damaged by projectiles with this enabled")]
		public bool MountProjectiles { get; set; } = false;
		[XmlElement]
		[SettingProperty("Projectiles don't damage Hero exemption", "Heroes-fired projectiles will damage the target")]
		public bool HeroProjectilesExemption { get; set; } = true;


		public const string InstanceID = "HeroicHeroesSettings";
	}
}
