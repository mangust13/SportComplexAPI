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
        public async Task<IActionResult> GetPurchasesForInternalManager([FromQuery] string? searchTerm = null)
        {
            var query = _context.Purchases
                .Include(p => p.Client).ThenInclude(c => c.Gender)
                .Include(p => p.PaymentMethod)
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

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                searchTerm = searchTerm.ToLower();

                query = query.Where(p =>
                    p.Client.client_full_name.ToLower().Contains(searchTerm) ||
                    p.Client.client_phone_number.Contains(searchTerm) ||
                    p.PaymentMethod.payment_method.ToLower().Contains(searchTerm) ||
                    p.Subscription.subscription_name.ToLower().Contains(searchTerm) ||
                    p.Subscription.BaseSubscription.SubscriptionTerm.subscription_term.ToLower().Contains(searchTerm) ||
                    p.Subscription.BaseSubscription.SubscriptionVisitTime.subscription_visit_time.ToLower().Contains(searchTerm) ||
                    p.Subscription.SubscriptionActivities.Any(sa =>
                        sa.Activity.activity_name.ToLower().Contains(searchTerm) ||
                        sa.Activity.activity_description.ToLower().Contains(searchTerm)
                    )
                );
            }

            var purchases = await query
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
                        .Select(sa => new ActivityInfoDto
                        {
                            ActivityName = sa.Activity.activity_name,
                            ActivityPrice = sa.Activity.activity_price,
                            ActivityDescription = sa.Activity.activity_description,
                            ActivityTypeAmount = sa.activity_type_amount
                        })
                        .ToList()
                })
                .ToListAsync();

            return Ok(purchases);
        }

    }
}
