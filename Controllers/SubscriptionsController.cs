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
        public async Task<IActionResult> GetAllSubscriptions([FromQuery] string? searchTerm = null)
        {
            var subscriptions = await _context.Subscriptions
                .Include(s => s.BaseSubscription)
                    .ThenInclude(bs => bs.SubscriptionTerm)
                .Include(s => s.BaseSubscription)
                    .ThenInclude(bs => bs.SubscriptionVisitTime)
                .Include(s => s.SubscriptionActivities)
                    .ThenInclude(sa => sa.Activity)
                .Select(s => new SubscriptionDto
                {
                    SubscriptionId = s.subscription_id,
                    SubscriptionName = s.subscription_name,
                    SubscriptionTotalCost = s.subscription_total_cost,
                    SubscriptionTerm = s.BaseSubscription.SubscriptionTerm.subscription_term,
                    SubscriptionVisitTime = s.BaseSubscription.SubscriptionVisitTime.subscription_visit_time,
                    Activities = s.SubscriptionActivities
                        .Select(sa => new ActivityDto
                        {
                            ActivityName = sa.Activity.activity_name,
                            ActivityPrice = sa.Activity.activity_price,
                            ActivityDescription = sa.Activity.activity_description,
                            ActivityTypeAmount = sa.activity_type_amount
                        })
                        .ToList()
                })
                .ToListAsync();
            return Ok(subscriptions);
            
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
