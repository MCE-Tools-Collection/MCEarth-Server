﻿using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Newtonsoft.Json;
using ProjectEarthServerAPI.Models.Features;
using ProjectEarthServerAPI.Util;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using ProjectEarthServerAPI.Models;

namespace ProjectEarthServerAPI.Controllers
{
    // TODO: Not done. Rewards need inventory implementation, timers for smelting process, and recipeId -> recipe time checks
    [Authorize]
    public class SmeltingController : Controller
    {
        [ApiVersion("1.1")]
        [Route("1/api/v{version:apiVersion}/smelting/{slot}/start")]
        public async Task<IActionResult> PostNewSmeltingJob(int slot)
        {
            string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var stream = new StreamReader(Request.Body);
            var body = await stream.ReadToEndAsync();

            var req = JsonConvert.DeserializeObject<SmeltingRequest>(body);

            var smeltingJob = SmeltingUtils.StartSmeltingJob(authtoken, slot, req); 

            Console.WriteLine($"User with id {authtoken} initiated smelting job in slot {slot}.");

            var updateResponse = new UpdateResponse {updates = new Updates()};

            var nextStreamId = GenericUtils.GetNextStreamVersion();

            updateResponse.updates.smelting = (uint) nextStreamId;
            updateResponse.updates.inventory = (uint) nextStreamId;

            return Content(JsonConvert.SerializeObject(updateResponse), "application/json");
            //return Accepted(Content(returnUpdates, "application/json"));
        }

        [ApiVersion("1.1")]
        [Route("1/api/v{version:apiVersion}/smelting/finish/price")]
        public IActionResult GetSmeltingPrice(int slot)
        {
            TimeSpan remainingTime = TimeSpan.Parse(Request.Query["remainingTime"]);
            var returnPrice = new CraftingPriceResponse
            {
                result = new CraftingPrice {
                cost = 1,
                discount = 0,
                validTime = remainingTime
                },
                updates = new Dictionary<string, int>()
            };

            return Content(JsonConvert.SerializeObject(returnPrice), "application/json");
        }


        [ApiVersion("1.1")]
        [Route("1/api/v{version:apiVersion}/smelting/{slot}")]
        public IActionResult GetSmeltingStatus(int slot)
        {
            string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var smeltingStatus = SmeltingUtils.GetSmeltingJobInfo(authtoken, slot);

            Console.WriteLine($"User with id {authtoken} requested smelting slot {slot} status.");

            return Content(JsonConvert.SerializeObject(smeltingStatus),"application/json");
            //return Accepted(Content(returnTokens, "application/json"));
        }

        [ApiVersion("1.1")]
        [Route("1/api/v{version:apiVersion}/smelting/{slot}/collectItems")]
        public IActionResult GetCollectSmeltingItems(int slot)
        {
            string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var returnUpdates = SmeltingUtils.FinishSmeltingJob(authtoken, slot);

            Console.WriteLine($"User with id {authtoken} collected results of smelting slot {slot}.");


            return Content(JsonConvert.SerializeObject(returnUpdates), "application/json");
            //return Accepted(Content(returnTokens, "application/json"));
        }

        [ApiVersion("1.1")]
        [Route("1/api/v{version:apiVersion}/smelting/{slot}/stop")]
        public IActionResult GetStopSmeltingJob(int slot)
        {
            string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var returnUpdates = SmeltingUtils.CancelSmeltingJob(authtoken, slot);

            Console.WriteLine($"User with id {authtoken} cancelled smelting job in slot {slot}.");

            return Accepted();

            //return Content(JsonConvert.SerializeObject(returnUpdates), "application/json");
            //return Accepted(Content(returnTokens, "application/json"));
        }

        [ApiVersion("1.1")]
        [Route("1/api/v{version:apiVersion}/smelting/{slot}/unlock")]
        public IActionResult GetUnlockSmeltingSlot(int slot)
        {
            string authtoken = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var returnUpdates = SmeltingUtils.UnlockSmeltingSlot(authtoken, slot);

            Console.WriteLine($"User with id {authtoken} cancelled smelting job in slot {slot}.");

            return Content(JsonConvert.SerializeObject(returnUpdates),"application/json");
        }
    }
}