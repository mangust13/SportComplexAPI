using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs;
using SportComplexAPI.Models;

namespace SportComplexAPI.Controllers
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
            string? search = null, string sortBy = "name",
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

            // Searching
            //if (!string.IsNullOrWhiteSpace(search))
            //{
            //    search = search.ToLower();

            //    query = query.Where(s =>
            //        s.subscription_name.ToLower().Contains(search) ||
            //        s.subscription_total_cost.ToString().Contains(search) ||
            //        s.BaseSubscription.SubscriptionVisitTime.subscription_visit_time.ToLower().Contains(search) ||
            //        s.BaseSubscription.SubscriptionTerm.subscription_term.ToLower().Contains(search) ||
            //        s.SubscriptionActivities.Any(sa =>
            //            sa.Activity.activity_name.ToLower().Contains(search)
            //        )
            //    );
            //}


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

            // Знайти відповідний BaseSubscription (по терміну і часу відвідування)
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

            // Створення нового абонемента
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

            // Створення активностей для підписки
            foreach (var act in dto.Activities)
            {
                var activityEntity = await _context.Activities
                    .FirstOrDefaultAsync(a => a.activity_name == act.ActivityName);

                if (activityEntity == null)
                {
                    // Якщо активності не існує — можна або кидати помилку, або скіпати
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
            {
                _context.SubscriptionActivities.RemoveRange(subscription.SubscriptionActivities);
            }

            _context.Subscriptions.Remove(subscription);

            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Абонемент з Id {subscriptionId} та всі пов’язані активності видалені." });
        }


        private async Task<string> GenerateUniqueSubscriptionName(string visitTime, decimal totalCost, List<string> activityNames)
        {
            string part1 = (visitTime == "Безлімітний") ? "Premium" : "Standard";
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

            // Завантажуємо всі існуючі підписки з такою базовою назвою
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
