using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.IO;
using System.Reflection;
using CrazyStorm.Core;

namespace CrazyStorm
{
    public partial class Main
    {
        #region Private Methods
        void GeneratePlayFile()
        {
            if (string.IsNullOrWhiteSpace(Core.File.CurrentDirectory))
            {
                MessageBox.Show((string)FindResource("NeedSaveFirstStr"), (string)FindResource("TipTitleStr"),
                    MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            GeneratePlayFile(filePath);
        }
        void GeneratePlayFile(string genPath)
        {
            genPath = Path.GetDirectoryName(genPath) + "\\" + fileName + ".bg";
            using (FileStream stream = new FileStream(genPath, FileMode.Create))
            {
                var writer = new BinaryWriter(stream);
                //Play file use UTF-8 encoding
                //Write play file header
                writer.Write(PlayDataHelper.GetStringBytes("BG"));
                //Write play file version
                writer.Write(PlayDataHelper.GetStringBytes(VersionInfo.PlayVersion));
                //Write play file data
                Compile();
                writer.Write(file.GeneratePlayData().ToArray());
            }
            MessageBox.Show((string)FindResource("PlayFileSavedStr"), (string)FindResource("TipTitleStr"),
                MessageBoxButton.OK, MessageBoxImage.Information);
        }
        void Compile()
        {
            foreach (var particleSystem in file.ParticleSystems)
            {
                foreach (var layer in particleSystem.Layers)
                {
                    foreach (var component in layer.Components)
                    {
                        CompilePropertyExpressions(component);
                        CompileEventGroups(component);
                    }
                }
            }
        }
        void CompilePropertyExpressions(Component component)
        {
            Type componentType = component.GetType();
            foreach (var property in component.Properties)
            {
                if (property.Value.Expression)
                {
                    var lexer = new Expression.Lexer();
                    lexer.Load(property.Value.Value);
                    var syntaxTree = new Expression.Parser(lexer).Expression();
                    if (syntaxTree.ContainType<Expression.Name>() || syntaxTree.ContainType<Expression.Call>())
                    {
                        var compiledBytes = new List<byte>();
                        syntaxTree.Compile(compiledBytes);
                        property.Value.CompiledExpression = compiledBytes.ToArray();
                    }
                    else
                    {
                        object value = syntaxTree.Eval(null);
                        componentType.GetProperty(property.Key).GetSetMethod().Invoke(component, new object[] { value });
                    }
                }
            }
        }
        void CompileEventGroups(Component component)
        {
            CompileEvents(component.ComponentEventGroups);
            if (component is Emitter)
                CompileEvents((component as Emitter).ParticleEventGroups);
            else if (component is EventField)
                CompileEvents((component as EventField).EventFieldEventGroups);
            else if (component is Rebounder)
                CompileEvents((component as Rebounder).RebounderEventGroups);
        }
        void CompileEvents(IList<EventGroup> eventGroups)
        {
            foreach (EventGroup eventGroup in eventGroups)
            {
                if (!string.IsNullOrEmpty(eventGroup.Condition))
                {
                    var lexer = new Expression.Lexer();
                    lexer.Load(eventGroup.Condition);
                    var syntaxTree = new Expression.Parser(lexer).Expression();
                    var compiledBytes = new List<byte>();
                    syntaxTree.Compile(compiledBytes);
                    eventGroup.CompiledCondition = compiledBytes.ToArray();
                }
                eventGroup.CompiledEvents.Clear();
                foreach (string originalEvent in eventGroup.OriginalEvents)
                    eventGroup.CompiledEvents.Add(EventHelper.GenerateEventData(originalEvent, (t) =>
                        {
                            var lexer = new Expression.Lexer();
                            lexer.Load(t);
                            var syntaxTree = new Expression.Parser(lexer).Expression();
                            var compiledBytes = new List<byte>();
                            syntaxTree.Compile(compiledBytes);
                            return compiledBytes.ToArray();
                        }));
            }
        }
        #endregion

        #region Window EventHandlers
        private void GeneratePlayFile_Click(object sender, RoutedEventArgs e)
        {
            GeneratePlayFile();
        }
        #endregion
    }
}
