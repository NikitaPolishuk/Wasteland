using System;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Enum;

public static class BuildingTypeDataEditor
{
    public static Type GetDataType(BuildingType buildingType)
    {
        return BuildingTypeDataDefinitions.GetDataType(buildingType);
    }

    public static bool TrySetData(BuildingConfig config, BuildingTypeData data, out string error)
    {
        if (config == null)
        {
            error = "Select a saved building configuration.";
            return false;
        }

        Type expectedType = GetDataType(config.BuildingType);
        if (data == null || data.GetType() != expectedType)
        {
            error = $"Data does not match the definition for {config.BuildingType}.";
            return false;
        }

        return BuildingConfigEditor.TryApply(config, "Change building type settings", candidate => candidate.SetTypeData(data), out error);
    }
}
