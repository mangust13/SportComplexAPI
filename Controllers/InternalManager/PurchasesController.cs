using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs.InternalManager;
using SportComplexAPI.Models;

namespace SportComplexAPI.Controllers.InternalManager
{
    [ApiController]
    [Route("api/[controller]")]
    public class PurchasesController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public PurchasesController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet("purchases-view")]
        public async Task<IActionResult> GetPurchases(
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
                    .ThenInclude(c => c.Gender)
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
                _ => query.OrderByDescending(p => p.purchase_date)
            };

            // Filters
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

            if (!string.IsNullOrWhiteSpace(clientGender))
            {
                query = query.Where(p => p.Client.Gender.gender_name.ToLower() == clientGender.ToLower());
            }

            if (minCost.HasValue)
            {
                query = query.Where(p => p.Subscription.subscription_total_cost >= minCost.Value);
            }

            if (maxCost.HasValue)
            {
                query = query.Where(p => p.Subscription.subscription_total_cost <= maxCost.Value);
            }

            if (!string.IsNullOrEmpty(purchaseDate) && DateTime.TryParse(purchaseDate, out var parsedDate))
            {
                query = query.Where(p => p.purchase_date.Date == parsedDate.Date);
            }

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

            var purchaseIds = await query.Select(p => p.purchase_id).ToListAsync();

            var attendanceCounts = await _context.AttendanceRecords
                .Where(ar => purchaseIds.Contains(ar.purchase_id))
                .GroupBy(ar => ar.purchase_id)
                .Select(g => new { PurchaseId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.PurchaseId, x => x.Count);

            var purchases = await query
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
                .ToListAsync();

            var result = purchases.Select(p => new PurchaseDto
            {
                PurchaseId = p.purchase_id,
                PurchaseNumber = p.purchase_number,
                PurchaseDate = p.purchase_date,
                PaymentMethod = p.PaymentMethod?.payment_method ?? "—",
                ClientFullName = p.Client?.client_full_name ?? "—",
                ClientGender = p.Client?.Gender?.gender_name ?? "—",
                ClientPhoneNumber = p.Client?.client_phone_number ?? "—",
                SubscriptionName = p.Subscription?.subscription_name ?? "—",
                SubscriptionTotalCost = p.Subscription?.subscription_total_cost ?? 0,
                SubscriptionTerm = p.Subscription?.BaseSubscription?.SubscriptionTerm?.subscription_term ?? "—",
                SubscriptionVisitTime = p.Subscription?.BaseSubscription?.SubscriptionVisitTime?.subscription_visit_time ?? "—",
                Activities = p.Subscription?.SubscriptionActivities != null
                    ? p.Subscription.SubscriptionActivities.Select(sa => new ActivityDto
                    {
                        ActivityName = sa.Activity?.activity_name ?? "—",
                        ActivityPrice = sa.Activity?.activity_price ?? 0,
                        ActivityDescription = sa.Activity?.activity_description ?? "—",
                        ActivityTypeAmount = sa.activity_type_amount
                    }).ToList()
                    : new List<ActivityDto>(),
                TotalAttendances = attendanceCounts.TryGetValue(p.purchase_id, out var count) ? count : 0
            }).ToList();


            return Ok(result);

        }


        public class PurchaseCreateDto
        {
            public int ClientId { get; set; }
            public int SubscriptionId { get; set; }
            public int PaymentMethodId { get; set; }
        }

        [HttpPost]
        public async Task<IActionResult> CreatePurchase([FromBody] PurchaseCreateDto dto)
        {
            var client = await _context.Clients.FindAsync(dto.ClientId);
            if (client == null) return NotFound($"Клієнт з ID {dto.ClientId} не знайдений.");

            var subscription = await _context.Subscriptions.FindAsync(dto.SubscriptionId);
            if (subscription == null) return NotFound($"Абонемент з ID {dto.SubscriptionId} не знайдений.");

            var paymentMethod = await _context.PaymentMethods.FindAsync(dto.PaymentMethodId);
            if (paymentMethod == null) return NotFound($"Метод оплати з ID {dto.PaymentMethodId} не знайдений.");

            var maxPurchaseNumber = await _context.Purchases.MaxAsync(p => (int?)p.purchase_number) ?? 1;
            var nextPurchaseNumber = maxPurchaseNumber + 1;

            var purchase = new Purchase
            {
                client_id = dto.ClientId,
                subscription_id = dto.SubscriptionId,
                payment_method_id = dto.PaymentMethodId,
                purchase_date = DateTime.UtcNow,
                purchase_number = nextPurchaseNumber,
            };

            _context.Purchases.Add(purchase);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                Message = "Покупка успішно створена!",
                PurchaseId = purchase.purchase_id
            });
        }

        [HttpDelete("{purchaseId}")]
        public async Task<IActionResult> DeletePurchase(int purchaseId)
        {
            var purchase = await _context.Purchases.FindAsync(purchaseId);
            if (purchase == null)
                return NotFound();

            _context.Purchases.Remove(purchase);
            await _context.SaveChangesAsync();

            return Ok(new { Message = $"Покупка з Id {purchaseId} видалена." });
        }

        [HttpPut("{purchaseId}")]
        public async Task<IActionResult> UpdatePurchase(int purchaseId, [FromBody] PurchaseUpdateDto dto)
        {
            var purchase = await _context.Purchases
                .Include(p => p.PaymentMethod)
                .Include(p => p.Client).ThenInclude(c => c.Gender)
                .Include(p => p.Subscription).ThenInclude(s => s.BaseSubscription).ThenInclude(bs => bs.SubscriptionTerm)
                .Include(p => p.Subscription).ThenInclude(s => s.BaseSubscription).ThenInclude(bs => bs.SubscriptionVisitTime)
                .Include(p => p.Subscription).ThenInclude(s => s.SubscriptionActivities).ThenInclude(sa => sa.Activity)
                .FirstOrDefaultAsync(p => p.purchase_id == purchaseId);

            if (purchase == null) return NotFound($"Покупка з ID {purchaseId} не знайдена.");

            var client = await _context.Clients.FindAsync(dto.ClientId);
            if (client == null) return NotFound($"Клієнт з ID {dto.ClientId} не знайдений.");

            var paymentMethod = await _context.PaymentMethods.FirstOrDefaultAsync(pm => pm.payment_method == dto.PaymentMethod);
            if (paymentMethod == null) return NotFound($"Метод оплати {dto.PaymentMethod} не знайдений.");

            var subscription = await _context.Subscriptions.FindAsync(dto.SubscriptionId);
            if (subscription == null) return NotFound($"Абонемент {dto.SubscriptionId} не знайдений.");

            purchase.client_id = client.client_id;
            purchase.payment_method_id = paymentMethod.payment_method_id;
            purchase.subscription_id = subscription.subscription_id;

            await _context.SaveChangesAsync();

            var attendanceCount = await _context.AttendanceRecords
                .Where(ar => ar.purchase_id == purchaseId)
                .CountAsync();

            var updatedDto = new PurchaseDto
            {
                PurchaseId = purchase.purchase_id,
                PurchaseNumber = purchase.purchase_number,
                PurchaseDate = purchase.purchase_date,
                PaymentMethod = paymentMethod.payment_method ?? "—",
                ClientFullName = client.client_full_name ?? "—",
                ClientGender = client.Gender.gender_name,
                ClientPhoneNumber = client.client_phone_number ?? "—",
                SubscriptionName = subscription.subscription_name ?? "—",
                SubscriptionTotalCost = subscription.subscription_total_cost,
                SubscriptionTerm = subscription.BaseSubscription?.SubscriptionTerm?.subscription_term ?? "—",
                SubscriptionVisitTime = subscription.BaseSubscription?.SubscriptionVisitTime?.subscription_visit_time ?? "—",
                Activities = subscription.SubscriptionActivities != null
                    ? subscription.SubscriptionActivities.Select(sa => new ActivityDto
                    {
                        ActivityName = sa.Activity?.activity_name ?? "—",
                        ActivityPrice = sa.Activity?.activity_price ?? 0,
                        ActivityDescription = sa.Activity?.activity_description ?? "—",
                        ActivityTypeAmount = sa.activity_type_amount
                    }).ToList()
                    : new List<ActivityDto>(),
                TotalAttendances = attendanceCount
            };

            return Ok(updatedDto);
        }


        public class PurchaseUpdateDto
        {
            public int ClientId { get; set; }
            public string PaymentMethod { get; set; }
            public int SubscriptionId { get; set; }
        }

        [HttpGet("subscriptions-names")]
        public async Task<IActionResult> GetSubscriptionNames()
        {
            var subscriptions = await _context.Subscriptions
                .Select(s => new
                {
                    Name = s.subscription_name,
                    TotalCost = s.subscription_total_cost
                })
                .ToListAsync();

            return Ok(subscriptions);
        }

    }
}
