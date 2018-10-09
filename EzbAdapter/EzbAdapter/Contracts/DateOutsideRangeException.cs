using System;

namespace EzbAdapter.Contracts
{
    public class DateOutsideRangeException : Exception
    {
        public DateOutsideRangeException(DateTime date) :base($"date {date} is outside the allowed gap between two dates") { }
    }
}