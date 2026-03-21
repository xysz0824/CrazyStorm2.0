/*
 * The MIT License (MIT)
 * Copyright (c) StarX 2026
 */
using CrazyStorm.Expression;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Configuration;
using System.IO;
using System.Linq.Expressions;
using System.Runtime.InteropServices.ComTypes;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace CrazyStorm.Core
{
    public partial class File : IXmlData, IGeneratePlayData, ILoadPlayData
    {
        #region Private Members
        List<ParticleSystem> particleSystems;
        GenericContainer<FileResource> images;
        GenericContainer<FileResource> sounds;
        GenericContainer<VariableResource> globals;
        [XmlAttribute]
        int fileResourceIndex;
        [XmlAttribute]
        int particleIndex;
        #endregion

        #region Public Members
        public string ResourceDirectory = string.Empty;
        public List<ParticleSystem> ParticleSystems { get { return particleSystems; } }
        public GenericContainer<FileResource> Images { get { return images; } }
        public GenericContainer<FileResource> Sounds { get { return sounds; } }
        public GenericContainer<VariableResource> Globals { get { return globals; } }
        public int ParticleIndex { get { return particleIndex++; } }
        public int FileResourceIndex { get { return fileResourceIndex++; } }
        #endregion

        #region Constructor
        public File()
        {
            particleSystems = new List<ParticleSystem>();
            images = new GenericContainer<FileResource>();
            sounds = new GenericContainer<FileResource>();
            globals = new GenericContainer<VariableResource>();
        }
        public File(string defaultParticleSystemName, string defaultLayerName, string defaultBodyPositionName) : this()
        {
            particleSystems.Add(new ParticleSystem(this, defaultParticleSystemName, defaultLayerName));
        }
        #endregion

        #region Public Methods
        public void UpdateResource()
        {
            foreach (var item in images) item.CheckValid();
            foreach (var item in sounds) item.CheckValid();
            foreach (var item in globals) item.CheckValid();
        }
        public object Clone()
        {
            throw new NotImplementedException();
        }
        public XmlElement BuildFromXml(XmlElement node)
        {
            XmlHelper.BuildFromFields(this, node);
            //particleSystems
            XmlHelper.BuildFromObjectList(particleSystems, new ParticleSystem(this, ""), node, "ParticleSystems");
            //images
            XmlHelper.BuildFromObjectList(images, new FileResource(this, 0, "", ""), node, "Images");
            //sounds
            XmlHelper.BuildFromObjectList(sounds, new FileResource(this, 0, "", ""), node, "Sounds");
            //globals
            XmlHelper.BuildFromObjectList(globals, new VariableResource(int.MinValue, ""), node, "Globals");
            foreach (var particleSystem in particleSystems)
            {
                foreach (var layer in particleSystem.Layers)
                {
                    foreach (var component in layer.Components)
                    {
                        component.System = particleSystem;
                        component.Globals = globals;
                    }
                }
            }
            return node;
        }
        public XmlElement StoreAsXml(XmlDocument doc, XmlElement node)
        {
            XmlHelper.StoreFields(this, doc, node);
            //particleSystems
            XmlHelper.StoreObjectList(particleSystems, doc, node, "ParticleSystems");
            //images
            XmlHelper.StoreObjectList(images, doc, node, "Images");
            //sounds
            XmlHelper.StoreObjectList(sounds, doc, node, "Sounds");
            //globals
            XmlHelper.StoreObjectList(globals, doc, node, "Globals");            
            return node;
        }
        public static bool CheckVersion(string filePath)
        {
            if (IsCS1(filePath)) return true;
            var doc = new XmlDocument();
            doc.Load(filePath);
            var root = (XmlElement)doc.SelectSingleNode(VersionInfo.AppName.Replace(" ", ""));
            if (root == null) throw new XmlException();
            else
            {
                if (!root.HasAttribute("version"))
                {
                    throw new System.IO.FileLoadException("FileDataError");
                }
                string version = root.GetAttribute("version");
                return VersionInfo.PlayVersion == version;
            }
        }
        public void RebuildComponentTree()
        {
            foreach (var particleSystem in ParticleSystems) particleSystem.RebuildComponentTree();
        }
        public void RebuildObjectReference()
        {
            foreach (var particleSystem in ParticleSystems)
            {
                particleSystem.RebuildDistortImageReference(Images);
                particleSystem.RebuildMaskImageReference(Images);
                //Rebuild all custom types
                foreach (var customType in particleSystem.CustomTypes)
                {
                    customType.RebuildImageReferenceFromCollection(Images);
                }
                //Collect all particle types
                var particleTypes = new List<ParticleType>();
                particleTypes.AddRange(ParticleType.DefaultTypes);
                particleTypes.AddRange(particleSystem.CustomTypes);
                //Rebuild components reference
                foreach (var layer in particleSystem.Layers)
                {
                    foreach (var component in layer.Components)
                    {
                        component.RebuildReferenceFromCollection();
                        //Rebuild particles reference
                        if (component is Emitter)
                        {
                            (component as Emitter).InitialTemplate.RebuildReferenceFromCollection(particleTypes);
                        }
                    }
                }
                particleSystem.RebuildDistortTypeReferences();
                particleSystem.RebuildMaskTypeReferences();
            }
        }
        public void Load(string filePath)
        {
            ResourceDirectory = Path.GetDirectoryName(filePath) + '\\';
            if (IsCS1(filePath))
            {
                ConvertFromCS1(filePath);
            }
            else
            {
                var doc = new XmlDocument();
                doc.Load(filePath);
                var root = (XmlElement)doc.SelectSingleNode(VersionInfo.AppName.Replace(" ", ""));
                if (root == null) throw new XmlException();
                else
                {
                    BuildFromXml(root);
                }
            }
            RebuildObjectReference();
            RebuildComponentTree();
            UpdateResource();
        }
        public void Save(string filePath)
        {
            ResourceDirectory = Path.GetDirectoryName(filePath) + '\\';
            var doc = new XmlDocument();
            var declaration = doc.CreateXmlDeclaration("1.0", "UTF-8", null);
            doc.AppendChild(declaration);
            var root = doc.CreateElement(VersionInfo.AppName.Replace(" ", ""));
            var version = doc.CreateAttribute("version");
            version.Value = VersionInfo.PlayVersion;
            root.Attributes.Append(version);
            StoreAsXml(doc, root);
            doc.AppendChild(root);
            doc.Save(filePath);
        }
        void CompilePropertyExpressions(PropertyContainer container)
        {
            if (container is Emitter) CompilePropertyExpressions((container as Emitter).InitialTemplate);
            Type containerType = container.GetType();
            var variables = new List<VariableResource>();
            if (container is Component) variables.AddRange((container as Component).Locals);
            variables.AddRange(Globals);
            foreach (var property in container.Properties)
            {
                if (property.Value.Expression)
                {
                    var lexer = new Lexer();
                    lexer.Load(property.Value.Value);
                    var syntaxTree = new Parser(lexer).Expression();
                    if (!SyntaxTree.CanEval(syntaxTree))
                    {
                        var compiledBytes = new List<byte>();
                        syntaxTree.Compile(containerType, null, variables, compiledBytes);
                        property.Value.CompiledExpression = compiledBytes.ToArray();
                    }
                    else
                    {
                        var value = syntaxTree.Eval(null);
                        var propertyInfo = containerType.GetProperty(property.Key);
                        //Casting
                        if (propertyInfo.PropertyType == typeof(int) && value is float)
                        {
                            value = (int)(float)value;
                        }
                        propertyInfo.GetSetMethod().Invoke(container, new object[] { value });
                        property.Value.Expression = false;
                    }
                }
            }
        }
        void CompileEventGroups(Component component)
        {
            if (component is Emitter)
            {
                var subType = (component as Emitter).InitialTemplate.GetType();
                CompileEvents(component, subType, component.ComponentEventGroups);
                CompileEvents(component, subType, (component as Emitter).ParticleEventGroups);
            }
            else if (component is EventField)
            {
                CompileEvents(component, null, component.ComponentEventGroups);
                CompileEvents(component, typeof(ParticleBase), (component as EventField).EventFieldEventGroups);
            }
            else if (component is Rebounder)
            {
                CompileEvents(component, null, component.ComponentEventGroups);
                CompileEvents(component, typeof(ParticleBase), (component as Rebounder).RebounderEventGroups);
            }
            else CompileEvents(component, null, component.ComponentEventGroups);
        }
        void CompileEvents(Component component, Type subType, GenericContainer<EventGroup> eventGroups)
        {
            var type = component.GetType();
            var variables = new List<VariableResource>();
            variables.AddRange(component.Locals);
            variables.AddRange(Globals);
            foreach (EventGroup eventGroup in eventGroups)
            {
                eventGroup.CompiledCondition = null;
                if (!string.IsNullOrEmpty(eventGroup.Condition))
                {
                    var lexer = new Expression.Lexer();
                    lexer.Load(eventGroup.Condition);
                    var syntaxTree = new Expression.Parser(lexer).Expression();
                    var compiledBytes = new List<byte>();
                    syntaxTree.Compile(type, subType, variables, compiledBytes);
                    eventGroup.CompiledCondition = compiledBytes.ToArray();
                }
                eventGroup.CompiledEvents.Clear();
                foreach (string originalEvent in eventGroup.Events)
                {
                    eventGroup.CompiledEvents.Add(EventHelper.GenerateEventData(type, subType, variables, originalEvent));
                }
            }
        }
        void Compile()
        {
            foreach (var particleSystem in ParticleSystems)
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
        public List<byte> GeneratePlayData(File file)
        {
            var fileBytes = new List<byte>();
            //images
            PlayDataHelper.GenerateObjectList(file, images, fileBytes);
            //sounds
            PlayDataHelper.GenerateObjectList(file, sounds, fileBytes);
            //globals
            PlayDataHelper.GenerateObjectList(file, globals, fileBytes);
            //particleSystems
            PlayDataHelper.GenerateObjectList(file, particleSystems, fileBytes);
            return fileBytes;
        }
        public byte[] GeneratePlayFile()
        {
            byte[] bytes = null;
            using (var stream = new MemoryStream())
            {
                var writer = new BinaryWriter(stream);
                //Play file use UTF-8 encoding
                //Write play file header
                writer.Write(PlayDataHelper.GetStringBytes("BG"));
                //Write play file version
                writer.Write(PlayDataHelper.GetStringBytes(VersionInfo.PlayVersion));
                //Write play file data
                Compile();
                writer.Write(GeneratePlayData(this).ToArray());
                bytes = stream.ToArray();
            }
            return bytes;
        }
        public void GeneratePlayFile(string fileName)
        {
            string genPath = $"{ResourceDirectory}{fileName}.bg";
            using (FileStream stream = new FileStream(genPath, FileMode.Create))
            {
                var writer = new BinaryWriter(stream);
                writer.Write(GeneratePlayFile());
            }
        }
        public void LoadPlayData(BinaryReader reader, float version)
        {
            //Images
            PlayDataHelper.ReadObjectList(Images, reader, version);
            foreach (var image in Images)
            {
                image.File = this;
                image.CheckValid();
            }
            //Sounds
            PlayDataHelper.ReadObjectList(Sounds, reader, version);
            foreach (var sound in Sounds)
            {
                sound.File = this;
                sound.CheckValid();
            }
            //Globals
            PlayDataHelper.ReadObjectList(Globals, reader, version);
            //ParticleSystems
            PlayDataHelper.ReadObjectList(ParticleSystems, reader, version);
            foreach (var particleSystem in ParticleSystems)
            {
                particleSystem.File = this;
                particleSystem.Sounds = sounds;
                foreach (var layer in particleSystem.Layers)
                {
                    foreach (var component in layer.Components)
                    {
                        component.System = particleSystem;
                        component.Globals = globals;
                    }
                }
            }
        }
        void RebuildObjectReference(File file)
        {
            foreach (var particleSystem in file.ParticleSystems)
            {
                particleSystem.RebuildDistortImageReference(file.Images);
                particleSystem.RebuildMaskImageReference(file.Images);
                //Rebuild all custom types
                foreach (var customType in particleSystem.CustomTypes)
                {
                    customType.RebuildImageReferenceFromCollection(file.Images);
                }
                //Collect all particle types
                var particleTypes = new List<ParticleType>();
                particleTypes.AddRange(ParticleType.DefaultTypes);
                particleTypes.AddRange(particleSystem.CustomTypes);
                //Rebuild components reference
                foreach (var layer in particleSystem.Layers)
                {
                    foreach (var component in layer.Components)
                    {
                        component.RebuildReferenceFromCollection();
                        //Rebuild particles reference
                        if (component is Emitter)
                        {
                            (component as Emitter).InitialTemplate.RebuildReferenceFromCollection(particleTypes);
                        }
                    }
                }
                particleSystem.RebuildDistortTypeReferences();
                particleSystem.RebuildMaskTypeReferences();
            }
        }
        public bool LoadPlayFile(byte[] bytes, string resourceDict, float baseVersion)
        {
            ResourceDirectory = resourceDict;
            var stream = new MemoryStream(bytes);
            var reader = new BinaryReader(stream);
            //Play file use UTF-8 encoding
            string header = PlayDataHelper.ReadString(reader);
            if (header == "BG")
            {
                float version = float.Parse(PlayDataHelper.ReadString(reader));
                if (version >= baseVersion)
                {
                    LoadPlayData(reader, version);
                    RebuildObjectReference(this);
                    RebuildComponentTree();
                    stream.Dispose();
                    return true;
                }
            }
            stream.Dispose();
            return false;
        }
        public bool LoadPlayFile(string filePath, float baseVersion)
        {
            var resourceDict = Path.GetDirectoryName(filePath) + '\\';
            return LoadPlayFile(System.IO.File.ReadAllBytes(filePath), resourceDict, baseVersion);
        }
        #endregion
    }
}
