using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SportComplexAPI.Data;
using SportComplexAPI.DTOs;
using SportComplexAPI.Models;

namespace SportComplexAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AttendanceRecordController : ControllerBase
    {
        private readonly SportComplexContext _context;
        public AttendanceRecordController(SportComplexContext context)
        {
            _context = context;
        }

        [HttpGet("attendance-record-view")]
        public async Task<IActionResult> GetAttendanceFullInfo()
        {
            var attendances = await _context.AttendanceRecords
                .Include(a => a.Purchase)
                    .ThenInclude(p => p.Subscription)
                        .ThenInclude(s => s.BaseSubscription)
                            .ThenInclude(bs => bs.SubscriptionTerm)
                .Include(a => a.Purchase)
                    .ThenInclude(p => p.Subscription)
                        .ThenInclude(s => s.BaseSubscription)
                            .ThenInclude(bs => bs.SubscriptionVisitTime)
                .Include(a => a.Training)
                    .ThenInclude(t => t.TrainerSchedule)
                        .ThenInclude(ts => ts.ActivityInGym)
                            .ThenInclude(aig => aig.Activity)
                .Include(a => a.Training)
                    .ThenInclude(t => t.TrainerSchedule)
                        .ThenInclude(ts => ts.Trainer)
                            .ThenInclude(tr => tr.TrainerActivities)
                                .ThenInclude(ta => ta.Activity)
                .Include(a => a.Training)
                    .ThenInclude(t => t.TrainerSchedule)
                        .ThenInclude(ts => ts.ActivityInGym)
                            .ThenInclude(aig => aig.Gym)
                                .ThenInclude(g => g.SportComplex)
                .Select(a => new AttendanceRecordDto
                {
                    PurchaseNumber = a.Purchase.purchase_number,
                    PurchaseDate = a.Purchase.purchase_date,
                    SubscriptionName = a.Purchase.Subscription.subscription_name,
                    SubscriptionTerm = a.Purchase.Subscription.BaseSubscription.SubscriptionTerm.subscription_term,
                    SubscriptionVisitTime = a.Purchase.Subscription.BaseSubscription.SubscriptionVisitTime.subscription_visit_time,
                    ActivityName = a.Training.TrainerSchedule.ActivityInGym.Activity.activity_name,
                    TrainerName = a.Training.TrainerSchedule.Trainer.trainer_full_name,
                    GymNumber = a.Training.TrainerSchedule.ActivityInGym.Gym.gym_number,
                    SportComplexAddress = a.Training.TrainerSchedule.ActivityInGym.Gym.SportComplex.complex_address
                })
                .ToListAsync();

            return Ok(attendances);
        }


    }
}
