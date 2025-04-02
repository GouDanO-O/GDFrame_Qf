﻿using System;
using GAS.Runtime;
using Sirenix.OdinInspector;

namespace GDFramework_Core.GAS.RunningTime.Effect.Modifier
{
    public enum EGeOperation
    {
        [LabelText(SdfIconType.PlusLg, Text = "加")]
        Add = 0,

        [LabelText(SdfIconType.DashLg, Text = "减")]
        Minus = 3,

        [LabelText(SdfIconType.XLg, Text = "乘")]
        Multiply = 1,

        [LabelText(SdfIconType.SlashLg, Text = "除")]
        Divide = 4,

        [LabelText(SdfIconType.Pencil, Text = "替")]
        Override = 2,
    }
    
    [Flags]
    public enum ESupportedOperation : byte
    {
        None = 0,

        [LabelText(SdfIconType.PlusLg, Text = "加")]
        Add = 1 << GEOperation.Add,

        [LabelText(SdfIconType.DashLg, Text = "减")]
        Minus = 1 << GEOperation.Minus,

        [LabelText(SdfIconType.XLg, Text = "乘")]
        Multiply = 1 << GEOperation.Multiply,

        [LabelText(SdfIconType.SlashLg, Text = "除")]
        Divide = 1 << GEOperation.Divide,

        [LabelText(SdfIconType.Pencil, Text = "替")]
        Override = 1 << GEOperation.Override,

        All = Add | Minus | Multiply | Divide | Override
    }
    
    public class GameplayEffectModifier
    {
        
    }
}