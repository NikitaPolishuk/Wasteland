using System;
using System.Collections.Generic;
using System.Reflection;
using Assets.Scripts.Enum;

namespace Assets.ScriptableObjects.BuildingConfig
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class BuildingTypeDataAttribute : Attribute
    {
        public BuildingTypeDataAttribute(BuildingType buildingType) => BuildingType = buildingType;

        public BuildingType BuildingType { get; }
    }

    public static class BuildingTypeDataDefinitions
    {
        private static Dictionary<BuildingType, Type> _types;

        public static Type GetDataType(BuildingType buildingType)
        {
            if (_types == null) _types = FindDefinitions();
            return _types.TryGetValue(buildingType, out Type type) ? type : null;
        }

        private static Dictionary<BuildingType, Type> FindDefinitions()
        {
            Dictionary<BuildingType, Type> types = new Dictionary<BuildingType, Type>();
            foreach (Assembly assembly in AppDomain.CurrentDomain.GetAssemblies())
            {
                Type[] assemblyTypes;
                try { assemblyTypes = assembly.GetTypes(); }
                catch (ReflectionTypeLoadException exception) { assemblyTypes = exception.Types; }
                foreach (Type type in assemblyTypes)
                {
                    if (type == null || type.IsAbstract || type.IsSerializable == false ||
                        typeof(BuildingTypeData).IsAssignableFrom(type) == false) continue;
                    BuildingTypeDataAttribute attribute = type.GetCustomAttribute<BuildingTypeDataAttribute>();
                    if (attribute == null) continue;
                    if (type.GetConstructor(Type.EmptyTypes) == null)
                        throw new InvalidOperationException($"Type data {type.FullName} needs a public parameterless constructor.");
                    if (types.ContainsKey(attribute.BuildingType))
                        throw new InvalidOperationException($"Multiple type data definitions for {attribute.BuildingType}.");
                    types.Add(attribute.BuildingType, type);
                }
            }

            return types;
        }
    }

    [Serializable]
    public abstract class BuildingTypeData
    {
        public virtual string[] GetValidationErrors() => Array.Empty<string>();
    }
}
