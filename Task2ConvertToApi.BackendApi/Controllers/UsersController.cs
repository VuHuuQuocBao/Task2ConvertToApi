using Data.EF;
using Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Task2ConvertToApi.BackendApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : Controller
    {
        private readonly Task2DbContext _context;

        public UsersController(Task2DbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public ActionResult Index()
        {
            return View();
        }

        [HttpGet("CalculateStatus")]
        public IActionResult CalculateStatus(Guid userId, int month, int year, bool isFirstSaturdayWorking)
        {
            var allWorkDay = this.GetAllWorkDay(year, month, isFirstSaturdayWorking);
            foreach (var item in allWorkDay)
            {
                var record = _context.UserDailyTimesheetModels.Where(x => x.Month == month && x.Year == year && x.UserId == userId)/*.Select(i => i.Day == item)*/.ToList();
                if (record.Count == 0)
                    continue;
                if (record[0].CheckInTime == null)
                {
                    record[0].Status = "absent";
                    _context.SaveChanges();
                }
                else if (record[0].CheckOutTime == null)
                {
                    record[0].Status = "Inprocess";
                    _context.SaveChanges();
                }
                else if (this.CalculateWorkTimePerDay(userId, record[0].Day, month, year) < 28800)
                {
                    record[0].Status = "INCOMPLETE";
                    _context.SaveChanges();
                }
                else
                {
                    Console.WriteLine("VALID");
                }
            }
            return Ok();
        }

        [HttpGet("CalculateWorkTimePerDay")]
        public int CalculateWorkTimePerDay(Guid userId, int day, int month, int year)
        {
            var query = _context.UserDailyTimesheetModels.Where(x => x.Day == day && x.Month == month && x.Year == year && x.UserId == userId);
            var checkIn = query.Select(x => x.CheckInTime).ToList();
            var checkOut = query.Select(x => x.CheckOutTime).ToList();
            if (checkIn == null)
            {
                return 0;
            }
            if (checkOut == null)
            {
                return 0;
            }

            DateTime d1 = new DateTime(year, month, day, 12, 0, 0);
            DateTime d2 = new DateTime(year, month, day, 13, 30, 0);

            var result1 = d1.Subtract((DateTime)checkIn[0]);
            var result1Hour = result1.Hours;
            var result1HourConvertToSecond = result1Hour * 3600;
            var result1Minute = result1.Minutes;
            var result1MinuteConverToSecond = result1Minute * 60;
            var result1Second = result1.Seconds;
            var result1Final = result1HourConvertToSecond + result1MinuteConverToSecond + result1Second;
            DateTime checkOutUpdate = (DateTime)checkOut[0];
            var result2 = checkOutUpdate.Subtract(d2);
            var result2Hour = result2.Hours;
            var result2HourConvertToSecond = result2Hour * 3600;
            var result2Minute = result2.Minutes;
            var result2MinuteConverToSecond = result2Minute * 60;
            var result2Second = result2.Seconds;
            var result2Final = result2HourConvertToSecond + result2MinuteConverToSecond + result2Second;
            var finalResult = result1Final + result2Final;

            var result = query.ToList();
            result[0].TotalActualWorkingTimeInSeconds = finalResult;
            _context.SaveChanges();
            return finalResult;
        }

        [HttpGet("CalculateLate")]
        public IActionResult CalculateLate(Guid userId, int day, int month, int year)
        {
            var checkIn = _context.UserDailyTimesheetModels.Where(x => x.Day == day && x.Month == month && x.Year == year && x.UserId == userId).Select(x => x.CheckInTime).ToList();
            if (checkIn == null)
            {
                return Ok("no check in");
            }
            var result = checkIn[0];
            DateTime d3 = new DateTime(2022, 5, 12, 8, 40, 0);
            var something = DateTime.Compare((DateTime)result, d3);
            Console.WriteLine(something);
            if (something < 0)
            {
                var writeToDatbase = _context.UserDailyTimesheetModels.Where(x => x.Day == day && x.Month == month && x.Year == year && x.UserId == userId).ToList();
                writeToDatbase[0].IsLate = true;
                _context.SaveChanges();
            }
            else
            {
                var writeToDatbase = _context.UserDailyTimesheetModels.Where(x => x.Day == day && x.Month == month && x.Year == year && x.UserId == userId).ToList();
                writeToDatbase[0].IsLate = false;
                _context.SaveChanges();
            }

            return Ok(userId);
        }

        [HttpGet("GetAllWorkDay")]
        public List<int> GetAllWorkDay(int year, int month, bool isFirstSaturdayWorking)
        {
            var allWeekDayDateTimeFormat = AllDatesInMonth(year, month).Where(i => i.DayOfWeek != DayOfWeek.Sunday && i.DayOfWeek != DayOfWeek.Saturday);
            var allSaturdayDateTimeFormat = AllDatesInMonth(year, month).Where(i => i.DayOfWeek == DayOfWeek.Saturday);

            List<int> allWeekDay = new List<int>();
            List<int> allSaturday = new List<int>();
            foreach (var item in allWeekDayDateTimeFormat)
            {
                allWeekDay.Add(item.Day);
            }

            foreach (var item in allSaturdayDateTimeFormat)
            {
                allSaturday.Add(item.Day);
            }
            if (isFirstSaturdayWorking)
            {
                var evens = allSaturday.Where((item, index) => index % 2 == 0);
                allWeekDay.AddRange(evens);
            }
            else
            {
                var odds = allSaturday.Where((item, index) => index % 2 != 0);
                allWeekDay.AddRange(odds);
            }
            return allWeekDay;
        }

        public static IEnumerable<DateTime> AllDatesInMonth(int year, int month)
        {
            List<DateTime> result = new List<DateTime>();

            int days = DateTime.DaysInMonth(year, month);
            for (int day = 1; day <= days; day++)
            {
                result.Add(new DateTime(year, month, day));
            }
            return result;
        }

        /// <summary>
        /// API seed data
        /// </summary>
        /// <returns></returns>

        [HttpGet("SeedData")]
        public async Task<IActionResult> SeedData()
        {
            List<UserDailyTimesheetModel> seedData = new List<UserDailyTimesheetModel>()
              {
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 1, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 1, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 1, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 2, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 2, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 2, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 3, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 3, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 3, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 4, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 4, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 4, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 5, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 5, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 5, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 6, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 6, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 6, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 8, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 8, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 8, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 9, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 9, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 9, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 10, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 10, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 10, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 11, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 11, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 11, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 12, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 12, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 12, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 15, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 15, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 15, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 16, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 16, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 16, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 17, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 17, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 17, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 18, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 18, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 18, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 19, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 19, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 19, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 20, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 20, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 20, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 22, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 22, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 22, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 23, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 23, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 23, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 24, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 24, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 24, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 25, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 25, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 25, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 26, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 26, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 26, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 27, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 27, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 27, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 29, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 29, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 29, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 30, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 30, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 30, 18, 0, 0) },
                 new UserDailyTimesheetModel() { UserId = new Guid("8bf9ddce-be27-4123-a27a-d5802873e3d1"), Day = 31, Month = 8, Year = 2022, IsLate = false, TotalLateInSeconds = null, TotalActualWorkingTimeInSeconds = null, Status = null, CheckInTime = new DateTime(2022, 08, 31, 9, 0, 0), CheckOutTime = new DateTime(2022, 08, 31, 18, 0, 0) }
              };

            await _context.UserDailyTimesheetModels.AddRangeAsync(seedData);
            _context.SaveChanges();

            return Ok();
        }

        /// <summary>
        /// phan trang
        /// </summary>
        /// <returns></returns>

        [HttpGet("FilterAndPaging")]
        public IActionResult FilterAndPaging(int pageIndex, int pageSize, int month, int year, Guid userId)
        {
            // select + filter
            var query = _context.UserDailyTimesheetModels.Where(x => x.Month == month && x.Year == year && x.UserId == userId);

            // paging
            var data = query.Skip((pageIndex - 1) * pageSize)
                .Take(pageSize).ToList();

            return Ok(data);
        }
    }
}