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
            return Ok(subscriptions);
            
        }
    }
}
