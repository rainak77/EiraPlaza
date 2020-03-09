﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using EiraPlaza.Data;
using EiraPlaza.Models;
using EiraPlaza.Utility;

namespace EiraPlaza.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = SD.ManagerUser)]

    public class CouponController : Controller
    {
        private readonly ApplicationDbContext _db;

        public CouponController( ApplicationDbContext db)
        {
            _db = db;
        }
        public async Task<IActionResult> Index()
        {

            return View(await _db.Coupon.ToListAsync());
        }


        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Coupon Coupons)
        {
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] p1 = null; 
                    using (var fs1=files[0].OpenReadStream())
                    {
                        using (var ms1=new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    Coupons.Picture = p1;
                }
                _db.Coupon.Add(Coupons);
                await _db.SaveChangesAsync();
                return RedirectToAction("Index");
            }
            return View(Coupons);
        }

        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var SelectedCoupon =await _db.Coupon.SingleOrDefaultAsync(m=>m.Id==id);
            if (SelectedCoupon == null)
            {
                return NotFound();
            }
            return View(SelectedCoupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Coupon coupon)
        {
            if (coupon.Id == 0)
            {
                return NotFound();
            }
            var couponFromDb = await _db.Coupon.Where(c => c.Id == coupon.Id)
                                               .FirstOrDefaultAsync();
            if (ModelState.IsValid)
            {
                var files = HttpContext.Request.Form.Files;
                if (files.Count > 0)
                {
                    byte[] p1 = null;
                    using (var fs1 = files[0].OpenReadStream())
                    {
                        using (var ms1 = new MemoryStream())
                        {
                            fs1.CopyTo(ms1);
                            p1 = ms1.ToArray();
                        }
                    }
                    couponFromDb.Picture = p1;
                }
                couponFromDb.MinimumAmount = coupon.MinimumAmount;
                couponFromDb.Name = coupon.Name;
                couponFromDb.IsActive = coupon.IsActive;
                couponFromDb.Discount = coupon.Discount;
                couponFromDb.CouponType = coupon.CouponType;

                await _db.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            return View(coupon);
        }

        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }
            var SelectedCoupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            if (SelectedCoupon == null)
            {
                return NotFound();
            }

            return View(SelectedCoupon);
        }
        [HttpPost,ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int? id)
        {
            var SelectedCoupon = await _db.Coupon.SingleOrDefaultAsync(m => m.Id == id);
            _db.Coupon.Remove(SelectedCoupon);
            await _db.SaveChangesAsync();
            return RedirectToAction(nameof(Index));

        }
    }
}