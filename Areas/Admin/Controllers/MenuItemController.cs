using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using EiraPlaza.Data;
using EiraPlaza.Models;
using EiraPlaza.Models.ViewModels;
using EiraPlaza.Utility;

namespace EiraPlaza.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]

    public class MenuItemController : Controller
    {
        private readonly ApplicationDbContext _db;
        private readonly IHostingEnvironment _hostingEnvironment;

        [BindProperty]
        public MenuItemViewModel MenuItemVM { get; set; }

        public MenuItemController(ApplicationDbContext db,IHostingEnvironment hostingEnvironment)
        {
            _db = db;
            _hostingEnvironment = hostingEnvironment;
            MenuItemVM = new MenuItemViewModel()
            {
                Category = _db.Category,
                MenuItem = new Models.MenuItem()
            };
        }
        public async Task<IActionResult> Index()
        {
            var menuItems = await _db.MenuItem.Include(s=>s.Category)
                                               .Include(s=>s.SubCategory)
                                               .ToListAsync();
            return View(menuItems);
        }

        //---------------------------------------------------//

        //Create-------------GET
        public IActionResult Create()
        {
            return View(MenuItemVM);
        }
        
        //Create-----------------POST
        [HttpPost,ActionName("Create")]
        [ValidateAntiForgeryToken]
        public  async Task<IActionResult> CreatePost()
        {
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                return View(MenuItemVM);
            }
            _db.MenuItem.Add(MenuItemVM.MenuItem);
            await _db.SaveChangesAsync();

            //work on image saving

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var MenuItemFromDB = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);
            if (files.Count > 0)
            {
                //  File uploaded
                var upload = Path.Combine(webRootPath, "images");
                var extension = Path.GetExtension(files[0].FileName);
                var totalpath = Path.Combine(upload, MenuItemVM.MenuItem.Id + extension);
                using (var filestream=new FileStream(totalpath, FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }
                MenuItemFromDB.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension;
            }
            else
            {
                var upload = Path.Combine(webRootPath, @"images\"+SD.DefaultFoodImage);
                System.IO.File.Copy(upload, webRootPath + @"\images\" + MenuItemVM.MenuItem.Id + ".png");
                MenuItemFromDB.Image = @"\images\" + MenuItemVM.MenuItem.Id + ".png";

                //no file uploaded use default
            }
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

//----------------------------------------------------------------------//
        
        // Get-------- Edit            
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

       // Post----EDIT
        [HttpPost, ActionName("Edit")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditPost(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem.SubCategoryId = Convert.ToInt32(Request.Form["SubCategoryId"].ToString());
            if (!ModelState.IsValid)
            {
                MenuItemVM.SubCategory = await _db.SubCategory.Where(s => s.CategoryId == MenuItemVM.MenuItem.CategoryId).ToListAsync();
                return View(MenuItemVM);
            }
           

            //work on image saving

            string webRootPath = _hostingEnvironment.WebRootPath;
            var files = HttpContext.Request.Form.Files;
            var MenuItemFromDB = await _db.MenuItem.FindAsync(MenuItemVM.MenuItem.Id);
            if (files.Count > 0)
            {
                //  File uploaded
                var upload = Path.Combine(webRootPath, "images");
                var extension_new = Path.GetExtension(files[0].FileName);
                var totalpath = Path.Combine(upload, MenuItemVM.MenuItem.Id + extension_new);

                //delete old file
                var imagepath = Path.Combine(webRootPath,MenuItemFromDB.Image.TrimStart('\\'));
                if (System.IO.File.Exists(imagepath))
                {
                    System.IO.File.Delete(imagepath);
                }
                ///Upload the new file to the database
                using (var filestream = new FileStream(totalpath, FileMode.Create))
                {
                    files[0].CopyTo(filestream);
                }
                MenuItemFromDB.Image = @"\images\" + MenuItemVM.MenuItem.Id + extension_new;
            }
            MenuItemFromDB.Name = MenuItemVM.MenuItem.Name;
            MenuItemFromDB.Description = MenuItemVM.MenuItem.Description;
            MenuItemFromDB.Price = MenuItemVM.MenuItem.Price;
            MenuItemFromDB.Spicyness = MenuItemVM.MenuItem.Spicyness;
            MenuItemFromDB.CategoryId = MenuItemVM.MenuItem.CategoryId;
            MenuItemFromDB.SubCategoryId = MenuItemVM.MenuItem.SubCategoryId;
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        //Details ---------------------
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);
            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }
            return View(MenuItemVM);
        }

        //GET : Delete MenuItem
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            MenuItemVM.MenuItem = await _db.MenuItem.Include(m => m.Category).Include(m => m.SubCategory).SingleOrDefaultAsync(m => m.Id == id);

            if (MenuItemVM.MenuItem == null)
            {
                return NotFound();
            }

            return View(MenuItemVM);
        }


        //POST Delete MenuItem
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            string webRootPath = _hostingEnvironment.WebRootPath;
            MenuItem menuItem = await _db.MenuItem.FindAsync(id);

            if (menuItem != null)
            {
                var imagePath = Path.Combine(webRootPath, menuItem.Image.TrimStart('\\'));

                if (System.IO.File.Exists(imagePath))
                {
                    System.IO.File.Delete(imagePath);
                }
                _db.MenuItem.Remove(menuItem);
                await _db.SaveChangesAsync();

            }

            return RedirectToAction(nameof(Index));
        }
    }
}