using MediatR;
using SchedulerJobCenter.Application.DTOs;

namespace SchedulerJobCenter.Application.Queries.ValidateCron
{

    public class ValidateCronHandler : IRequestHandler<ValidateCronQuery, CronValidationDto>
    {
        public Task<CronValidationDto> Handle(ValidateCronQuery query, CancellationToken ct)
        {
            try
            {
                var tz = TimeZoneInfo.FindSystemTimeZoneById(query.TimeZoneId);
                var cronExpr = new Quartz.CronExpression(query.CronExpression)
                {
                    TimeZone = tz
                };

                var fireTimes = new List<string>();
                var next = DateTimeOffset.UtcNow;

                for (var i = 0; i < 5; i++)
                {
                    var nextFire = cronExpr.GetNextValidTimeAfter(next);
                    if (nextFire is null) break;

                    var inTz = TimeZoneInfo.ConvertTime(nextFire.Value, tz);
                    fireTimes.Add(inTz.ToString("yyyy-MM-dd HH:mm:ss zzz"));
                    next = nextFire.Value;
                }

                return Task.FromResult(new CronValidationDto
                {
                    IsValid = true,
                    NextFireTimes = fireTimes,
                    TimeZoneId = query.TimeZoneId
                });
            }
            catch (Exception ex) when (ex is TimeZoneNotFoundException or InvalidTimeZoneException)
            {
                throw new ArgumentException($"Invalid TimeZone: '{query.TimeZoneId}'.");
            }
            catch (Exception ex)
            {
                throw new ArgumentException($"Invalid Cron expression: {ex.Message}");
            }
        }
    }
}
