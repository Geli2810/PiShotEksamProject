namespace PiShotProject
{
    public class Class1
    {
        private int _year;
        private int _month;
        private int _day;

        public Class1(int year, int month, int day)
        {
            _year = year;
            _month = month;
            _day = day;
        }

        public Class1() { }

        public int Year
        {
            get { return _year; }
            set
            {
                if (value <0|| value <2010 || value > DateTime.Now.Year)
                {
                    throw new ArgumentException("Invalid year");
                }
                _year = value;
            }
        }

        public int Month
        {
            get { return _month; }
            set
            {
                if (value < 1 || value > 12)
                {
                    throw new ArgumentException("Invalid month");
                }
                _month = value;
            }
        }

        public int Day
        {
            get { return _day; }
            set
            {
                if (value < 1 || value > 31)
                {
                    throw new ArgumentException("Invalid day");
                }
                _day = value;
            }
        }

        public override string ToString() => $"{_year:D4}-{_month:D2}-{_day:D2}";



    }
}
