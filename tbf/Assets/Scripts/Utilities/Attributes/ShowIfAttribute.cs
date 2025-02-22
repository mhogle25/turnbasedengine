using System;
using UnityEngine;

/// <summary>
/// https://stackoverflow.com/questions/58441744/how-to-enable-disable-a-list-in-unity-inspector-using-a-bool/58446816#58446816?newreg=5a0f396713544cc586c8b0556eef5f16
/// Other Script in Editor folder
/// </summary>

namespace BF2D.Attributes
{
    public enum ConditionOperator
    {
        // A field is visible/enabled only if all conditions are true.
        AND,
        // A field is visible/enabled if at least ONE condition is true.
        OR,
        NAND
    }

    public enum ActionOnConditionFail
    {
        // If condition(s) are false, don't draw the field at all.
        DontDraw,
        // If condition(s) are false, just set the field as disabled.
        JustDisable,
    }

    [AttributeUsage(AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public class ShowIfAttribute : PropertyAttribute
    {
        public ActionOnConditionFail Action { get; private set; }
        public ConditionOperator Operator { get; private set; }
        public string[] Conditions { get; private set; }

        public ShowIfAttribute(ActionOnConditionFail action, ConditionOperator conditionOperator, params string[] conditions)
        {
            Action = action;
            Operator = conditionOperator;
            Conditions = conditions;
        }
    }
}
