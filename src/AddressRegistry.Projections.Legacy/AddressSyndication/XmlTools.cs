namespace AddressRegistry.Projections.Legacy.AddressSyndication
{
    using System;
    using System.Collections;
    using System.Linq;
    using System.Reflection;
    using System.Xml;
    using System.Xml.Linq;
    using Be.Vlaanderen.Basisregisters.GrAr.Provenance;
    using NodaTime;

    public static class XmlTools
    {
        private static readonly Type[] WriteTypes = new[] {
            typeof(string),
            typeof(DateTime),
            typeof(Enum),
            typeof(decimal),
            typeof(Guid),
            typeof(Instant),
            typeof(LocalDate),
            typeof(LocalDateTime),
            typeof(DateTimeOffset),
            typeof(Plan),
            typeof(Organisation),
            typeof(Modification)
        };

        /// <summary>
        /// Preferred way to exclude properties
        /// </summary>
        private static readonly Type[] ExcludeTypes = new[]
        {
            typeof(Application)
        };

        /// <summary>
        /// Alternative way if property is a primitive type or included in WriteTypes.
        /// </summary>
        private static readonly string[] ExcludePropertyNames = new[]
        {
            "Operator"
        };

        private static bool IsSimpleType(this Type type) => type.IsPrimitive || WriteTypes.Contains(type);

        private static bool IsExcludedType(this Type type) => ExcludeTypes.Contains(type);

        private static bool IsExcludedPropertyName(this string propertyName) => ExcludePropertyNames.Contains(propertyName);

        public static XElement ToXml(this object input) => input.ToXml(null);

        public static XElement ToXml(this object input, string element, int? arrayIndex = null, string arrayName = null)
        {
            if (input == null)
                return null;

            if (string.IsNullOrEmpty(element))
            {
                string name = input.GetType().Name;
                element = name.Contains("AnonymousType")
                    ? "Object"
                    : arrayIndex != null
                        ? arrayName + "_" + arrayIndex
                        : name;
            }

            element = XmlConvert.EncodeName(element);
            var ret = new XElement(element);

            var type = input.GetType();
            var props = type.GetProperties();

            var elements = from prop in props
                let pType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType
                let name = XmlConvert.EncodeName(prop.Name)
                let val = pType.IsArray ? "array" : prop.GetValue(input, null)
                let value = pType.IsEnumerable()
                    ? GetEnumerableElements(prop, (IEnumerable)prop.GetValue(input, null))
                    : pType.IsSimpleType() ? new XElement(name, val) : val.ToXml(name)
                where value != null && !pType.IsExcludedType() && !name.IsExcludedPropertyName()
                select value;

            ret.Add(elements);

            return ret;
        }

        private static readonly Type[] FlatternTypes = new[] {
            typeof(string)
        };

        public static bool IsEnumerable(this Type type) => typeof(IEnumerable).IsAssignableFrom(type) && !FlatternTypes.Contains(type);

        private static XElement GetEnumerableElements(PropertyInfo info, IEnumerable input)
        {
            var name = XmlConvert.EncodeName(info.Name);

            XElement rootElement = new XElement(name);

            int i = 0;
            foreach (var v in input)
            {
                XElement childElement = v.GetType().IsSimpleType() || v.GetType().IsEnum ? new XElement(name + "_" + i, v) : ToXml(v, null, i, name);
                rootElement.Add(childElement);
                i++;
            }
            return rootElement;
        }
    }
}
