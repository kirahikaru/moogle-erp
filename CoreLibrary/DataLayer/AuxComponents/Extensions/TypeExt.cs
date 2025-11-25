using System.Reflection;
using System.ComponentModel;

namespace DataLayer.AuxComponents.Extensions;

public static class TypeExt
{
    public static string GetDisplayName(this Type value)
    {
        try
        {
			CustomAttributeData? displayNameAttr = value.CustomAttributes?.FirstOrDefault(x => x.AttributeType == typeof(DisplayNameAttribute));

			if (displayNameAttr != null)
				return displayNameAttr.ConstructorArguments[0]!.Value!.ToString()!;
			else
				return value.Name;
		}
        catch
		{
			return "Error";
		}
    }
}
