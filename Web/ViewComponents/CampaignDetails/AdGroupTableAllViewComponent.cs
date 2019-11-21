﻿using Maximiz.Database.Querying;
using Maximiz.Mapper;
using Maximiz.Model.Entity;
using Maximiz.Repositories.Abstraction;
using Maximiz.ViewModels.CampaignDetails;
using Maximiz.ViewModels.EntityModels;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Maximiz.ViewComponents.CampaignDetails
{

    /// <summary>
    /// View component for loading our campaign tables async.
    /// </summary>
    public class AdGroupTableAllViewComponent : ViewComponent
    {

        /// <summary>
        /// Data store for ad groups.
        /// </summary>
        private readonly IAdGroupRepository _adGroupRepository;

        /// <summary>
        /// Maps our ad groups.
        /// </summary>
        private readonly IMapper<AdGroupWithStats, AdGroupModel> _mapperAdGroups;

        /// <summary>
        /// Constructor for dependency injection.
        /// </summary>
        public AdGroupTableAllViewComponent(IAdGroupRepository adGroupRepository,
             IMapper<AdGroupWithStats, AdGroupModel> mapperAdGroups)
        {
            _adGroupRepository = adGroupRepository;
            _mapperAdGroups = mapperAdGroups;
        }

        /// <summary>
        /// Invokes this view component and fetches a given query from the data
        /// store.
        /// TODO Who is going to translate the query object?
        /// </summary>
        /// <param name="campaignId">The internal id of the linked campaign</param>
        /// <param name="query"><see cref="QueryAdGroupWithStats"/></param>
        /// <returns>Partial view containing a table</returns>
        public async Task<IViewComponentResult> InvokeAsync(Guid campaignId, QueryAdGroupWithStats query)
        {
            // Simulate waiting
            await Task.Delay(new Random().Next(200, 800));

            // TODO Remove assignment??
            var all = _mapperAdGroups.ConvertAll(await _adGroupRepository.GetAllAsync(query));
            var linkedIds = all.ToList().GetRange(1, 2).Select(x => x.Id); // TODO We need the campaignId here

            return View("TableRows", new AdGroupTableAllViewModel
            {
                AdGroupsAll = all,
                AdGroupIdsLinked = linkedIds,
                TotalCount = await _adGroupRepository.GetCount(query)
            });
        }

    }
}