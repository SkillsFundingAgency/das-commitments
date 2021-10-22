using System;

namespace SFA.DAS.CommitmentsV2.Shared.Models
{
    /// <summary>
    ///     Encapsulates a date whilst exposing separate day, month and year properties that can be set independently.
    ///     The three elements are validated when set but may not be valid when used together, for example 31-Feb. In this
    ///     case accessing the <see cref="Date"/> will throw an exception. To avoid this check the <see cref="IsValid"/>
    ///     property before accessing <see cref="Date"/>.
    /// </summary>
    public class DateModel
    {
        private DateTime? _currentValue;
        private int? _day;
        private int? _month;
        private int? _year;

        public DateModel()
        {
            // provide a default constructor
        }

        public DateModel(DateTime dateTime) : this()
        {
            _currentValue = dateTime;
            _day = dateTime.Day;
            _month = dateTime.Month;
            _year = dateTime.Year;
        }

        public virtual int? Day
        {
            get => _day;
            set
            {
                SetIfDifferent(_day, value, newValue => _day = newValue);
            }
        }

        public int? Month
        {
            get => _month;
            set
            {
                SetIfDifferent(_month, value, newValue => _month = newValue);
            }
        }

        public int? Year
        {
            get => _year;
            set
            {
                SetIfDifferent(_year, value, newValue => _year = newValue);
            }
        }

        public DateTime? Date => _currentValue ?? (_currentValue =
                                     IsValid ? new DateTime(Year.Value, Month.Value, Day.Value) : (DateTime?) null);

        public bool IsValid => HasValue && (IsValidDay(Day) && IsValidMonth(Month) && IsValidYear(Year) &&
                                            Day <= DateTime.DaysInMonth(Year.Value, Month.Value));

        public virtual bool HasValue => Day.HasValue || Month.HasValue || Year.HasValue;

        private bool IsValidDay(int? day)
        {
            return day.HasValue && day > 0 && day <= 31;
        }

        private bool IsValidMonth(int? month)
        {
            return month.HasValue && month > 0 && month <= 12;
        }

        private bool IsValidYear(int? year)
        {
            return year.HasValue && year >= 1000 && year >= DateTime.MinValue.Year && year <= DateTime.MaxValue.Year;
        }

        private void SetIfDifferent(int? currentValue, int? newValue, Action<int?> change)
        {
            if (currentValue != newValue)
            {
                change(newValue);
                _currentValue = null;
            }
        }
    }
}