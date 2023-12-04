using System;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace SongOrganizer.Data;

public class Enums
{
    public static string GetDescription(Enum value)
    {
        FieldInfo fi = value.GetType().GetField(value.ToString());
        DescriptionAttribute[] attr = fi.GetCustomAttributes(typeof(DescriptionAttribute), false) as DescriptionAttribute[];
        return attr != null && attr.Any() ? attr.First().Description : value.ToString();
    }
}
