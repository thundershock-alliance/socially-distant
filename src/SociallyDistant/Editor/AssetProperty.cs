﻿using System;
using System.Linq;
using System.Reflection;
using SociallyDistant.Editor.Attributes;
using SociallyDistant.Editor.PropertyEditors;
using Thundershock;
using Thundershock.Tweaker.Attributes;

namespace SociallyDistant.Editor
{
    public class AssetProperty
    {
        public string Category { get; }
        public string Name { get; }
        public string Description { get; }
        public PropertyInfo Property { get; }
        public CustomEditorAttribute CustomEditor { get; }

        public AssetProperty(PropertyInfo prop)
        {
            Property = prop;

            Name = (prop.GetCustomAttributes(true).OfType<TweakNameAttribute>().FirstOrDefault()?.Name) ?? prop.Name;
            Category = (prop.GetCustomAttributes(true).OfType<TweakCategoryAttribute>().FirstOrDefault()?.Category) ?? "Uncategorized";
            Description =
                (prop.GetCustomAttributes(true).OfType<TweakDescriptionAttribute>().FirstOrDefault()?.Description) ??
                string.Empty;

            CustomEditor = prop.GetCustomAttributes(true).OfType<CustomEditorAttribute>().FirstOrDefault();
        }

        public IAssetPropertyEditor CreateEditor(IAsset asset)
        {
            if (Property.PropertyType.IsEnum)
            {
                var enumEditor = new EnumEditor();
                enumEditor.Initialize(asset, Property);
                return enumEditor;
            }
            
            foreach (var type in ThundershockPlatform.GetAllTypes<IAssetPropertyEditor>())
            {
                if (type.GetConstructor(Type.EmptyTypes) == null)
                    continue;

                var attr = type.GetCustomAttributes(false).OfType<PropertyEditorAttribute>().FirstOrDefault();

                if (attr == null)
                    continue;

                if (attr.Type.IsAssignableFrom(this.Property.PropertyType))
                {
                    var e = (IAssetPropertyEditor) Activator.CreateInstance(type, null);

                    e.Initialize(asset, this.Property);

                    return e;
                }
            }

            var bad = new BadPropertyEditor();
            bad.Initialize(asset, this.Property);
            return bad;
        }
        
    }
}