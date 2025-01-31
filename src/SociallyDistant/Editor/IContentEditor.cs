﻿using System;
using SociallyDistant.Editor.PropertyEditors;
using Thundershock.Core.Rendering;
using Thundershock.Gui.Elements;

namespace SociallyDistant.Editor
{
    public interface IContentEditor
    {
        bool ShowEditor { get; set; }
        string DataDirectory { get; }
        GraphicsProcessor Graphics { get; }
        
        Visibility OverlayVisibility { get; set; }
        
        string ImageSelectTitle { get; set; }
        
        void UpdateMenu();

        void Error(string message);

        void UpdateGoodies(AssetInfo info);
        void ExpandGoodieCategory(AssetInfo info);
        void SelectGoodie(IAsset asset);
        void UpdateGoodieLists();
        bool AskForFolder(string title, out string folder);
        void AddCategory(string name);
        void AddEditItem(string category, string name, string desc, IAssetPropertyEditor editor);
        void AddEditAction(string category, string name, string description, Action action);
        void ClearCategories();

        void SetCustomViewElement(Element element);
        void ShowImageSelect(Action<ImageAsset> callback);
    }
}