﻿/*  
 ▄▀▀▄ ▄▀▄  ▄▀▀█▄▄▄▄  ▄▀▀▄▀▀▀▄  ▄▀▄▄▄▄   ▄▀▀▄ ▄▀▀▄  ▄▀▀▄▀▀▀▄  ▄▀▀▄ ▀▀▄ 
█  █ ▀  █ ▐  ▄▀   ▐ █   █   █ █ █    ▌ █   █    █ █   █   █ █   ▀▄ ▄▀ 
▐  █    █   █▄▄▄▄▄  ▐  █▀▀█▀  ▐ █      ▐  █    █  ▐  █▀▀█▀  ▐     █   
  █    █    █    ▌   ▄▀    █    █        █    █    ▄▀    █        █   
▄▀   ▄▀    ▄▀▄▄▄▄   █     █    ▄▀▄▄▄▄▀    ▀▄▄▄▄▀  █     █       ▄▀    
█    █     █    ▐   ▐     ▐   █     ▐             ▐     ▐       █     
▐    ▐     ▐                  ▐                                 ▐   
*/

namespace SteamProfilePrivacy
{
    using System.Globalization;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Converters;

    public partial class RenderProfilePrivacy
    {
        [JsonProperty("PrivacySettings")]
        public PrivacySettings PrivacySettings { get; set; }

        [JsonProperty("eCommentPermission")]
        public int ECommentPermission { get; set; }
    }

    public partial class PrivacySettings
    {
        [JsonProperty("PrivacyProfile")]
        public int PrivacyProfile { get; set; }

        [JsonProperty("PrivacyInventory")]
        public int PrivacyInventory { get; set; }

        [JsonProperty("PrivacyInventoryGifts")]
        public int PrivacyInventoryGifts { get; set; }

        [JsonProperty("PrivacyOwnedGames")]
        public int PrivacyOwnedGames { get; set; }

        [JsonProperty("PrivacyPlaytime")]
        public int PrivacyPlaytime { get; set; }

        [JsonProperty("PrivacyFriendsList")]
        public int PrivacyFriendsList { get; set; }
    }

    public partial class RenderProfilePrivacy
    {
        public static RenderProfilePrivacy FromJson(string json) => JsonConvert.DeserializeObject<RenderProfilePrivacy>(json, SteamProfilePrivacy.Converter.Settings);
    }
    
    internal static class Converter
    {
        public static readonly JsonSerializerSettings Settings = new JsonSerializerSettings
        {
            MetadataPropertyHandling = MetadataPropertyHandling.Ignore,
            DateParseHandling = DateParseHandling.None,
            Converters =
            {
                new IsoDateTimeConverter { DateTimeStyles = DateTimeStyles.AssumeUniversal }
            },
        };
    }
}
