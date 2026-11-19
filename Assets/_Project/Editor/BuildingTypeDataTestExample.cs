using System;
using Assets.ScriptableObjects.BuildingConfig;
using Assets.Scripts.Buildings;
using Assets.Scripts.Enum;

// The out-of-range type is reserved for editor tests; no game type is added.
[Serializable, BuildingTypeData((BuildingType)777)]
public sealed class BuildingTypeDataTestExample : BuildingTypeData
{
    public int Radius = 1;

    public override string[] GetValidationErrors() => Radius > 0 ? Array.Empty<string>() : new[] { "Radius must be positive." };
}

public sealed class BuildingTypeDataTestBuilding : BaseBuilding
{
    public int Radius => Config.GetTypeData<BuildingTypeDataTestExample>().Radius;
}
