namespace Shelly.Utilities.Extensions;

public class EnumExtensions
{
    public static List<string> ToNameList<TEnum>() where TEnum : struct, Enum => Enum.GetNames<TEnum>().ToList();
}