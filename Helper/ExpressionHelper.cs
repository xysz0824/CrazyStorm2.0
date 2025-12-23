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
        public static string FindTranslation(string original)
        {
            original = $"{original}Str";
            var merged = App.Current.Resources.MergedDictionaries;
            var lang = merged.Where(d => d.Source != null && d.Source.OriginalString.StartsWith("Lang\\"));
            foreach (var langE in lang)
            {
                foreach (DictionaryEntry e in langE)
                {
                    var resourceKey = e.Key as string;
                    if (resourceKey == null) continue;
                    var resourceValue = e.Value as string;
                    if (resourceValue == null) continue;
                    if (resourceKey == original)
                    {
                        return resourceValue;
                    }
                }
            }
            return null;
        }
        public static string FindReverseTranslation(string translated)
        {
            var merged = App.Current.Resources.MergedDictionaries;
            var lang = merged.Where(d => d.Source != null && d.Source.OriginalString.StartsWith("Lang\\"));
            foreach (var langE in lang)
            {
                foreach (DictionaryEntry e in langE)
                {
                    var resourceKey = e.Key as string;
                    if (resourceKey == null) continue;
                    var resourceValue = e.Value as string;
                    if (resourceValue == null) continue;
                    if (resourceValue == translated)
                    {
                        return resourceKey.Replace("Str", "");
                    }
                }
            }
            return null;
        }
        public static string TranslateProperty(string properyName)
        {
            string[] split = properyName.Split('.');
            string displayName = FindTranslation(split[0]);
            if (displayName != null && split.Length > 1) displayName += "." + split[1];
            else if (displayName == null) displayName = split[0];
            return displayName;
        }
        public static string Translate(string expression)
        {
            var lexer = new Lexer();
            lexer.Load(expression);
            for (int i = 0; i < lexer.Tokens.Count; ++i)
            {
                var token = lexer.Tokens[i] as IdentifierToken;
                if (token != null && !token.IsOperator)
                {
                    var translated = FindTranslation((string)token.GetValue());
                    if (translated != null) token.SetValue(translated);
                }
            }
            return lexer.Output();
        }
        public static string ReverseTranslate(string expression)
        {
            var lexer = new Lexer();
            lexer.Load(expression);
            for (int i = 0; i < lexer.Tokens.Count; ++i)
            {
                var token = lexer.Tokens[i] as IdentifierToken;
                if (token != null && !token.IsOperator)
                {
                    var original = FindReverseTranslation(token.GetValue() as string);
                    if (original != null) token.SetValue(original);
                }
            }
            return lexer.Output();
        }
    }
}
