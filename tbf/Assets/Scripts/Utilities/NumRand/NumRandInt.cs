using Newtonsoft.Json;
using System;
using BF2D.Utilities;
using BF2D.Game.Enums;
using UnityEngine;

namespace BF2D.Game
{
    [Serializable]
    public class NumRandInt
    {
        [JsonIgnore] public string Expression => this.expression;
        [JsonIgnore] public int Value => this.value;

        [JsonProperty] private readonly string expression = string.Empty;
        [JsonProperty] private readonly int value = 0;

        [JsonIgnore] private readonly NumRand calculator = new();

        public NumRandInt(int value)
        {
            this.value = value;
        }

        public int Calculate() => Calculate(new NumRand.CalcSpecs());

        public int Calculate(CharacterStats source)
        {
            try
            {
                return Calculate(new NumRand.CalcSpecs
                {
                    modifyEveryRandOp = source.Luck,
                    termRegistry = new System.Collections.Generic.Dictionary<string, int>
                    {
                        { CharacterStatsProperty.Speed.ToString(),      source.Speed },
                        { CharacterStatsProperty.Attack.ToString(),     source.Attack },
                        { CharacterStatsProperty.Defense.ToString(),    source.Defense },
                        { CharacterStatsProperty.Focus.ToString(),      source.Focus },
                        { CharacterStatsProperty.Luck.ToString(),       source.Luck },
                        { CharacterStatsProperty.MaxHealth.ToString(),  source.MaxHealth },
                        { CharacterStatsProperty.MaxStamina.ToString(), source.MaxStamina }
                    }
                });
            }
            catch (Exception x)
            {
                Debug.LogError(x.ToString());
                return 0;
            }
        }

        public int Calculate(NumRand.CalcSpecs specs)
        {
            return this.calculator.Calculate(this.expression, specs) + this.value;
        }

        public string TextBreakdown() => TextBreakdown(new NumRand.TextSpecs { randModifierColor = Colors.Cyan });

        public string TextBreakdown(CharacterStats source)
        {
            if (source is null)
                return TextBreakdown();

            try
            {
                return TextBreakdown(new NumRand.TextSpecs
                {
                    modifyEveryRandOp = source.GetModifierText(CharacterStatsProperty.Luck),
                    randModifierColor = Colors.Cyan,
                    termRegistry = new System.Collections.Generic.Dictionary<string, string>
                    {
                        { CharacterStatsProperty.Speed.ToString(),      source.GetStatsPropertyText(CharacterStatsProperty.Speed) },
                        { CharacterStatsProperty.Attack.ToString(),     source.GetStatsPropertyText(CharacterStatsProperty.Attack) },
                        { CharacterStatsProperty.Defense.ToString(),    source.GetStatsPropertyText(CharacterStatsProperty.Defense) },
                        { CharacterStatsProperty.Focus.ToString(),      source.GetStatsPropertyText(CharacterStatsProperty.Focus) },
                        { CharacterStatsProperty.Luck.ToString(),       source.GetStatsPropertyText(CharacterStatsProperty.Luck) },
                        { CharacterStatsProperty.MaxHealth.ToString(),  source.GetStatsPropertyText(CharacterStatsProperty.MaxHealth) },
                        { CharacterStatsProperty.MaxStamina.ToString(), source.GetStatsPropertyText(CharacterStatsProperty.MaxStamina) }
                    }
                });
            }
            catch (Exception x)
            {
                Debug.LogError(x.Message);
                return string.Empty;
            }
        }

        public string TextBreakdown(NumRand.TextSpecs specs)
        {
            if (this.value != 0 && string.IsNullOrEmpty(this.expression))
                return $"{this.value}";

            if (this.value != 0)
                return $"({this.calculator.TextBreakdown(this.expression, specs)}){this.value.NonZeroToSignedString()}";

            return this.calculator.TextBreakdown(this.expression, specs);
        }
    }
}
