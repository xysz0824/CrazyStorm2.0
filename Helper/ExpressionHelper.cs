using CrazyStorm.Expression;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace CrazyStorm
{
    class ExpressionHelper
    {
        static Dictionary<string, string> logicOperatorMap = new Dictionary<string, string>()
        { {"&", "And"}, {"|", "Or"} };
        static Dictionary<string, string> translationMap = new Dictionary<string, string>();
        static Dictionary<string, string> reverseTranslationMap = new Dictionary<string, string>();

        public static void InitializeTranslationCache()
        {
            translationMap.Clear();
            reverseTranslationMap.Clear();

            var application = App.Current;
            if (application == null) return;

            foreach (var dictionary in application.Resources.MergedDictionaries)
            {
                if (dictionary.Source == null ||
                    !dictionary.Source.OriginalString.StartsWith("Lang\\", StringComparison.Ordinal))
                    continue;

                foreach (DictionaryEntry entry in dictionary)
                {
                    var resourceKey = entry.Key as string;
                    if (resourceKey == null) continue;

                    var resourceValue = entry.Value as string;
                    if (resourceValue == null) continue;

                    translationMap[resourceKey] = resourceValue;
                    if (!reverseTranslationMap.ContainsKey(resourceValue))
                        reverseTranslationMap.Add(resourceValue, resourceKey);
                }
            }
        }

        public static string FindTranslation(string original)
        {
            foreach (var logicKV in logicOperatorMap)
            {
                if (original == logicKV.Key)
                {
                    original = logicKV.Value;
                    break;
                }
            }

            var resourceKey = $"{original}Str";
            string translated;
            if (translationMap.TryGetValue(resourceKey, out translated))
                return translated;

            return original;
        }
        public static string FindReverseTranslation(string translated)
        {
            string resourceKey;
            if (reverseTranslationMap.TryGetValue(translated, out resourceKey) &&
                resourceKey.EndsWith("Str", StringComparison.Ordinal))
                translated = resourceKey.Substring(0, resourceKey.Length - 3);

            foreach (var logicKV in logicOperatorMap)
            {
                if (translated == logicKV.Value)
                {
                    return logicKV.Key;
                }
            }
            return translated;
        }
        public static string TranslateProperty(string properyName)
        {
            string[] split = properyName.Split('.');
            string displayName = FindTranslation(split[0]);
            if (displayName != null && split.Length > 1) displayName += "." + split[1];
            return displayName;
        }
        public static string Translate(string expression)
        {
            var lexer = new Lexer();
            lexer.Load(expression);
            for (int i = 0; i < lexer.Tokens.Count; ++i)
            {
                var token = lexer.Tokens[i] as IdentifierToken;
                if (token != null)
                {
                    var translated = TranslateProperty((string)token.GetValue());
                    if (translated != null) token.SetValue(translated);
                }
            }
            return lexer.Output();
        }
        public static string ReverseTranslateProperty(string properyName)
        {
            string[] split = properyName.Split('.');
            string displayName = FindReverseTranslation(split[0]);
            if (displayName != null && split.Length > 1) displayName += "." + split[1];
            return displayName;
        }
        public static string ReverseTranslate(string expression)
        {
            var lexer = new Lexer();
            lexer.Load(expression);
            for (int i = 0; i < lexer.Tokens.Count; ++i)
            {
                var token = lexer.Tokens[i] as IdentifierToken;
                if (token != null)
                {
                    var original = ReverseTranslateProperty(token.GetValue() as string);
                    if (original != null) token.SetValue(original);
                }
            }
            return lexer.Output();
        }
    }
}
