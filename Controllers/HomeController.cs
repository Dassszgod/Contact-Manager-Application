using ContactManagerApp.Models;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Globalization;
using System.Reflection.PortableExecutable;

namespace ContactManagerApp.Controllers
{
    public class HomeController : Controller
    {
		private readonly AppDbContext _context;

		public HomeController(AppDbContext context)
		{
			_context = context;
		}

		public IActionResult Index()
        {
			var users = _context.Users.AsQueryable();

			return View(users);
        }

        [HttpPost] 
        public IActionResult UploadCsv(IFormFile csv)
        {
			if (csv == null || csv.Length == 0)
			{
				return BadRequest("No file uploaded.");
			}

			using (var reader = new StreamReader(csv.OpenReadStream()))
			using (var csvFile = new CsvReader(reader, new CsvConfiguration(CultureInfo.InvariantCulture)
			{
				Delimiter = ";",
				HeaderValidated=null,
				MissingFieldFound=null
			}))
			{
				var records = csvFile.GetRecords<User>().ToList();

				_context.Users.AddRange(records);
				_context.SaveChanges();
			}

			return RedirectToAction(nameof(Index));
		}

		[HttpPost]
		public IActionResult DeleteConfirmed(int Id)
		{
			var user = _context.Users.Find(Id);
			if (user != null)
			{
				_context.Users.Remove(user);
				_context.SaveChanges();
				return RedirectToAction(nameof(Index));
			}
			return NotFound();
		}

		[HttpGet]
		public IActionResult Edit(int id)
		{
			var user = _context.Users.Find(id);
			if (user == null)
			{
				return NotFound();
			}
			return View(user);
		}

		[HttpPost]
		public IActionResult Edit(User model)
		{

			var currentUser = _context.Users.Find(model.Id);
			if (currentUser != null)
			{

				currentUser.Name = model.Name;
				currentUser.DateOfBirth = model.DateOfBirth;
				currentUser.Married = model.Married;
				currentUser.Phone = model.Phone;
				currentUser.Salary = model.Salary;

				_context.SaveChanges();
				return RedirectToAction(nameof(Index));
			}


			foreach (var error in ModelState.Values.SelectMany(v => v.Errors))
			{
				Console.WriteLine(error.ErrorMessage);
			}

			return View(model);
		}

		[ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
