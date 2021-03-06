﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System.Runtime.Serialization;

namespace Maximiz.ViewModels.Enums
{

    /// <summary>
    /// Model enum to represent a connection type.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum ConnectionType
    {
        [EnumMember(Value = "cable")]
        Cable,

        [EnumMember(Value = "wifi")]
        Wifi,

        [EnumMember(Value = "cellular")]
        Cellular
    }
}
