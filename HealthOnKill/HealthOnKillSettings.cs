using System;
using System.Xml.Serialization;
using ModLib.Definitions;
using ModLib.Definitions.Attributes;

namespace HealthOnKill
{
	public class HealthOnKillSettings : SettingsBase
	{

		public override string ModName
		{
			get
			{
				return "Health on Kill";
			}
		}


		public override string ModuleFolderName
		{
			get
			{
				return "HealthOnKill";
			}
		}

		[XmlElement]
		public override string ID { get; set; } = "HealthOnKillSettings";


		public static HealthOnKillSettings Instance
		{
			get
			{
				return (HealthOnKillSettings)SettingsDatabase.GetSettings<HealthOnKillSettings>();
			}
		}
		[XmlElement]
		[SettingProperty("Projectiles don't show on Player", "Projectiles will stick to player with this enabled")]
		public bool MainStick { get; set; } = true;
		[XmlElement]
		[SettingProperty("Health on Kill Base Amount", 1f, 20f, "The base amount of health gained on kill.")]
		public float HealthOnKillAmount { get; set; } = 8f;
        [XmlElement]
        [SettingProperty("Health on Kill Player Multiplier", 0f, 5f, "Multiplies base health on kill for player.")]
        public float PlayerMultiplier { get; set; } = 1f;
        [XmlElement]
        [SettingProperty("Health on Kill Hero Multiplier",0f,5f, "Multiplies base health on kill for heroes .")]
        public float HeroMultiplier{ get; set; } = 1f;
        [XmlElement]
        [SettingProperty("Health on Kill AI Multiplier", 0f, 5f, "Multiplies base health on kill for AI")]
        public float AIMultiplier { get; set; } = 0f;
		[XmlElement]
		[SettingProperty("Health on Kill Mount Multiplier", 0f, 5f, "Multiplies base health on kill for mount (only applies to characters with over 0 multiplier)")]
		public float MountMultiplier { get; set; } = 0f;
		[XmlElement]
        [SettingProperty("Health on Kill For Only Friendly troops", "With this enabled only friendly troops will heal on kills")]
        public bool FriendlyOnly { get; set; } = true;
		[XmlElement]
		[SettingProperty("Damage Multiplier for Player", 1, 5, "Multiplies damage dealt by player.")]
		public int DPlayerMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Damage Multiplier for Heroes", 1, 5, "Multiplies damage dealt by heroes .")]
		public int DHeroMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Damage Multiplier for AI", 1, 5, "Multiplies damage dealt by AI")]
		public int DAIMultiplier { get; set; } = 1;
		[XmlElement]
		[SettingProperty("Damage Multiplier For Only Friendly troops", "With this enabled only friendly troops will multiply damage dealt")]
		public bool DFriendlyOnly { get; set; } = true;
		[XmlElement]
		[SettingProperty("Projectiles don't damage For Only Friendly troops", "With this enabled only friendly troops will not be damaged from projectiles")]
		public bool FriendlyProjectiles { get; set; } = true;
		[XmlElement]
		[SettingProperty("Projectiles don't damage Player", "Projectiles will not damage or stick to player with this enabled")]
		public bool MainProjectiles{ get; set; } = true;
		[XmlElement]
		[SettingProperty("Projectiles don't damage Heroes", "Projectiles will not damage or stick to heroes with this enabled")]
		public bool HeroProjectiles { get; set; } = true;
		[XmlElement]
		[SettingProperty("Projectiles don't damage AI", "Projectiles will not damage or stick to AI with this enabled")]
		public bool AIProjectiles { get; set; } = false;
		[XmlElement]
		[SettingProperty("Projectiles don't damage Mounts", "Projectiles will not damage or stick to Mounts whose riders aren't damaged by projectiles with this enabled")]
		public bool MountProjectiles { get; set; } = false;

		public const string InstanceID = "HealthOnKillSettings";
	}
}
