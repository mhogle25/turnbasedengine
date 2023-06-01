using UnityEngine;
using System;
using Newtonsoft.Json;
using BF2D.Enums;
using BF2D.Game.Enums;

namespace BF2D.Game.Actions
{
    /// <summary>
    /// Gem
    /// </summary>
    [Serializable]
    public class CharacterStatsAction : Entity, IUtilityEntity
    {
        public class Info
        {
            public string message = string.Empty;
            public bool targetWasKilled = false;
            public bool targetWasRevived = false;
            public CharacterStats target = null;
            public bool failed = false;

            public string GetMessage()
            {
                if (this.failed)
                    return $"But {this.target.Name} was not affected. {Strings.DialogTextbox.BriefPause}";
                return message;
            }
        }

        [JsonProperty] private readonly string spriteID = string.Empty;
        [JsonProperty] private readonly int successRate = 100;
        [JsonProperty] private CombatAlignment? alignment = null;

        [JsonProperty] private readonly NumericProperty damage = null;
        [JsonProperty] private readonly NumericProperty directDamage = null;
        [JsonProperty] private readonly NumericProperty criticalDamage = null;
        [JsonProperty] private readonly NumericProperty psychicDamage = null;
        [JsonProperty] private readonly NumericProperty heal = null;
        [JsonProperty] private readonly NumericProperty recover = null;
        [JsonProperty] private readonly NumericProperty exert = null;
        [JsonProperty] private readonly bool resetHealth = false;
        [JsonProperty] private readonly bool resetStamina = false;
        [JsonProperty] private readonly StatusEffectProperty statusEffect = null;
        [JsonProperty] private readonly NumericProperty constitutionUp = null;
        [JsonProperty] private readonly NumericProperty enduranceUp = null;
        [JsonProperty] private readonly NumericProperty swiftnessUp = null;
        [JsonProperty] private readonly NumericProperty strengthUp = null;
        [JsonProperty] private readonly NumericProperty toughnessUp = null;
        [JsonProperty] private readonly NumericProperty willUp = null;
        [JsonProperty] private readonly NumericProperty fortuneUp = null;

        [JsonIgnore] public override string ID { get => this.id; set => this.id = value; }
        [JsonIgnore] private string id = string.Empty;
        [JsonIgnore] public string SpriteID => this.spriteID;
        [JsonIgnore] public int SuccessRate => this.successRate;
        [JsonIgnore] public CombatAlignment Alignment
        {
            get
            {
                if (this.alignment is not null)
                    return (CombatAlignment) this.alignment;

                int offensePoints = 0;
                int defensePoints = 0;
                int neutralPoints = 0;

                offensePoints += this.damage is not null ? 1 : 0;
                offensePoints += this.directDamage is not null ? 1 : 0;
                offensePoints += this.criticalDamage is not null ? 1 : 0;
                offensePoints += this.psychicDamage is not null ? 1 : 0;
                defensePoints += this.heal is not null ? 1 : 0;
                defensePoints += this.recover is not null ? 1 : 0;
                offensePoints += this.exert is not null ? 1 : 0;
                defensePoints += this.resetHealth ? 1 : 0;
                defensePoints += this.resetStamina ? 1 : 0;

                switch (this.statusEffect?.Get().Alignment)
                {
                    case CombatAlignment.Offense: offensePoints++; break;
                    case CombatAlignment.Defense: defensePoints++; break;
                    case CombatAlignment.Neutral: neutralPoints++; break;
                }

                defensePoints += this.constitutionUp is not null ? 1 : 0;
                neutralPoints += this.enduranceUp is not null ? 1 : 0;
                neutralPoints += this.swiftnessUp is not null ? 1 : 0;
                offensePoints += this.strengthUp is not null ? 1 : 0;
                defensePoints += this.toughnessUp is not null ? 1 : 0;
                neutralPoints += this.willUp is not null ? 1 : 0;
                neutralPoints += this.fortuneUp is not null ? 1 : 0;

                return CombatAlignmentSelector.Calculate(offensePoints, defensePoints, neutralPoints);
            }
        } 

        #region Public Methods
        public string GetAnimationKey()
        {
            if (this.criticalDamage is not null || this.psychicDamage is not null || this.directDamage is not null || this.damage is not null)
                return Strings.Animation.Damaged;

            return Strings.Animation.Flashing;
        }

        public Info Run(CharacterStats source, CharacterStats target)
        {
            Info info = new()
            {
                target = target
            };

            if (!Utilities.Probability.Roll(source, this.successRate))
            {
                info.failed = true;
                return info;
            }

            bool targetDeadPrevious = target.Dead;

            if (this.resetHealth)
            {
                info.message += $"{target.Name}'s {Strings.Character.Health} went up to full. {Strings.DialogTextbox.BriefPause}";
                target.ResetHealth();
            }

            if (this.resetStamina)
            {
                info.message += $"{target.Name}'s {Strings.Character.Stamina} went up to full. {Strings.DialogTextbox.BriefPause}";
                target.ResetStamina();
            }

            bool deathGem = ContainsAura(AuraType.Death);
            bool deathTarget = target.ContainsAura(AuraType.Death);

            if (this.damage is not null)
                if (deathGem && deathTarget)
                    info.message += RnHealHelper(this.damage, source, target);
                else
                    info.message += RnDamageHelper(this.damage, target.Damage, source, target, true);

            if (this.directDamage is not null)
                if (deathGem && deathTarget)
                    info.message += RnHealHelper(this.directDamage, source, target);
                else 
                    info.message += RnDamageHelper(this.directDamage, target.DirectDamage, source, target, true);

            if (this.psychicDamage is not null)
                if (deathGem && deathTarget)
                    info.message += RnHealHelper(this.psychicDamage, source, target);
                else 
                    info.message += RnDamageHelper(this.psychicDamage, target.PsychicDamage, source, target, true);

            if (this.criticalDamage is not null)
                if (deathGem && deathTarget)
                    info.message += RnHealHelper(this.criticalDamage, source, target);
                else
                    info.message += RnCriticalDamageHelper(this.criticalDamage, source, target);

            if (this.heal is not null)
                if ((deathGem && deathTarget) || (!deathGem && !deathTarget))
                    info.message += RnHealHelper(this.heal, source, target);
                else
                    info.message += RnDamageHelper(this.heal, target.Damage, source, target, false);

            if (this.recover is not null)
                info.message += $"{target.Name} recovered {this.recover.Run(source, target.Recover)} {Strings.Character.Stamina.ToLower()}. {Strings.DialogTextbox.BriefPause}";

            if (this.exert is not null)
                info.message += $"{target.Name} exerted {this.exert.Run(source, target.Exert)} {Strings.Character.Stamina.ToLower()}. {Strings.DialogTextbox.BriefPause}";

            if (this.statusEffect is not null)
                info.message += this.statusEffect.Run(source, target);

            if (this.constitutionUp is not null)
                info.message += $"{target.Name}'s {Strings.Character.Constitution} went up by {this.constitutionUp.Run(source, target.ConstitutionUp)}. {Strings.DialogTextbox.BriefPause}";

            if (this.enduranceUp is not null)
                info.message += $"{target.Name}'s {Strings.Character.Endurance} went up by {this.enduranceUp.Run(source, target.EnduranceUp)}. {Strings.DialogTextbox.BriefPause}";

            if (this.swiftnessUp is not null)
                info.message += $"{target.Name}'s {Strings.Character.Swiftness} went up by {this.swiftnessUp.Run(source, target.SwiftnessUp)}. {Strings.DialogTextbox.BriefPause}";

            if (this.strengthUp is not null)
                info.message += $"{target.Name}'s {Strings.Character.Strength} went up by {this.strengthUp.Run(source, target.StrengthUp)}. {Strings.DialogTextbox.BriefPause}";

            if (this.toughnessUp is not null)
                info.message += $"{target.Name}'s {Strings.Character.Toughness} went up by {this.toughnessUp.Run(source, target.ToughnessUp)}. {Strings.DialogTextbox.BriefPause}";

            if (this.willUp is not null)
                info.message += $"{target.Name}'s {Strings.Character.Will} went up by {this.willUp.Run(source, target.WillUp)}. {Strings.DialogTextbox.BriefPause}";

            if (this.fortuneUp is not null)
                info.message += $"{target.Name}'s {Strings.Character.Fortune} went up by {this.fortuneUp.Run(source, target.FortuneUp)}. {Strings.DialogTextbox.BriefPause}";


            info.targetWasKilled = !targetDeadPrevious && target.Dead;
            info.targetWasRevived = targetDeadPrevious && !target.Dead;

            return info;
        }

        public string TextBreakdown(CharacterStats source)
        {
            string text = string.Empty;

            if (this.damage is not null)
                text += TbNumericPropertyHelper(this.damage, source, Strings.Character.Damage, Colors.Red);

            if (this.directDamage is not null)
                text += TbNumericPropertyHelper(this.directDamage, source, Strings.Character.Damage, Colors.Red);

            if (this.criticalDamage is not null)
                text += TbNumericPropertyHelper(this.criticalDamage, source, Strings.Character.CriticalDamage, Colors.Yellow);

            if (this.psychicDamage is not null)
                text += TbNumericPropertyHelper(this.psychicDamage, source, Strings.Character.PsychicDamage + '\n', Colors.Magenta);

            if (this.heal is not null)
                text += TbNumericPropertyHelper(this.heal, source, Strings.Character.Heal, Colors.Green);

            if (this.recover is not null)
                text += TbNumericPropertyHelper(this.recover, source, Strings.Character.Recover, Colors.Cyan);

            if (this.exert is not null)
                text += TbNumericPropertyHelper(this.exert, source, Strings.Character.Exert, Colors.Blue);

            if (this.resetHealth)
                text += $"Fill {Strings.Character.Health} ({source.MaxHealth})\n";

            if (this.resetStamina)
                text += $"Fill {Strings.Character.Stamina} ({source.MaxStamina})\n";

            if (this.constitutionUp is not null)
                text += TbNumericPropertyHelper(this.constitutionUp, source, $"{Strings.Character.Constitution} Up", Colors.Orange);

            if (this.enduranceUp is not null)
                text += TbNumericPropertyHelper(this.enduranceUp, source, $"{Strings.Character.Endurance} Up", Colors.Orange);

            if (this.swiftnessUp is not null)
                text += TbNumericPropertyHelper(this.swiftnessUp, source, $"{Strings.Character.Swiftness} Up", Colors.Orange);

            if (this.strengthUp is not null)
                text += TbNumericPropertyHelper(this.strengthUp, source, $"{Strings.Character.Strength} Up", Colors.Orange);

            if (this.toughnessUp is not null)
                text += TbNumericPropertyHelper(this.toughnessUp, source, $"{Strings.Character.Toughness} Up", Colors.Orange);

            if (this.willUp is not null)
                text += TbNumericPropertyHelper(this.willUp, source, $"{Strings.Character.Will} Up", Colors.Orange);

            if (this.fortuneUp is not null)
                text += TbNumericPropertyHelper(this.fortuneUp, source, $"{Strings.Character.Fortune} Up", Colors.Orange);

            if (this.statusEffect is not null)
            {
                StatusEffect effect = this.statusEffect.Get();
                text += $"{effect?.Name}";
                if (this.statusEffect.successRate < 100)
                    text += $" <color=#{ColorUtility.ToHtmlStringRGBA(Colors.Cyan)}>{this.statusEffect.successRate}%{source.GetModifierText(CharacterStatsProperty.Luck)} chance</color>";
                if (!effect.Singleton)
                    text += ", stackable";
                text += '\n';
            }

            if (this.SuccessRate < 100)
                text += TbSuccessRateHelper(source);

            return text;
        }
        #endregion

        #region Private Methods
        private string RnDamageHelper(NumericProperty numericProperty, CalculatedAction action, CharacterStats source, CharacterStats target, bool critEnabled)
        {
            string message = string.Empty;
            if (Utilities.Probability.Roll(source, source.CurrentJob.CritChance) && !target.CritImmune && critEnabled)
                message += RnCriticalDamageHelper(numericProperty, source, target);
            else
                message += $"{target.Name} took {numericProperty.Run(source, action)} {Strings.Character.Damage.ToLower()}. {Strings.DialogTextbox.BriefPause}";
            return message;
        }

        private string RnCriticalDamageHelper(NumericProperty numericProperty, CharacterStats source, CharacterStats target)
        {
            return $"{Strings.Character.CriticalDamage}.{Strings.DialogTextbox.BriefPause} {target.Name} took {numericProperty.Run(source, target.CriticalDamage)} {Strings.Character.Damage.ToLower()}. {Strings.DialogTextbox.BriefPause}";
        }

        private string RnHealHelper(NumericProperty numericProperty, CharacterStats source, CharacterStats target)
        {
            return $"{target.Name} gained {numericProperty.Run(source, target.Heal)} {Strings.Character.Health.ToLower()}. {Strings.DialogTextbox.BriefPause}";
        }

        private string TbNumericPropertyHelper(NumericProperty numericProperty, CharacterStats source, string statsActionName, Color32 color)
        {
            string text = $"{statsActionName} {numericProperty.Number.TextBreakdown(source)}";
            foreach (CharacterStatsProperty modifier in numericProperty.Modifiers)
                text += $"<color=#{ColorUtility.ToHtmlStringRGBA(color)}>{source.GetModifierText(modifier)}</color>";
            text += '\n';
            return text;
        }

        private string TbSuccessRateHelper(CharacterStats source)
        {
            if (this.SuccessRate < 0)
                return $"Always Fails";

            return $"Success Rate <color=#{ColorUtility.ToHtmlStringRGBA(Colors.Cyan)}>{this.SuccessRate}%{source.GetModifierText(CharacterStatsProperty.Luck)}</color>\n";
        }
        #endregion
    }
}