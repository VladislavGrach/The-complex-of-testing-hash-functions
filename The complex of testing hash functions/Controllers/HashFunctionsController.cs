using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using The_complex_of_testing_hash_functions.Models;

namespace The_complex_of_testing_hash_functions.Controllers
{
    public class HashFunctionsController : Controller
    {
        private readonly HashTestingContext _context;

        public HashFunctionsController(HashTestingContext context)
        {
            _context = context;
        }

        // GET: HashFunctions
        public async Task<IActionResult> Index()
        {
            return View(await _context.HashFunctions.ToListAsync());
        }

        // GET: HashFunctions/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hashFunction = await _context.HashFunctions.FirstOrDefaultAsync(m => m.Id == id);
            if (hashFunction == null) return NotFound();

            return View(hashFunction);
        }

        // GET: HashFunctions/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: HashFunctions/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Algorithm,Description")] HashFunction hashFunction)
        {
            if (ModelState.IsValid)
            {
                _context.Add(hashFunction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hashFunction);
        }

        // GET: HashFunctions/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hashFunction = await _context.HashFunctions.FindAsync(id);
            if (hashFunction == null) return NotFound();

            return View(hashFunction);
        }

        // POST: HashFunctions/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Algorithm,Description")] HashFunction hashFunction)
        {
            if (id != hashFunction.Id) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(hashFunction);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!HashFunctionExists(hashFunction.Id))
                        return NotFound();
                    else
                        throw;
                }
                return RedirectToAction(nameof(Index));
            }
            return View(hashFunction);
        }

        // GET: HashFunctions/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hashFunction = await _context.HashFunctions.FirstOrDefaultAsync(m => m.Id == id);
            if (hashFunction == null) return NotFound();

            return View(hashFunction);
        }

        // POST: HashFunctions/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var hashFunction = await _context.HashFunctions.FindAsync(id);
            if (hashFunction != null)
            {
                _context.HashFunctions.Remove(hashFunction);
                await _context.SaveChangesAsync();
            }
            return RedirectToAction(nameof(Index));
        }

        private bool HashFunctionExists(int id)
        {
            return _context.HashFunctions.Any(e => e.Id == id);
        }

    }
}
