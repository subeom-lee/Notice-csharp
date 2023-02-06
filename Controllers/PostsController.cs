using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Printing;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure.Internal;
using Microsoft.Extensions.Hosting;
using Notice.Data;
using Notice.Models;
using AutoMapper;
using Newtonsoft.Json.Linq;
using Microsoft.IdentityModel.Tokens;

namespace Notice.Controllers
{
    public class PostsController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PostsController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Posts
        public async Task<IActionResult> Index(string searchString, int searchCategory, string sortType, int page = 1)
        {
            const int pageSize = 10;
            var posts = _context.Posts.AsQueryable();
            if (!String.IsNullOrEmpty(searchString))
            {
                posts = posts.Where(p => p.title.Contains(searchString) || p.contents.Contains(searchString));
            }
            if (String.IsNullOrEmpty(sortType) || sortType == "createdatetime")
            {
                posts = posts.OrderByDescending(p => p.CreatedDatetime);
            }
            else if (sortType == "viewcount")
            {
                posts = posts.OrderByDescending(p => p.ViewCount);
            }
            else if (sortType == "olddatetime")
            {
                posts = posts.OrderBy(p => p.CreatedDatetime);
            }
            if (searchCategory == 0)
            {
                posts = posts.OrderByDescending(p => p.CreatedDatetime);
            }
            else if (searchCategory > posts.Max(p => p.Category_id))
            {
                posts = posts.OrderByDescending(p => p.CreatedDatetime);
            }
            else if (searchCategory != 0)
            {
                posts = posts.Where(p => p.Category_id == searchCategory);
            }
            var totalPages = (int)Math.Ceiling((double)posts.Count() / pageSize);

            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Post, PostDto>();
            });

            IMapper mapper = config.CreateMapper();

            // Table 조회
            List<Post>? listItems = await posts.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

            // 목록 조회용 모델
            List<PostDto>? dest = mapper.Map<List<Post>?, List<PostDto>?>(listItems);
            foreach (PostDto item in dest)
            {
                var category = await _context.Categories
                //DB              찾는거
                .FirstOrDefaultAsync(p => p.Category_id == item.Category_id);
                item.Category_Value = category.CategoryValue;
            }

            var viewModel = new PostViewModel
            {
                Items = dest,
                CurrentPage = page,
                TotalPages = totalPages,
                SearchString = searchString,
            };

            return View(viewModel);
        }

        public class PostViewModel
        {
            public List<PostDto>? Items { get; set; }
            public int CurrentPage { get; set; }
            public int TotalPages { get; set; }
            public string SearchString { get; set; }
        }

        // GET: Posts/Details/5
        public async Task<IActionResult> Details(int? id, int page = 1)
        {
            ViewBag.CurrentPage = page;
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.post_id == id);
            if (post == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(p => p.Category_id == post.Category_id);

            _context.Add(post);


            post.ViewCount++;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            PostDto dto = new PostDto();
            dto.post_id = post.post_id;
            dto.title = post.title;
            dto.contents = post.contents;
            dto.CreatedDatetime = post.CreatedDatetime;
            dto.UpdatedDatetime = post.UpdatedDatetime;
            dto.ViewCount = post.ViewCount;
            dto.Category_id = post.Category_id;
            dto.Category_Value = category.CategoryValue;

            return View(dto);
        }

        // GET: Posts/Create
        public async Task<IActionResult> Create()
        {
            List<SelectListItem> items = new List<SelectListItem>();

            if (_context.Categories == null)
            {
                return NotFound();
            }

            List<Category> categories = await _context.Categories.ToListAsync();
            foreach (Category item in categories)
            {
                items.Add(new SelectListItem()
                {
                    Text = item.CategoryValue,
                    Value = item.Category_id.ToString(),
                });
            }

            ViewBag.Categories = items;

            return View();
        }

        // POST: Posts/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("post_id, title, contents, Category_id, CategoryValue")] Post post, Category category)
        {
            if (ModelState.IsValid)
            {
                post.Category_id = Convert.ToInt16(category.CategoryValue);
                _context.Add(post);
                //_context.Add(category);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }

            return View(post);
        }

        // GET: Posts/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            List<SelectListItem> items = new List<SelectListItem>();

            if (_context.Categories == null)
            {
                return NotFound();
            }

            List<Category> categories = await _context.Categories.ToListAsync();
            foreach (Category item in categories)
            {
                var selected = item.Category_id == post.Category_id ? true : false;
                items.Add(new SelectListItem()
                {
                    Text = item.CategoryValue,
                    Value = item.Category_id.ToString(),
                    Selected = selected
                });
            }

            ViewBag.Categories = items;

            return View(post);
        }

        // POST: Posts/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("post_id, title, contents, Category_id, CategoryValue")] Post post, Category category)
        {
            if (id != post.post_id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    post.UpdatedDatetime = DateTime.Now;
                    var postFromDb = await _context.Posts.FirstOrDefaultAsync(x => x.post_id == post.post_id);
                    postFromDb.title = post.title;
                    postFromDb.contents = post.contents;
                    postFromDb.UpdatedDatetime = DateTime.Now;
                    postFromDb.Category_id = Convert.ToInt16(category.CategoryValue);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PostExists(post.post_id))
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
            return View(post);
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(m => m.post_id == id);
            if (post == null)
            {
                return NotFound();
            }

            return View(post);
        }

        // POST: Posts/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Posts == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Posts'  is null.");
            }
            var post = await _context.Posts.FindAsync(id);
            if (post != null)
            {
                _context.Posts.Remove(post);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PostExists(int id)
        {
            return _context.Posts.Any(e => e.post_id == id);
        }
    }
}
