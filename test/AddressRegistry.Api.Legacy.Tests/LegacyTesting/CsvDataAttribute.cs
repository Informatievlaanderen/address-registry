namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using CsvHelper;
    using Xunit.Sdk;

    public class CsvDataAttribute : DataAttribute
    {
        public CsvDataAttribute(string fileName)
        {
            FileName = fileName;
        }

        public string FileName { get; private set; }

        public string QueryString { get; private set; }

        public override IEnumerable<object[]> GetData(MethodInfo testMethod)
        {
            if (testMethod == null)
                throw new ArgumentNullException("testMethod");

            ParameterInfo[] pars = testMethod.GetParameters();
            return DataSource(FileName, QueryString, pars.Select(par => par.ParameterType).ToArray());
        }

        private IEnumerable<object[]> DataSource(string fileName, string selectString, Type[] parameterTypes)
        {
            //var result = new List<object[]>();
            var uniques = new HashSet<string>();
            using (CsvReader reader = new CsvReader(
                new StreamReader(
                    new FileStream(GetFullFilename(fileName), FileMode.Open)),
                new CsvHelper.Configuration.Configuration() { Delimiter = ";" }))
            {
                while (reader.Read())
                {
                    var record = reader.GetRecord<string>();
                    if (!uniques.Contains(record))
                    {
                        uniques.Add(string.Concat(reader));
                        yield return ConvertParameters(record, parameterTypes);
                    }
                    //result.Add(ConvertParameters(reader.CurrentRecord, parameterTypes));
                }

                //return result.GroupBy(fields=>string.Concat(fields)).Select(g=>g.First());
            }
        }

        private static string GetFullFilename(string filename)
        {
            string executable = new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath;
            return Path.GetFullPath(Path.Combine(Path.GetDirectoryName(executable), filename));
        }

        private static object[] ConvertParameters(string record, Type[] parameterTypes)
        {
            var values = record.Split(";");
            object[] result = new object[values.Length];

            for (int idx = 0; idx < values.Length; idx++)
                result[idx] = ConvertParameter(values[idx], idx >= parameterTypes.Length ? null : parameterTypes[idx]);

            return result;
        }

        /// <summary>
        /// Converts a parameter to its destination parameter type, if necessary.
        /// </summary>
        /// <param name="parameter">The parameter value</param>
        /// <param name="parameterType">The destination parameter type (null if not known)</param>
        /// <returns>The converted parameter value</returns>
        private static object ConvertParameter(object parameter, Type parameterType)
        {
            if ((parameter is double || parameter is float) &&
                (parameterType == typeof(int) || parameterType == typeof(int?)))
            {
                int intValue;
                string floatValueAsString = parameter.ToString();

                if (Int32.TryParse(floatValueAsString, out intValue))
                    return intValue;
            }

            return parameter;
        }
    }
}
