namespace AddressRegistry.Api.Legacy.Tests.LegacyTesting.Generate
{
    using System;
    using System.Collections.Generic;

    public static class Produce
    {
        //Use this flag to investigate possible problems with multiple execution of IEnumerable (for example, IQueryables being executed multiple times)
        private const bool useDeferredEnumerableExecution = false;

        private const string digits = "0123456789";
        private const string lowercaseChars = "abcdefghijklmnopqrstuvwxyz";
        private const string uppercaseChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
        private const string specialChars = "/-.,";
        private const string spaceChars = " ";

        private const int manyMaxLimit = 25;

        public static Generator<string> AlphaNumericString(int minLength = 1, int maxLength = 25)
        {
            return new Generator<string>(r =>
            {
                var alphaNumericCharacters = string.Concat(digits, lowercaseChars, uppercaseChars);
                var length = r.Next(minLength, maxLength);
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    int charIndex = r.Next(alphaNumericCharacters.Length);
                    chars[i] = alphaNumericCharacters[charIndex];
                }

                return new string(chars);
            });
        }

        public static Generator<string> AlphaNumericString(int length)
        {
            return new Generator<string>(r =>
            {
                var alphaNumericCharacters =string.Concat(digits, lowercaseChars, uppercaseChars);
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    int charIndex = r.Next(alphaNumericCharacters.Length);
                    chars[i] = alphaNumericCharacters[charIndex];
                }

                return new string(chars);
            });
        }

        public static Generator<string> UpperCaseString(int minLength = 1, int maxLength = 25)
        {
            return new Generator<string>(r =>
            {
                var length = r.Next(minLength, maxLength);
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    int charIndex = r.Next(uppercaseChars.Length);
                    chars[i] = uppercaseChars[charIndex];
                }

                return new string(chars);
            });
        }

        public static Generator<string> UpperCaseString(int length)
        {
            return new Generator<string>(r =>
            {
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    int charIndex = r.Next(uppercaseChars.Length);
                    chars[i] = uppercaseChars[charIndex];
                }

                return new string(chars);
            });
        }

        public static Generator<string> LowerCaseString(int minLength = 1, int maxLength = 25)
        {
            return new Generator<string>(r =>
            {
                var length = r.Next(minLength, maxLength);
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    int charIndex = r.Next(lowercaseChars.Length);
                    chars[i] = lowercaseChars[charIndex];
                }

                return new string(chars);
            });
        }

        public static Generator<string> LowerCaseString(int length)
        {
            return new Generator<string>(r =>
            {
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    int charIndex = r.Next(lowercaseChars.Length);
                    chars[i] = lowercaseChars[charIndex];
                }

                return new string(chars);
            });
        }

        public static Generator<string> NumericString(int length)
        {
            return new Generator<string>(r =>
            {
                var chars = new char[length];
                for (int i = 0; i < length; i++)
                {
                    int charIndex = r.Next(digits.Length);
                    chars[i] = digits[charIndex];
                }

                return new string(chars);
            });
        }

        public static Generator<int> Integer(int minValue = 0, int maxValue = int.MaxValue)
        {
            return new Generator<int>(r => r.Next(minValue, maxValue));
        }

        public static Generator<IEnumerable<T>> Many<T>(Generator<T> generator)
        {
            return new Generator<IEnumerable<T>>(r =>
            {
                int length = r.Next(2, 25);

                return Exactly(length, generator).Generate(r);

                //return ForLoop(length, () => generator.Generate(r));

                //var elements = new List<T>();
                //for (int i = 0; i < length; i++)
                //    elements.Add(generator.Generate(r));
                //return elements;
            });
        }

        public static Generator<IEnumerable<T>> Exactly<T>(int length, Generator<T> generator)
        {
            return new Generator<IEnumerable<T>>(r =>
            {
                if(useDeferredEnumerableExecution)
                    return ForLoop(length, () => generator.Generate(r));

                var elements = new List<T>();
                for (int i = 0; i < length; i++)
                    elements.Add(generator.Generate(r));
                return elements;
            });
        }

        public static Generator<IEnumerable<T>> One<T>(Generator<T> generator)
        {
            return Exactly(1, generator);
        }

        public static Generator<IEnumerable<T>> EmptyList<T>()
        {
            return new Generator<IEnumerable<T>>(r=>new T[] { });

        }

        private static IEnumerable<T> ForLoop<T>(int count, Func<T> func)
        {
            //this can be used to detect multiple enumerations
            //Mocking.LogAction($"Enumeration of IEnumerable<{typeof(T).Name}>");
            for (int i = 0; i < count; i++)
                yield return func();
        }
    }
}
