using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using MongoDB.Driver;
using VasosInteligentes.Data;
using VasosInteligentes.Models;

namespace VasosInteligentes.Controllers;

[Authorize(Roles = "Administrador")]
public class UsersController : Controller
{
    private readonly ContextMongoDb _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(ContextMongoDb context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Users
    public async Task<IActionResult> Index()
    {
        var users = await _context.Users.Find(_ => true).ToListAsync();
        return View(users);
    }

    // GET: Users/Details/5
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .Find(m => m.Id == id)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // GET: Users/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: Users/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create([Bind("Nome,Telefone,Email,Senha")] User user)
    {
        ModelState.Remove("Id");
        if (ModelState.IsValid)
        {
            await _context.Users.InsertOneAsync(user);
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: Users/Edit/5
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
            .Find(u => u.Id == id)
            .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }
        return View(user);
    }

    // POST: Users/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(string id, [Bind("Nome,Telefone,Email,Senha")] User user)
    {
        if (id != user.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _context.Users.ReplaceOneAsync(u => u.Id == id, user);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await UserExists(user.Id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }
            return RedirectToAction(nameof(Index));
        }
        return View(user);
    }

    // GET: Users/Delete/5
    public async Task<IActionResult> Delete(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var user = await _context.Users
           .Find(u => u.Id == id)
           .FirstOrDefaultAsync();

        if (user == null)
        {
            return NotFound();
        }

        return View(user);
    }

    // POST: Users/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        var user = await _context.Users.FindAsync(id);
        if (user != null)
        {
            await _context.Users.DeleteOneAsync(u => u.Id == id);
        }
        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> UserExists(string id)
    {
        return await _context.Users
            .Find(u => u.Id == id)
            .AnyAsync();
    }
}
