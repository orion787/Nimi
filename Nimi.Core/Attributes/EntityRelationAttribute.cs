using System;

namespace Nimi.Core.Attributes
{
    [AttributeUsage(AttributeTargets.Property)]
    public class EntityRelationAttribute : Attribute
    {
        public Type TargetType { get; }
        public EntityRelationAttribute(Type targetType)
            => TargetType = targetType;
    }
}

