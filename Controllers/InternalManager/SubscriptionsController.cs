﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs.InternalManager;
using SportComplexAPI.Models;
using SportComplexAPI.Services;

namespace SportComplexAPI.Controllers.InternalManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class SubscriptionsController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public SubscriptionsController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet("subscriptions-view")]
        public async Task<IActionResult> GetAllSubscriptions(
            string sortBy = "name",
            string order = "asc", decimal? minCost = null,
            decimal? maxCost = null, string? activities = null,
            string? visitTime = null, string? term = null
            )
        {
            var query = _context.Subscriptions
                .Include(s => s.BaseSubscription)
                    .ThenInclude(bs => bs.SubscriptionTerm)
                .Include(s => s.BaseSubscription)
                    .ThenInclude(bs => bs.SubscriptionVisitTime)
                .Include(s => s.SubscriptionActivities)
                    .ThenInclude(sa => sa.Activity)
                .AsQueryable();


            // Sorting
            query = (sortBy, order.ToLower()) switch
            {

                ("name", "asc") => query.OrderBy(s => s.subscription_name),
                ("name", "desc") => query.OrderByDescending(s => s.subscription_name),
                ("cost", "asc") => query.OrderBy(s => s.subscription_total_cost),
                ("cost", "desc") => query.OrderByDescending(s => s.subscription_total_cost),
                ("term", "asc") => query.OrderBy(s =>
                    s.BaseSubscription.SubscriptionTerm.subscription_term == "1 місяць" ? 1 :
                    s.BaseSubscription.SubscriptionTerm.subscription_term == "3 місяці" ? 3 :
                    s.BaseSubscription.SubscriptionTerm.subscription_term == "6 місяців" ? 6 :
                    s.BaseSubscription.SubscriptionTerm.subscription_term == "1 рік" ? 12 : 0),
                ("term", "desc") => query.OrderByDescending(s =>
                    s.BaseSubscription.SubscriptionTerm.subscription_term == "1 місяць" ? 1 :
                    s.BaseSubscription.SubscriptionTerm.subscription_term == "3 місяці" ? 3 :
                    s.BaseSubscription.SubscriptionTerm.subscription_term == "6 місяців" ? 6 :
                    s.BaseSubscription.SubscriptionTerm.subscription_term == "1 рік" ? 12 : 0),
                _ => query.OrderBy(s => s.subscription_name)
            };

            //Filtration    
            if (minCost.HasValue)
                query = query.Where(s => s.subscription_total_cost >= minCost.Value);

            if (maxCost.HasValue)
                query = query.Where(s => s.subscription_total_cost <= maxCost.Value);

            if (!string.IsNullOrWhiteSpace(activities))
            {
                var activityList = activities.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(a => a.Trim().ToLower()).ToList();
                query = query.Where(s => s.SubscriptionActivities.Any(sa => activityList.Contains(sa.Activity.activity_name.ToLower())));
            }

            if (!string.IsNullOrWhiteSpace(visitTime))
            {
                query = query.Where(s => s.BaseSubscription.SubscriptionVisitTime.subscription_visit_time == visitTime);
            }

            if (!string.IsNullOrWhiteSpace(term))
            {
                query = query.Where(s => s.BaseSubscription.SubscriptionTerm.subscription_term == term);
            }
            

            var result = await query.Select(s => new SubscriptionDto
            {
                SubscriptionId = s.subscription_id,
                SubscriptionName = s.subscription_name,
                SubscriptionTotalCost = s.subscription_total_cost,
                SubscriptionTerm = s.BaseSubscription.SubscriptionTerm.subscription_term,
                SubscriptionVisitTime = s.BaseSubscription.SubscriptionVisitTime.subscription_visit_time,
                Activities = s.SubscriptionActivities.Select(sa => new ActivityDto
                {
                    ActivityName = sa.Activity.activity_name,
                    ActivityPrice = sa.Activity.activity_price,
                    ActivityDescription = sa.Activity.activity_description,
                    ActivityTypeAmount = sa.activity_type_amount
                }).ToList()
            }).ToListAsync();

            return Ok(result);
            
        }

        [HttpPost]
        public async Task<IActionResult> CreateSubscription([FromBody] SubscriptionDto dto)
        {
            if (dto == null || dto.Activities == null || dto.Activities.Count == 0)
                return BadRequest("Некоректні дані абонемента.");

            var baseSubscription = await _context.BaseSubscriptions
                .Include(bs => bs.SubscriptionTerm)
                .Include(bs => bs.SubscriptionVisitTime)
                .FirstOrDefaultAsync(bs =>
                    bs.SubscriptionTerm.subscription_term == dto.SubscriptionTerm &&
                    bs.SubscriptionVisitTime.subscription_visit_time == dto.SubscriptionVisitTime);

            if (baseSubscription == null)
            {
                return BadRequest("Не знайдено відповідний базовий абонемент.");
            }

            var subscription = new Subscription
            {
                base_subscription_id = baseSubscription.base_subscription_id,
                subscription_name = await GenerateUniqueSubscriptionName(
                    dto.SubscriptionVisitTime,
                    dto.SubscriptionTotalCost,
                    dto.Activities.Select(a => a.ActivityName).ToList()
                ),
                subscription_total_cost = dto.SubscriptionTotalCost,
            };

            _context.Subscriptions.Add(subscription);
            await _context.SaveChangesAsync();

            foreach (var act in dto.Activities)
            {
                var activityEntity = await _context.Activities
                    .FirstOrDefaultAsync(a => a.activity_name == act.ActivityName);

                if (activityEntity == null)
                {
                    return BadRequest($"Активність '{act.ActivityName}' не знайдена в базі.");
                }

                var subscriptionActivity = new SubscriptionActivity
                {
                    subscription_id = subscription.subscription_id,
                    activity_id = activityEntity.activity_id,
                    activity_type_amount = act.ActivityTypeAmount
                };

                _context.SubscriptionActivities.Add(subscriptionActivity);
            }

            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";

            LogService.LogAction(userName, roleName, $"Створив новий абонемент");

            return Ok(new { message = "Абонемент успішно створено." });
        }

        [HttpDelete("{subscriptionId}")]
        public async Task<IActionResult> DeleteSubscription(int subscriptionId)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.SubscriptionActivities)
                .FirstOrDefaultAsync(s => s.subscription_id == subscriptionId);

            if (subscription == null)
                return NotFound();

            if (subscription.SubscriptionActivities != null && subscription.SubscriptionActivities.Any())
                _context.SubscriptionActivities.RemoveRange(subscription.SubscriptionActivities);

            _context.Subscriptions.Remove(subscription);
            await _context.SaveChangesAsync();

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Видалив абонемент (ID: {subscriptionId})");

            return Ok(new { Message = $"Абонемент з Id {subscriptionId} та всі пов’язані активності видалені." });
        }

        [HttpPut("{subscriptionId}")]
        public async Task<IActionResult> UpdateSubscription(int subscriptionId, [FromBody] SubscriptionUpdateDto dto)
        {
            var subscription = await _context.Subscriptions
                .Include(s => s.BaseSubscription)
                .Include(s => s.SubscriptionActivities)
                .FirstOrDefaultAsync(s => s.subscription_id == subscriptionId);

            if (subscription == null)
                return NotFound("Абонемент не знайдено");

            var baseSub = await _context.BaseSubscriptions
                .Include(bs => bs.SubscriptionTerm)
                .Include(bs => bs.SubscriptionVisitTime)
                .FirstOrDefaultAsync(bs =>
                    bs.SubscriptionTerm.subscription_term == dto.SubscriptionTerm &&
                    bs.SubscriptionVisitTime.subscription_visit_time == dto.SubscriptionVisitTime);

            if (baseSub == null)
                return BadRequest("Базовий абонемент не знайдено");

            subscription.base_subscription_id = baseSub.base_subscription_id;
            subscription.subscription_total_cost = dto.SubscriptionTotalCost;

            _context.SubscriptionActivities.RemoveRange(subscription.SubscriptionActivities);
            foreach (var act in dto.Activities)
            {
                var activity = await _context.Activities.FirstOrDefaultAsync(a => a.activity_name == act.ActivityName);
                if (activity == null)
                    return BadRequest($"Активність '{act.ActivityName}' не знайдена");

                _context.SubscriptionActivities.Add(new SubscriptionActivity
                {
                    subscription_id = subscription.subscription_id,
                    activity_id = activity.activity_id,
                    activity_type_amount = act.ActivityTypeAmount
                });
            }

            var userName = Request.Headers["X-User-Name"].FirstOrDefault() ?? "Anonymous";
            var roleName = Request.Headers["X-User-Role"].FirstOrDefault() ?? "Unknown";
            LogService.LogAction(userName, roleName, $"Змінив абонемент (ID: {subscriptionId})");

            await _context.SaveChangesAsync();
            return Ok("Абонемент оновлено");
        }

        private async Task<string> GenerateUniqueSubscriptionName(string visitTime, decimal totalCost, List<string> activityNames)
        {
            string part1 = visitTime == "Безлімітний" ? "Premium" : "Standard";
            string part2 = totalCost > 1500 ? "Elite" : "Basic";

            var mindBody = new[] { "Йога", "Пілатес", "БодіБаланс" };
            var cardio = new[] { "Біг", "Спінінг", "ВІТ", "Кікбоксинг", "Лес Міллс" };
            var strength = new[] { "БодіПамп", "ІксКор" };

            string part3 = "Mixed";

            foreach (var name in activityNames)
            {
                if (mindBody.Contains(name)) { part3 = "Mind&Body"; break; }
                if (cardio.Contains(name)) { part3 = "Cardio"; break; }
                if (strength.Contains(name)) { part3 = "Strength"; break; }
            }

            string baseName = $"{part1} {part2} {part3}";

            var existingNames = await _context.Subscriptions
                .Where(s => s.subscription_name.StartsWith(baseName))
                .Select(s => s.subscription_name)
                .ToListAsync();

            if (!existingNames.Contains(baseName))
            {
                return baseName;
            }

            int counter = 1;
            string newName;
            do
            {
                newName = $"{baseName} {counter}";
                counter++;
            }
            while (existingNames.Contains(newName));

            return newName;
        }


    }
}
