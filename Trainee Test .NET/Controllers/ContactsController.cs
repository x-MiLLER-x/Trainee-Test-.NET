using Microsoft.AspNetCore.Mvc;
using System.IO;
using CsvHelper;
using CsvHelper.Configuration;
using System.Globalization;
using System.Threading.Tasks;
using Trainee_Test_.NET.Data;
using Trainee_Test_.NET.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Trainee_Test_.NET.Controllers
{
    public class ContactsController : Controller
    {
        // Dependency Injection of the database context and logger
        private readonly ApplicationDbContext _context;
        private readonly ILogger<ContactsController> _logger;

        public ContactsController(ApplicationDbContext context, ILogger<ContactsController> logger)
        {
            _context = context;  // Database context for interacting with the Contacts table
            _logger = logger;    // Logger for error tracking
        }

        // GET: Contacts/UploadCSV
        // This action returns the view where users can upload a CSV file
        public IActionResult UploadCSV()
        {
            return View();
        }

        // POST: Contacts/UploadCSV
        // This action processes the uploaded CSV file and inserts the data into the database
        [HttpPost]
        public async Task<IActionResult> UploadCSV(IFormFile file)
        {
            // Check if a file was uploaded and has content
            if (file != null && file.Length > 0)
            {
                try
                {
                    // Configuring CSVHelper to handle the CSV format and delimiter
                    var config = new CsvConfiguration(CultureInfo.InvariantCulture)
                    {
                        Delimiter = ";",  // Set delimiter to semicolon
                        HeaderValidated = null,  // Disable header validation
                        MissingFieldFound = null,  // Disable missing field validation
                        PrepareHeaderForMatch = args => args.Header.ToLower(), // Ensure headers are treated case-insensitively
                    };

                    // Reading the uploaded file stream
                    using (var stream = new StreamReader(file.OpenReadStream()))
                    using (var csv = new CsvReader(stream, config))
                    {
                        // Specify the date format to correctly parse dates from the CSV
                        csv.Context.TypeConverterOptionsCache.GetOptions<DateTime>().Formats = new[] { "dd.MM.yyyy" };

                        // Parse records into a list of Contact objects
                        var contacts = csv.GetRecords<Contact>().ToList();

                        // Adding each contact record to the database context
                        foreach (var contact in contacts)
                        {
                            _context.Contacts.Add(contact);  // Add contact to the database context
                        }

                        // Save the changes to the database
                        await _context.SaveChangesAsync();
                    }

                    // Redirect to the Index action after successful upload
                    return RedirectToAction("Index");
                }
                catch (Exception ex)
                {
                    // Log the error in case of any exceptions
                    _logger.LogError(ex, "Error occurred while uploading CSV file.");

                    // Display the error message on the page
                    ModelState.AddModelError(string.Empty, $"An error occurred while uploading the file: {ex.Message}");
                }
            }
            else
            {
                // If no file was uploaded or file is empty
                ModelState.AddModelError(string.Empty, "No file selected or file is empty.");
            }

            // Return the view with error messages if any
            return View();
        }

        // GET: Contacts/Index
        // This action retrieves all contacts from the database and returns the Index view
        public async Task<IActionResult> Index()
        {
            // Fetch all contacts from the database asynchronously
            var contacts = await _context.Contacts.ToListAsync();

            // Pass the contact list to the view
            return View(contacts);
        }

        // GET: Contacts/Edit/{id}
        // This action returns the Edit view for a specific contact by ID
        public async Task<IActionResult> Edit(int id)
        {
            // Find the contact by ID in the database
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                // Return a 404 Not Found response if the contact doesn't exist
                return NotFound();
            }

            // Return the Edit view with the contact details
            return View(contact);
        }

        // POST: Contacts/Edit
        // This action updates the contact information in the database
        [HttpPost]
        public async Task<IActionResult> Edit(Contact contact)
        {
            // Validate the contact details
            if (ModelState.IsValid)
            {
                // Update the contact details in the database
                _context.Update(contact);
                await _context.SaveChangesAsync();  // Save changes asynchronously

                // Redirect to the Index action after a successful update
                return RedirectToAction("Index");
            }

            // If validation fails, return to the Edit view with the same contact details
            return View(contact);
        }

        // GET: Contacts/Delete/{id}
        // This action returns the Delete confirmation view for a specific contact by ID
        public async Task<IActionResult> Delete(int id)
        {
            // Find the contact by ID in the database
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                // Return a 404 Not Found response if the contact doesn't exist
                return NotFound();
            }

            // Return the Delete confirmation view with the contact details
            return View(contact);
        }

        // POST: Contacts/DeleteConfirmed
        // This action deletes the contact from the database after confirmation
        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            // Find the contact by ID in the database
            var contact = await _context.Contacts.FindAsync(id);
            if (contact == null)
            {
                // Return a 404 Not Found response if the contact doesn't exist
                return NotFound();
            }

            // Remove the contact from the database context
            _context.Contacts.Remove(contact);

            // Save the changes to the database
            await _context.SaveChangesAsync();

            // Redirect to the Index action after successful deletion
            return RedirectToAction("Index");
        }
    }
}
