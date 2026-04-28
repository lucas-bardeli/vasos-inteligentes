using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using MongoDB.Bson;
using MongoDB.Driver;
using VasosInteligentes.Data;
using VasosInteligentes.Models;

namespace VasosInteligentes.Controllers;

public class VasosController : Controller
{
    private readonly ContextMongoDb _context;
    private readonly UserManager<ApplicationUser> _userManager;

    public VasosController(ContextMongoDb context, UserManager<ApplicationUser> userManager)
    {
        _context = context;
        _userManager = userManager;
    }

    // GET: Vasos
    [Authorize(Roles = "Administrador")]
    public async Task<IActionResult> Index()
    {
        var pipeline = new BsonDocument[]
        {
            // Criar campos temporários será usado na conversão de Object para String
            new BsonDocument("$addFields", new BsonDocument
            {
                { "PlantaIdObj", new BsonDocument("$toObjectId", "$PlantaId") }
            }),
            // Faz o "JOIN" usando o campo convertido
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Planta" },
                { "localField", "PlantaIdObj" },
                { "foreignField", "_id" },
                { "as", "PlantaRelacionada" }
            }),
            // Remover campos extras para não "quebrar" o C#
            new BsonDocument("$project", new BsonDocument
            {
                { "PlantaIdObj", 0 }
            })
        };

        var result = await _context.Vaso.Aggregate<Vaso>(pipeline).ToListAsync();

        return View(result);
    }

    // GET: Vasos
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> MeusVasos()
    {
        // Pegar o usuário logado
        var user = _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Accounts"); 
        }
        var usuarioId = user.Id;

        var pipeline = new BsonDocument[]
        {
            new BsonDocument("$match", new BsonDocument("UserId", usuarioId)),
            // Criar campos temporários será usado na conversão de Object para String
            new BsonDocument("$addFields", new BsonDocument
            {
                { "PlantaIdObj", new BsonDocument("$toObjectId", "$PlantaId") }
            }),
            // Faz o "JOIN" usando o campo convertido
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Planta" },
                { "localField", "PlantaIdObj" },
                { "foreignField", "_id" },
                { "as", "PlantaRelacionada" }
            }),
            // Remover campos extras para não "quebrar" o C#
            new BsonDocument("$project", new BsonDocument
            {
                { "PlantaIdObj", 0 }
            })
        };

        var result = await _context.Vaso.Aggregate<Vaso>(pipeline).ToListAsync();

        return View(result);
    }

    // GET: Vasos/Details/5
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Details(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var pipeline = new BsonDocument[]
        {
            // Buscar apenas o vaso cujo id vem por parâmetro
            new BsonDocument("$match", new BsonDocument("_id", new BsonObjectId(new ObjectId(id)))),
            // Criar campos temporários será usado na conversão de Object para String
            new BsonDocument("$addFields", new BsonDocument
            {
                { "PlantaIdObj", new BsonDocument("$toObjectId", "$PlantaId") }
            }),
            // Faz o "JOIN" usando o campo convertido
            new BsonDocument("$lookup", new BsonDocument
            {
                { "from", "Planta" },
                { "localField", "PlantaIdObj" },
                { "foreignField", "_id" },
                { "as", "PlantaRelacionada" }
            }),
            // Remover campos extras para não "quebrar" o C#
            new BsonDocument("$project", new BsonDocument
            {
                { "PlantaIdObj", 0 }
            })
        };

        var vaso = await _context.Vaso.Aggregate<Vaso>(pipeline).FirstOrDefaultAsync();

        if (vaso == null)
        {
            return NotFound();
        }

        return View(vaso);
    }

    // GET: Vasos/Create
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Create()
    {
        var plantas = await _context.Planta.Find(_ => true).ToListAsync();
        ViewBag.PlantaId = new SelectList(plantas, "Id", "Nome");
        return View();
    }

    // POST: Vasos/Create
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Create([Bind("Nome,PlantaId,Localizacao")] Vaso vaso)
    {
        // Pegar o usuário logado
        var user = _userManager.GetUserAsync(User);
        if (user == null)
        {
            return RedirectToAction("Login", "Accounts");
        }
        var usuarioId = user.Id;

        // Como UsuarioId não vem da view, vai criar um erro na ModelState que deve ser retirado. 
        ModelState.Remove("UsuarioId");
        if (ModelState.IsValid)
        {
            await _context.Vaso.InsertOneAsync(vaso);
            return RedirectToAction(nameof(Index));
        }
        return View(vaso);
    }

    // GET: Vasos/Edit/5
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Edit(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        var vaso = await _context.Vaso.Find(m => m.Id == id).FirstOrDefaultAsync();

        if (vaso == null)
        {
            return NotFound();
        }

        var plantas = await _context.Planta.Find(_ => true).ToListAsync();
        ViewBag.PlantaId = new SelectList(plantas, "Id", "Nome", vaso.PlantaId);

        return View(vaso);
    }

    // POST: Vasos/Edit/5
    // To protect from overposting attacks, enable the specific properties you want to bind to.
    // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
    [HttpPost]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> Edit(string id, [Bind("Id,Nome,PlantaId,Localizacao")] Vaso vaso)
    {
        if (id != vaso.Id)
        {
            return NotFound();
        }

        if (ModelState.IsValid)
        {
            try
            {
                await _context.Vaso.ReplaceOneAsync(p => p.Id == id, vaso);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!await VasoExists(vaso.Id))
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
        return View(vaso);
    }

    // POST: Vasos/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    [Authorize(Roles = "Usuario")]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        if (id == null)
        {
            return NotFound();
        }

        await _context.Vaso.DeleteOneAsync(m => m.Id == id);

        return RedirectToAction(nameof(Index));
    }

    private async Task<bool> VasoExists(string id)
    {
        return await _context.Vaso.Find(m => m.Id == id).AnyAsync();
    }
}
