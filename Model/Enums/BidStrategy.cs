﻿using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Maximiz.Model.Enums
{

    /// <summary>
    /// Used to indicate our bid strategy.
    /// </summary>
    [JsonConverter(typeof(StringEnumConverter))]
    public enum BidStrategy
    {

        /// <summary>
        /// Uses automatic optimization within the external networks.
        /// </summary>
        [EnumMember(Value = "smart")]
        Smart,

        /// <summary>
        /// Uses a fixed cpc at all times.
        /// </summary>
        [EnumMember(Value = "fixed")]
        Fixed
    }
}
