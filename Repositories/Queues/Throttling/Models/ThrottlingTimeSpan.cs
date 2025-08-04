using Infrastructure.Utils.Dates;
using System;

namespace Infrastructure.Utils.Repositories.Queues.Throttling.Models;

public sealed class ThrottlingTimeSpan(TimeSpan timeSpan, int maxExecutes, bool isFixed = false) : EnhancedTimeSpan(timeSpan, isFixed)
{
    public readonly int MaxExecutes = maxExecutes;
}
