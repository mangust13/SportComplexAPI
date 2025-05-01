using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs;
using SportComplexAPI.Models;

namespace SportComplexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchaseController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public PurchaseController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet("purchases-view")]
        public async Task<IActionResult> GetPurchases(
            string? search = null,
            string sortBy = "purchaseDate",
            string order = "desc",
            string? activities = null,
            string? paymentMethods = null,
            string? clientGender = null,
            decimal? minCost = null,
            decimal? maxCost = null,
            string? purchaseDate = null)
        {
            var query = _context.Purchases
                .Include(p => p.PaymentMethod)
                .Include(p => p.Client)
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.BaseSubscription)
                        .ThenInclude(bs => bs.SubscriptionTerm)
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.BaseSubscription)
                        .ThenInclude(bs => bs.SubscriptionVisitTime)
                .Include(p => p.Subscription)
                    .ThenInclude(s => s.SubscriptionActivities)
                        .ThenInclude(sa => sa.Activity)
                .AsQueryable();

            // Searching
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.ToLower();

                query = query.Where(p =>
                    p.purchase_number.ToString().Contains(search) ||
                    p.Client.client_full_name.ToLower().Contains(search) ||
                    p.Client.client_phone_number.Contains(search) ||
                    p.PaymentMethod.payment_method.ToLower().Contains(search) ||
                    p.Subscription.subscription_name.ToLower().Contains(search) ||
                    p.Subscription.subscription_total_cost.ToString().Contains(search) ||
                    p.Subscription.BaseSubscription.SubscriptionTerm.subscription_term.ToLower().Contains(search) ||
                    p.Subscription.BaseSubscription.SubscriptionVisitTime.subscription_visit_time.ToLower().Contains(search) ||
                    p.Subscription.SubscriptionActivities.Any(sa =>
                        sa.Activity.activity_name.ToLower().Contains(search) ||
                        sa.Activity.activity_description.ToLower().Contains(search)
                    )
                );
            }

            // Sorting
            query = (sortBy, order.ToLower()) switch
            {
                ("purchaseNumber", "asc") => query.OrderBy(p => p.purchase_number),
                ("purchaseNumber", "desc") => query.OrderByDescending(p => p.purchase_number),
                ("purchaseDate", "asc") => query.OrderBy(p => p.purchase_date),
                ("purchaseDate", "desc") => query.OrderByDescending(p => p.purchase_date),
                ("subscriptionName", "asc") => query.OrderBy(p => p.Subscription.subscription_name),
                ("subscriptionName", "desc") => query.OrderByDescending(p => p.Subscription.subscription_name),
                ("subscriptionTotalCost", "asc") => query.OrderBy(p => p.Subscription.subscription_total_cost),
                ("subscriptionTotalCost", "desc") => query.OrderByDescending(p => p.Subscription.subscription_total_cost),
                _ => query.OrderByDescending(p => p.purchase_date) // default fallback
            };

            //Filtration    
            // 🔗 Фільтрація: методи оплати
            if (!string.IsNullOrWhiteSpace(paymentMethods))
            {
                var methods = paymentMethods.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(m => m.Trim().ToLower())
                    .ToList();

                if (methods.Any())
                {
                    query = query.Where(p => methods.Contains(p.PaymentMethod.payment_method.ToLower()));
                }
            }

            // 🔗 Фільтрація: стать клієнта
            if (!string.IsNullOrWhiteSpace(clientGender))
            {
                query = query.Where(p => p.Client.Gender.gender_name.ToLower() == clientGender.ToLower());
            }

            // 🔗 Фільтрація: вартість абонемента
            if (minCost.HasValue)
            {
                query = query.Where(p => p.Subscription.subscription_total_cost >= minCost.Value);
            }

            if (maxCost.HasValue)
            {
                query = query.Where(p => p.Subscription.subscription_total_cost <= maxCost.Value);
            }

            // 🔗 Фільтрація: дата покупки
            if (!string.IsNullOrEmpty(purchaseDate) && DateTime.TryParse(purchaseDate, out var parsedDate))
            {
                query = query.Where(p => p.purchase_date.Date == parsedDate.Date);
            }

            // 🔗 Фільтрація: види активностей
            if (!string.IsNullOrWhiteSpace(activities))
            {
                var activityList = activities.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(a => a.Trim().ToLower())
                    .ToList();

                if (activityList.Any())
                {
                    query = query.Where(p =>
                        p.Subscription.SubscriptionActivities
                            .Any(sa => activityList.Contains(sa.Activity.activity_name.ToLower())));
                }
            }

            // Returning
            var result = await query
                .Select(p => new PurchaseDto
                {
                    PurchaseNumber = p.purchase_number,
                    PurchaseDate = p.purchase_date,
                    PaymentMethod = p.PaymentMethod.payment_method,
                    ClientFullName = p.Client.client_full_name,
                    ClientGender = p.Client.Gender.gender_name,
                    ClientPhoneNumber = p.Client.client_phone_number,
                    SubscriptionName = p.Subscription.subscription_name,
                    SubscriptionTotalCost = p.Subscription.subscription_total_cost,
                    SubscriptionTerm = p.Subscription.BaseSubscription.SubscriptionTerm.subscription_term,
                    SubscriptionVisitTime = p.Subscription.BaseSubscription.SubscriptionVisitTime.subscription_visit_time,
                    Activities = p.Subscription.SubscriptionActivities
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

            return Ok(result);
        }

    }
}
