﻿// ----------------------------------------------------------------------------------
// Microsoft Developer & Platform Evangelism
// 
// Copyright (c) Microsoft Corporation. All rights reserved.
// 
// THIS CODE AND INFORMATION ARE PROVIDED "AS IS" WITHOUT WARRANTY OF ANY KIND, 
// EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE IMPLIED WARRANTIES 
// OF MERCHANTABILITY AND/OR FITNESS FOR A PARTICULAR PURPOSE.
// ----------------------------------------------------------------------------------
// The example companies, organizations, products, domain names,
// e-mail addresses, logos, people, places, and events depicted
// herein are fictitious.  No association with any real company,
// organization, product, domain name, email address, logo, person,
// places, or events is intended or should be inferred.
// ----------------------------------------------------------------------------------

using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.ApplicationModel.Resources.Core;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using System.Net.Http;
using Windows.Data.Json;
using Windows.ApplicationModel;
using Windows.Storage.Streams;
using System.Threading.Tasks;
using Windows.Storage;

// The data model defined by this file serves as a representative example of a strongly-typed
// model that supports notification when members are added, removed, or modified.  The property
// names chosen coincide with data bindings in the standard item templates.
//
// Applications may use this model as a starting point and build on it, or discard it entirely and
// replace it with something appropriate to their needs.

namespace TemperatureMonitorGrid
{
    /// <summary>
    /// Base class for <see cref="TemperatureDataItem"/> and <see cref="TemperatureDataGroup"/> that
    /// defines properties common to both.
    /// </summary>
    [Windows.Foundation.Metadata.WebHostHidden]
    public abstract class TemperatureDataCommon : TemperatureMonitorGrid.Common.BindableBase
    {
        internal  static Uri _baseUri = new Uri("ms-appx:///");

        public TemperatureDataCommon(String uniqueId, String title, String shortTitle, String sensorUrl)
        {
            this._uniqueId = uniqueId;
            this._title = title;
            this._shortTitle = shortTitle;
            this._sensorUrl = sensorUrl;
        }

        private string _uniqueId = string.Empty;
        public string UniqueId
        {
            get { return this._uniqueId; }
            set { this.SetProperty(ref this._uniqueId, value); }
        }

        private string _title = string.Empty;
        public string Title
        {
            get { return this._title; }
            set { this.SetProperty(ref this._title, value); }
        }

        private string _shortTitle = string.Empty;
        public string ShortTitle
        {
            get { return this._shortTitle; }
            set { this.SetProperty(ref this._shortTitle, value); }
        }

        private ImageSource _image = null;
        private String _sensorUrl = null;

        public Uri SensorUrl
        {
            get
            {
                return new Uri(TemperatureDataCommon._baseUri, this._sensorUrl); 
            }
        } 

        public ImageSource Image
        {
            get
            {
                if (this._image == null && this._sensorUrl != null)
                {
                    this._image = new BitmapImage(new Uri(TemperatureDataCommon._baseUri, this._sensorUrl));
                }
                return this._image;
            }

            set
            {
                this._sensorUrl = null;
                this.SetProperty(ref this._image, value);
            }
        }

        public void SetImage(String path)
        {
            this._image = null;
            this._sensorUrl = path;
            this.OnPropertyChanged("Image");
        }

        public string GetImageUri()
        {
            return _sensorUrl;
        }
    }

    /// <summary>
    /// Recipe item data model.
    /// </summary>
    public class TemperatureDataItem : TemperatureDataCommon
    {
        public TemperatureDataItem()
            : base(String.Empty, String.Empty, String.Empty, String.Empty)
        {
        }
        
        public TemperatureDataItem(String uniqueId, String title, String shortTitle, String imagePath, int preptime, String directions, ObservableCollection<string> ingredients, TemperatureDataGroup group)
            : base(uniqueId, title, shortTitle, imagePath)
        {
            this._preptime = preptime;
            this._directions = directions;
            this._ingredients = ingredients;
            this._group = group;
        }

        private int _preptime = 0;
        public int PrepTime
        {
            get { return this._preptime; }
            set { this.SetProperty(ref this._preptime, value); }
        }
        
        private string _directions = string.Empty;
        public string Directions
        {
            get { return this._directions; }
            set { this.SetProperty(ref this._directions, value); }
        }

        private ObservableCollection<string> _ingredients;
        public ObservableCollection<string> Ingredients
        {
            get { return this._ingredients; }
            set { this.SetProperty(ref this._ingredients, value); }
        }
    
        private TemperatureDataGroup _group;
        public TemperatureDataGroup Group
        {
            get { return this._group; }
            set { this.SetProperty(ref this._group, value); }
        }

        private ImageSource _tileImage;
        private string _tileImagePath;

        public Uri TileImagePath
        {
            get
            {
                return new Uri(TemperatureDataCommon._baseUri, this._tileImagePath);
            }
        }
        
        public ImageSource TileImage
        {
            get
            {
                if (this._tileImage == null && this._tileImagePath != null)
                {
                    this._tileImage = new BitmapImage(new Uri(TemperatureDataCommon._baseUri, this._tileImagePath));
                }
                return this._tileImage;
            }
            set
            {
                this._tileImagePath = null;
                this.SetProperty(ref this._tileImage, value);
            }
        }

        public void SetTileImage(String path)
        {
            this._tileImage = null;
            this._tileImagePath = path;
            this.OnPropertyChanged("TileImage");
        }
    }

    /// <summary>
    /// Recipe group data model.
    /// </summary>
    public class TemperatureDataGroup : TemperatureDataCommon
    {
        public TemperatureDataGroup()
            : base(String.Empty, String.Empty, String.Empty, String.Empty)
        {
        }
        
        public TemperatureDataGroup(String uniqueId, String title, String shortTitle, String imagePath, String description)
            : base(uniqueId, title, shortTitle, imagePath)
        {
        }

        private ObservableCollection<TemperatureDataItem> _items = new ObservableCollection<TemperatureDataItem>();
        public ObservableCollection<TemperatureDataItem> Items
        {
            get { return this._items; }
        }

        public IEnumerable<TemperatureDataItem> TopItems
        {
            // Provides a subset of the full items collection to bind to from a GroupedItemsPage
            // for two reasons: GridView will not virtualize large items collections, and it
            // improves the user experience when browsing through groups with large numbers of
            // items.
            //
            // A maximum of 12 items are displayed because it results in filled grid columns
            // whether there are 1, 2, 3, 4, or 6 rows displayed
            get { return this._items.Take(12); }
        }

        private string _description = string.Empty;
        public string Description
        {
            get { return this._description; }
            set { this.SetProperty(ref this._description, value); }
        }

        private ImageSource _groupImage;
        private string _groupImagePath;  

        public ImageSource GroupImage
        {
            get
            {
                if (this._groupImage == null && this._groupImagePath != null)
                {
                    this._groupImage = new BitmapImage(new Uri(TemperatureDataCommon._baseUri, this._groupImagePath));
                }
                return this._groupImage;
            }
            set
            {
                this._groupImagePath = null;
                this.SetProperty(ref this._groupImage, value);
            }
        }

        public int RecipesCount
        {
            get
            {
                return this.Items.Count; 
            } 
        } 

        public void SetGroupImage(String path)
        {
            this._groupImage = null;
            this._groupImagePath = path;
            this.OnPropertyChanged("GroupImage");
        }
    }

    /// <summary>
    /// Creates a collection of groups and items.
    /// </summary>
    public sealed class TemperatureDataSource
    {
        //public event EventHandler RecipesLoaded;

        private static TemperatureDataSource _recipeDataSource = new TemperatureDataSource();
        
        private ObservableCollection<TemperatureDataGroup> _allGroups = new ObservableCollection<TemperatureDataGroup>();
        public ObservableCollection<TemperatureDataGroup> AllGroups
        {
            get { return this._allGroups; }
        }

        public static IEnumerable<TemperatureDataGroup> GetGroups(string uniqueId)
        {
            if (!uniqueId.Equals("AllGroups")) throw new ArgumentException("Only 'AllGroups' is supported as a collection of groups");

            return _recipeDataSource.AllGroups;
        }

        public static TemperatureDataGroup GetGroup(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _recipeDataSource.AllGroups.Where((group) => group.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static TemperatureDataItem GetItem(string uniqueId)
        {
            // Simple linear search is acceptable for small data sets
            var matches = _recipeDataSource.AllGroups.SelectMany(group => group.Items).Where((item) => item.UniqueId.Equals(uniqueId));
            if (matches.Count() == 1) return matches.First();
            return null;
        }

        public static async Task LoadRemoteDataAsync()
        {
            // Retrieve recipe data from Azure
            var client = new HttpClient();
            client.MaxResponseContentBufferSize = 1024 * 1024; // Read up to 1 MB of data
            var response = await client.GetAsync(new Uri("http://contosorecipes8.blob.core.windows.net/AzureRecipesRP"));
            var result = await response.Content.ReadAsStringAsync();

            // Parse the JSON recipe data
            var recipes = JsonArray.Parse(result);

            // Convert the JSON objects into RecipeDataItems and RecipeDataGroups
            CreateRecipesAndRecipeGroups(recipes);
        }

        public static async Task LoadLocalDataAsync()
        {
            // Retrieve recipe data from Recipes.txt
            var file = await Package.Current.InstalledLocation.GetFileAsync("Data\\Recipes.txt");
            var result = await FileIO.ReadTextAsync(file);

            // Parse the JSON recipe data
            var recipes = JsonArray.Parse(result);

            // Convert the JSON objects into RecipeDataItems and RecipeDataGroups
            CreateRecipesAndRecipeGroups(recipes);
        }

        private static void CreateRecipesAndRecipeGroups(JsonArray array)
        {
            foreach (var item in array)
            {
                var obj = item.GetObject();
                TemperatureDataItem recipe = new TemperatureDataItem();
                TemperatureDataGroup group = null;

                foreach (var key in obj.Keys)
                {
                    IJsonValue val;
                    if (!obj.TryGetValue(key, out val))
                        continue;

                    switch (key)
                    {
                        case "key":
                            recipe.UniqueId = val.GetNumber().ToString();
                            break;
                        case "title":
                            recipe.Title = val.GetString();
                            break;
                        case "shortTitle":
                            recipe.ShortTitle = val.GetString();
                            break;
                        case "preptime":
                            recipe.PrepTime = (int)val.GetNumber();
                            break;
                        case "directions":
                            recipe.Directions = val.GetString();
                            break;
                        case "ingredients":
                            var ingredients = val.GetArray();
                            var list = (from i in ingredients select i.GetString()).ToList();
                            recipe.Ingredients = new ObservableCollection<string>(list);
                            break;
                        case "backgroundImage":
                            recipe.SetImage(val.GetString());
                            break;
                        case "tileImage":
                            recipe.SetTileImage(val.GetString());
                            break;
                        case "group":
                            var recipeGroup = val.GetObject();

                            IJsonValue groupKey;
                            if (!recipeGroup.TryGetValue("key", out groupKey))
                                continue;

                            group = _recipeDataSource.AllGroups.FirstOrDefault(c => c.UniqueId.Equals(groupKey.GetString()));

                            if (group == null)
                                group = CreateRecipeGroup(recipeGroup);

                            recipe.Group = group;
                            break;
                    }
                }

                if (group != null)
                    group.Items.Add(recipe);
            }
        }
        
        private static TemperatureDataGroup CreateRecipeGroup(JsonObject obj)
        {
            TemperatureDataGroup group = new TemperatureDataGroup();

            foreach (var key in obj.Keys)
            {
                IJsonValue val;
                if (!obj.TryGetValue(key, out val))
                    continue;

                switch (key)
                {
                    case "key":
                        group.UniqueId = val.GetString();
                        break;
                    case "title":
                        group.Title = val.GetString();
                        break;
                    case "shortTitle":
                        group.ShortTitle = val.GetString();
                        break;
                    case "description":
                        group.Description = val.GetString();
                        break;
                    case "backgroundImage":
                        group.SetImage(val.GetString());
                        break;
                    case "groupImage" :
                        group.SetGroupImage(val.GetString());
                        break; 
                }
            }

            _recipeDataSource.AllGroups.Add(group);
            return group;
        }
    }
}
