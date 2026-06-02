using BepInEx.Configuration;
using System;
using System.Linq;

namespace CrestLoadouts
{
    internal sealed class AcceptableEnumList<T> : AcceptableValueBase where T : Enum
    {
        public AcceptableEnumList(params T[] acceptableEnums)
            : base(typeof(T))
        {
            if (acceptableEnums == null)
            {
                throw new ArgumentNullException("acceptableEnums");
            }

            if (acceptableEnums.Length == 0)
            {
                throw new ArgumentException("At least one acceptable value is needed.", "acceptableEnums");
            }

            AcceptableEnums = acceptableEnums;
        }

        private T[] AcceptableEnums { get; }

        public override object Clamp(object value)
        {
            return IsValid(value) ? value : AcceptableEnums[0];
        }

        public override bool IsValid(object value)
        {
            return value is T typedValue && AcceptableEnums.Any(acceptable => acceptable.Equals(typedValue));
        }

        public override string ToDescriptionString()
        {
            return "# Acceptable values: " + string.Join(", ", AcceptableEnums.Select(value => value.ToString()).ToArray());
        }
    }
}
