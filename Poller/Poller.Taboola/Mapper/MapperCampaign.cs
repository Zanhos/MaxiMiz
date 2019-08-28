﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Maximiz.Model.Entity;
using Maximiz.Model.Enums;
using Poller.Helper;
using Poller.Taboola.Model;
using CampaignCore = Maximiz.Model.Entity.Campaign;
using CampaignTaboola = Poller.Taboola.Model.Campaign;

namespace Poller.Taboola.Mapper
{

    /// <summary>
    /// Our converter for campaigns.
    /// </summary>
    class MapperCampaign : IMapper<CampaignTaboola, CampaignCore>
    {

        private const string DefaultString = "default";
        private const int DefaultNumber = -1;
        private const string DefaultJson = "{}";
        private const string DefaultCampaignIdNumber = "xxxxxxxx";
        private const string DefaultCampaignIdName = "invalid-campaign-id";
        private const string DefaultCurrency = "XXX";
        private const Delivery DefaultDelivery = Delivery.Unknown;
        private const string DefaultLanguage = "NL";
        private readonly int[] DefaultLocations = new int[0];

        /// <summary>
        /// Converts our core model to taboola campaign.
        /// </summary>
        /// <param name="core">The object to convert</param>
        /// <returns>The converted object</returns>
        public CampaignTaboola Convert(CampaignCore core)
        {
            if (core == null) throw new ArgumentNullException(nameof(core));

            var result = new CampaignTaboola
            {
                Id = core.SecondaryId,
                Name = core.Name,
                Branding = core.BrandingText,
                Cpc = core.InitialCpc,
                SpendingLimit = core.Budget,
                DailyCap = core.DailyBudget,
                Spent = core.Spent,
                StartDate = core.StartDate,
                EndDate = core.EndDate,
                Utm = core.Utm,
                Note = core.Note,
            };

            CampaignDetails details = Json.Deserialize
               <CampaignDetails>(core.Details);
            PushDetails(result, details);

            return result;
        }

        /// <summary>
        /// Pushes our details to a given taboola
        /// campaign object. This will overwrite in 
        /// all cases.
        /// </summary>
        /// <param name="to">The object to push to</param>
        /// <param name="details">The extracted details</param>
        /// <return>The pushed object with extracted details</return>
        private CampaignTaboola PushDetails(CampaignTaboola to,
            CampaignDetails details)
        {
            if (to == null) throw new ArgumentNullException(nameof(to));
            if (details == null) throw new ArgumentNullException(nameof(details));

            to.Account = details.Account;
            to.DailyAdDeliveryModel = details.DailyAdDeliveryModel;
            to.PublisherBidModifier = details.PublisherBidModifier;
            to.SpendingLimitModel = details.SpendingLimitModel;
            to.CountryTargeting = details.CountryTargeting;
            to.SubCountryTargeting = details.SubCountryTargeting;
            to.PostalCodeTargeting = details.PostalCodeTargeting;
            to.ContextualTargeting = details.ContextualTargeting;
            to.PlatformTargeting = details.PlatformTargeting;
            to.OsTargeting = details.OsTargeting;
            to.ConnectionTypeTargeting = details.ConnectionTypeTargeting;
            to.CpaGoal = details.CpaGoal;
            to.BidStrategy = details.BidStrategy;
            to.TrafficAllocationMode = details.TrafficAllocationMode;
            to.ApprovalState = details.ApprovalState;
            to.Status = details.Status;
            to.Active = details.Active;

            return to;
        }

        /// <summary>
        /// Converts a taboola campaign to our core model.
        /// </summary>
        /// <param name="external">The object to convert</param>
        /// <returns>The converted object</returns>
        public CampaignCore Convert(CampaignTaboola external)
        {
            if (external == null) throw new ArgumentNullException(nameof(external));

            var result = GetDefault();

            result.SecondaryId = external.Id;
            result.Name = external.Name;
            result.BrandingText = external.Branding;
            result.InitialCpc = external.Cpc;
            result.Budget = external.SpendingLimit;
            result.DailyBudget = external.DailyCap;
            result.Spent = external.Spent;
            result.StartDate = external.StartDate;
            result.EndDate = external.EndDate;
            result.Utm = external.Utm;
            result.Note = external.Note;
            result.Details = ExtractDetailsToString(external);

            return result;
        }

        /// <summary>
        /// Extracts the details from a Taboola campaign.
        /// This then converts it to a JSON string.
        /// </summary>
        /// <param name="from">The Taboola campaign</param>
        /// <returns>The details object as JSON string</returns>
        private string ExtractDetailsToString(
            CampaignTaboola from)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));

            return Json.Serialize(new CampaignDetails
            {
                Account = from.Account,
                DailyAdDeliveryModel = from.DailyAdDeliveryModel,
                PublisherBidModifier = from.PublisherBidModifier,
                SpendingLimitModel = from.SpendingLimitModel,
                CountryTargeting = from.CountryTargeting,
                SubCountryTargeting = from.SubCountryTargeting,
                PostalCodeTargeting = from.PostalCodeTargeting,
                ContextualTargeting = from.ContextualTargeting,
                PlatformTargeting = from.PlatformTargeting,
                OsTargeting = from.OsTargeting,
                ConnectionTypeTargeting = from.ConnectionTypeTargeting,
                CpaGoal = from.CpaGoal,
                BidStrategy = from.BidStrategy,
                TrafficAllocationMode = from.TrafficAllocationMode,
                ApprovalState = from.ApprovalState,
                Status = from.Status,
                Active = from.Active
            });
        }

        /// <summary>
        /// Bulk conversion from Taboola to Core.
        /// </summary>
        /// <param name="list">Taboola list</param>
        /// <returns>Core list</returns>
        public IEnumerable<CampaignCore> ConvertAll(
            IEnumerable<CampaignTaboola> list)
        {
            IList<CampaignCore> result = new List<CampaignCore>();
            foreach (var item in list.AsParallel())
            {
                result.Add(Convert(item));
            }
            return result;
        }

        /// <summary>
        /// Bulk conversion from Core to Taboola.
        /// </summary>
        /// <param name="list">Core list</param>
        /// <returns>Taboola list</returns>
        public IEnumerable<CampaignTaboola> ConvertAll(
            IEnumerable<CampaignCore> list)
        {
            IList<CampaignTaboola> result = new List<CampaignTaboola>();
            foreach (var item in list.AsParallel())
            {
                result.Add(Convert(item));
            }
            return result;
        }

        /// <summary>
        /// Creates a default without null values.
        /// TODO Match db schema
        /// TODO Fill up
        /// </summary>
        /// <returns>Default</returns>
        private CampaignCore GetDefault()
        {
            return new CampaignCore
            {
                SecondaryId = DefaultCampaignIdNumber,
                Name = DefaultCampaignIdName,
                Language = DefaultLanguage,
                Budget = DefaultNumber,
                Delivery = DefaultDelivery,
                LocationInclude = DefaultLocations,
                LocationExclude = DefaultLocations
            };
        }
    }
}