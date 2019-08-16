﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Maximiz.Model.Entity;
using Maximiz.Repositories.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Maximiz.Controllers
{
    public class CampaignController : Controller
    {
        private readonly ICampaignRepository _campaignRepo;

        public CampaignController(ICampaignRepository campaignRepo)
        {
            _campaignRepo = campaignRepo;
        }

        public IActionResult Index()
        {
            return View(_campaignRepo.GetAllCampaigns().Result);
        }

        public IActionResult Details(Guid id)
        {
            return View(_campaignRepo.GetCampaign(id).Result);
        }
    }
}