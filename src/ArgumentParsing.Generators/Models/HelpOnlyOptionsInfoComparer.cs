using ArgumentParsing.Generators.Models;

public sealed class HelpOnlyOptionsInfoComparer : IEqualityComparer<OptionsInfo>
{
    public static HelpOnlyOptionsInfoComparer Instance = new();

    private HelpOnlyOptionsInfoComparer()
    {
    }

    bool IEqualityComparer<OptionsInfo>.Equals(OptionsInfo x, OptionsInfo y)
    {
        if (x.QualifiedTypeName != y.QualifiedTypeName ||
            x.OptionInfos.Length != y.OptionInfos.Length ||
            x.ParameterInfos.Length != y.ParameterInfos.Length ||
            x.RemainingParametersInfo is null ^ y.RemainingParametersInfo is null ||
            x.HelpTextGeneratorInfo is null ^ y.HelpTextGeneratorInfo is null)
        {
            return false;
        }

        if (x.RemainingParametersInfo is not null &&
            x.RemainingParametersInfo.HelpDescription != y.RemainingParametersInfo!.HelpDescription)
        {
            return false;
        }

        if (x.HelpTextGeneratorInfo is not null && x.HelpTextGeneratorInfo != y.HelpTextGeneratorInfo)
        {
            return false;
        }

        for (var i = 0; i < x.OptionInfos.Length; i++)
        {
            var xInfo = x.OptionInfos[i];
            var yInfo = y.OptionInfos[i];

            if (xInfo.ShortName != yInfo.ShortName ||
                xInfo.LongName != yInfo.LongName ||
                xInfo.IsRequired != yInfo.IsRequired ||
                xInfo.HelpDescription != yInfo.HelpDescription)
            {
                return false;
            }
        }

        for (var i = 0; i < x.ParameterInfos.Length; i++)
        {
            var xInfo = x.ParameterInfos[i];
            var yInfo = y.ParameterInfos[i];

            if (xInfo.Name != yInfo.Name ||
                xInfo.IsRequired != yInfo.IsRequired ||
                xInfo.HelpDescription != yInfo.HelpDescription)
            {
                return false;
            }
        }

        return true;
    }

    int IEqualityComparer<OptionsInfo>.GetHashCode(OptionsInfo obj)
    {
        var mainHashCode = new HashCode();

        mainHashCode.Add(obj.QualifiedTypeName);

        foreach (var optionInfo in obj.OptionInfos)
        {
            var optionInfoHash = HashCode.Combine(optionInfo.ShortName, optionInfo.LongName, optionInfo.IsRequired, optionInfo.HelpDescription);
            mainHashCode.Add(optionInfoHash);
        }

        foreach (var parameterInfo in obj.ParameterInfos)
        {
            var parameterInfoHash = HashCode.Combine(parameterInfo.Name, parameterInfo.IsRequired, parameterInfo.HelpDescription);
            mainHashCode.Add(parameterInfoHash);
        }

        mainHashCode.Add(obj.RemainingParametersInfo?.HelpDescription);
        mainHashCode.Add(obj.HelpTextGeneratorInfo);

        return mainHashCode.ToHashCode();
    }
}
