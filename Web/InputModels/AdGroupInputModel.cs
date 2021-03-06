﻿using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Maximiz.Model.Entity;

namespace Maximiz.InputModels
{
    /// <summary>
    /// A model containing the user based input for a single <see cref="AdGroup"/>.
    /// </summary>
    public class AdGroupInputModel
    {

        /// <summary>
        /// Group name.
        /// </summary>
        [Required]
        [MaxLength(64)]
        public string Name { get; set; }

        /// <summary>
        /// Description for all items in the group.
        /// </summary>
        [Required]
        [MaxLength(90)]
        public string Description { get; set; }

        /// <summary>
        /// Display URL for all items in the group.
        /// </summary>
        [Required]
        [Display(Name = "Destination URL")]
        public string Url { get; set; }

        /// <summary>
        /// Ad items to create for this group.
        /// </summary>
        public List<AdItemInputModel> AdItems { get; set; }

        /// <summary>
        /// The link to the image.
        /// TODO Is this correct?
        /// </summary>
        public string ImagePath { get; set; }

    }
}
