using System;
using System.Collections.Generic;
using UnityEngine;

namespace BF2D.Utilities
{
    [Serializable]
    public class NumRand
    {
        private const char OP_RANGE = '$';
        private const char OP_ADD = '+';
        private const char OP_SUB = '-';
        private const char OP_MULT = '*';

        private readonly HashSet<char> operatorsSet = new()
        {
            NumRand.OP_RANGE,
            NumRand.OP_ADD,
            NumRand.OP_SUB,
            NumRand.OP_MULT
        };

        public class TextSpecs
        {
            public string modifyEveryRandOp = null;
            public Dictionary<string, string> termRegistry = new();
            public Color randModifierColor = Color.white;
        }

        public class CalcSpecs
        {
            public int? modifyEveryRandOp = null;
            public Dictionary<string, int> termRegistry = new();
            public bool canExceedMax = false;
            public bool canExceedMin = false;
            public bool showLogs = false;
        }

        private class FloatOp
        {
            public FloatOp(string raw)
            {
                this.raw = raw;
            }

            public float Arg0 { set { this.arg0 = value; } }
            private float? arg0 = null;
            public float Arg1 { set { this.arg1 = value; } }
            private float? arg1 = null;

            private readonly string raw = string.Empty;

            public bool AnyArgsInitialized => this.arg0 is not null || this.arg1 is not null;

            public bool FirstArgInitialized => this.arg0 is not null && this.arg1 is null;

            public int Calculate(CalcSpecs specs)
            {
                if (this.arg0 is null || this.arg1 is null)
                    throw new Exception("[NumRand:FloatOp:Calculate] Tried to calculate but one or both of the arguments were null");

                return Calculate(this.arg0.GetValueOrDefault(), this.arg1.GetValueOrDefault(), specs);
            }

            private int Calculate(float x, float y, CalcSpecs specs)
            {
                if (this.raw.Length < 1)
                    throw new Exception($"[NumRand:FloatOp:Calculate] Tried to calculate but the operation was invalid -> {this.raw}");

                if (this.raw[0] == NumRand.OP_ADD)
                {
                    return (int) (x + y);
                }
                else if (this.raw[0] == NumRand.OP_SUB)
                {
                    return (int) (x - y);
                }
                else if (this.raw[0] == NumRand.OP_MULT)
                {
                    return (int) (x * y);
                }
                else if (this.raw[0] == NumRand.OP_RANGE)
                {
                    float max = x > y ? x : y;
                    float min = x < y ? x : y;

                    int minI = (int)min;
                    int maxI = (int)max;

                    if (minI == maxI)
                        return minI;

                    int value = UnityEngine.Random.Range(minI, maxI + 1);

                    if (specs.showLogs)
                        Debug.Log($"Random Value: {value}");

                    value += specs.modifyEveryRandOp is null ? 0 : specs.modifyEveryRandOp.GetValueOrDefault();

                    if (!specs.canExceedMax && value > maxI)
                        value = maxI;

                    if (!specs.canExceedMin && value < minI)
                        value = minI;

                    if (specs.showLogs)
                        Debug.Log($"After Specs: {value}");

                    return value;
                }
                else
                {
                    throw new Exception($"[NumRand:FloatOp:Calculate] Syntax Error: The operation '{this.raw}' was invalid");
                }
            }
        }

        private class StringOp
        {
            public StringOp(string op)
            {
                this.op = op;
            }

            public string Arg0 { set { this.arg0 = value; } }
            private string arg0 = null;
            public string Arg1 { set { this.arg1 = value; } }
            private string arg1 = null;

            private readonly string op = string.Empty;

            public bool AnyArgumentsSet { get { return this.arg0 is not null || this.arg1 is not null; } }

            public bool FirstArgumentSet { get { return this.arg0 is not null && this.arg1 is null; } }

            public string TextBreakdown(TextSpecs specs)
            {
                if (this.arg0 is null || this.arg1 is null)
                    throw new Exception("[NumRand:StringOp:TextBreakdown] Tried to create a text breakdown but one or both of the arguments were null");

                return Calculate(this.arg0, this.arg1, specs);
            }

            private string Calculate(string x, string y, TextSpecs specs)
            {
                if (this.op.Length < 1)
                    throw new Exception($"[NumRand:StringOp:TextBreakdown] Tried to create a text breakdown but the operation was invalid -> {this.op}");

                if (this.op == $"{NumRand.OP_ADD}")
                {
                    return $"{x}{NumRand.OP_ADD}{y}";
                }
                else if (this.op == $"{NumRand.OP_SUB}")
                {
                    return $"{x}{NumRand.OP_SUB}{y}";
                }
                else if (this.op == $"{NumRand.OP_MULT}")
                {
                    return $"{x}{NumRand.OP_MULT}{y}";
                }
                else if (this.op == $"{NumRand.OP_RANGE}")
                {
                    if (string.IsNullOrEmpty(specs.modifyEveryRandOp))
                        return $"{x} to {y}".Colorize(specs.randModifierColor);

                    return $"{x} to {y} {specs.modifyEveryRandOp}".Colorize(specs.randModifierColor);
                }
                else
                {
                    throw new Exception($"[NumRand:StringOp:TextBreakdown] Syntax Error: The operation '{this.op}' was invalid");
                }
            }
        }

        private readonly Stack<FloatOp> opStackF = new();
        private FloatOp CurrentOpF { get { return this.opStackF.Peek(); } }

        private readonly Stack<StringOp> opStackS = new();
        private StringOp CurrentOpS { get { return this.opStackS.Peek(); } }

        public int Calculate(string expression)
        {
            return Calculate(expression, new CalcSpecs());
        }

        public string TextBreakdown(string expression)
        {
            return TextBreakdown(expression, new TextSpecs());
        }

        public int Calculate(string expression, CalcSpecs specs)
        {
            if (string.IsNullOrEmpty(expression))
                return 0;

            return CalculateParser(expression, specs);
        }

        public string TextBreakdown(string expression, TextSpecs specs)
        {
            if (string.IsNullOrEmpty(expression))
                return string.Empty;

            return TextBreakdownParser(expression, specs);
        }

        private int CalculateParser(string expression, CalcSpecs specs)
        {
            string[] operations = expression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (operations.Length < 3)
            {
                if (operations.Length == 2)
                    throw new Exception($"[NumRand:CalculateParser] Syntax Error: The expression supplied was smaller than a single operation -> {expression}");

                string value = operations[0];
                if (specs.termRegistry.ContainsKey(value))
                    return specs.termRegistry[value];

                return (int)TryParse(value);
            }

            if (!IsAnOperator(operations[0]))
                throw new Exception($"[NumRand:CalculateParser] Syntax Error: The first term of the expression must be an operator");

            this.opStackF.Push(new FloatOp(operations[0]));

            int i = 1;
            float? stagedValue = null;
            while (this.opStackF.Count > 0)
            {
                if (i >= operations.Length)
                    throw new Exception($"[NumRand:CalculateParser] Syntax Error: Incomplete operation -> {expression}");

                if (i < operations.Length && IsAnOperator(operations[i]))
                {
                    this.opStackF.Push(new FloatOp(operations[i]));
                    i++;
                    continue;
                }

                float arg = 0f;
                if (stagedValue is not null)
                {
                    arg = (float) stagedValue;
                    stagedValue = null;
                }
                else if (!specs.termRegistry.ContainsKey(operations[i]))
                {
                    try
                    {
                        arg = float.Parse(operations[i]);
                    }
                    catch
                    {
                        throw new Exception($"[NumRand:CalculateParser] Syntax Error: The provided term could not be converted into a float -> {operations[i]}");
                    }
                }

                arg = specs.termRegistry.ContainsKey(operations[i]) ? specs.termRegistry[operations[i]] : arg;

                if (!this.CurrentOpF.AnyArgsInitialized)
                {
                    this.CurrentOpF.Arg0 = arg;
                    i++;
                    continue;
                }

                if (this.CurrentOpF.FirstArgInitialized)
                {
                    this.CurrentOpF.Arg1 = arg;
                    FloatOp op = this.opStackF.Pop();
                    int value = op.Calculate(specs);
                    stagedValue = value;
                    continue;
                }
            }

            if (specs.showLogs)
                Debug.Log($"Final Value: {stagedValue}");

            return (int) stagedValue;
        }

        private string TextBreakdownParser(string expression, TextSpecs specs)
        {
            string[] operations = expression.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);

            if (operations.Length < 3)
            {
                if (operations.Length == 2)
                    throw new Exception($"[NumRand:TextBreakdownParser] Syntax Error: The expression supplied was smaller than a single operation -> {expression}");

                string value = operations[0];
                if (specs.termRegistry.ContainsKey(value))
                    return specs.termRegistry[value];

                return value;
            }

            if (!IsAnOperator(operations[0]))
                throw new Exception($"[NumRand:TextBreakdownParser] Syntax Error: The first term of the expression must be an operator");

            this.opStackS.Push(new StringOp(operations[0]));

            int i = 1;
            string stagedValue = null;
            while (this.opStackS.Count > 0)
            {
                if (i >= operations.Length)
                    throw new Exception($"[NumRand:TextBreakdownParser] Syntax Error: Incomplete operation -> {expression}");

                if (IsAnOperator(operations[i]))
                {
                    this.opStackS.Push(new StringOp(operations[i]));
                    i++;
                    continue;
                }

                string arg;
                if (stagedValue is not null)
                {
                    arg = $"({stagedValue})";
                    stagedValue = null;
                }
                else
                {
                    arg = operations[i];
                }

                arg = specs.termRegistry.ContainsKey(operations[i]) ? specs.termRegistry[operations[i]] : arg;

                if (!this.CurrentOpS.AnyArgumentsSet)
                {
                    this.CurrentOpS.Arg0 = arg;
                    i++;
                    continue;
                }

                if (this.CurrentOpS.FirstArgumentSet)
                {
                    this.CurrentOpS.Arg1 = arg;
                    StringOp op = this.opStackS.Pop();
                    string value = op.TextBreakdown(specs);
                    stagedValue = value;
                    continue;
                }
            }

            return stagedValue;

        }

        private bool IsAnOperator(string value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                if (!this.operatorsSet.Contains(value[i]))
                    return false;
            }
            return true;
        }

        private float TryParse(string value)
        {
            try
            {
                return float.Parse(value);
            }
            catch
            {
                throw new Exception($"[NumRand:TryParse] Syntax Error: The provided term could not be converted into a float -> {value}");
            }
        }
    }
}