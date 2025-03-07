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

        public async Task<IActionResult> Index()
        {
            return View(await _context.HashFunctions.ToListAsync());
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var hashFunction = await _context.HashFunctions.FirstOrDefaultAsync(m => m.Id == id);
            if (hashFunction == null) return NotFound();

            return View(hashFunction);
        }

        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,AlgorithmType,Description")] HashFunction hashFunction)
        {
            if (ModelState.IsValid)
            {
                _context.HashFunctions.Add(hashFunction);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(hashFunction);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var hashFunction = await _context.HashFunctions.FindAsync(id);
            if (hashFunction == null) return NotFound();

            return View(hashFunction);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, HashFunction hashFunction)
        {
            if (id != hashFunction.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                _context.Update(hashFunction);
                _context.SaveChanges();
                return RedirectToAction("Index");
            }

            return View(hashFunction);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var hashFunction = await _context.HashFunctions.FirstOrDefaultAsync(m => m.Id == id);
            if (hashFunction == null) return NotFound();

            return View(hashFunction);
        }

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
