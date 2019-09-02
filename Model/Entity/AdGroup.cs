﻿using Maximiz.Model.Enums;
using System;

namespace Maximiz.Model.Entity
{
    /// <summary>
    /// Group of advertisement items.
    /// </summary>
    [Serializable]
    public class AdGroup : EntityAudit<int>
    {
        /// <summary>
        /// Group name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// URL for all items in the group.
        /// </summary>
        public string Url { get; set; }

        /// <summary>
        /// Description for all items in the group.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// The current status of the group.
        /// </summary>
        public Status Status { get; set; }
        public string StatusText => Status.GetEnumMemberName();
    }
}
