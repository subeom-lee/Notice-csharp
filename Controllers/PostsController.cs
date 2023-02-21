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
using Microsoft.CodeAnalysis;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

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
            if (searchCategory != 0 && posts.Where(p => p.Category_id == searchCategory).Any())
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
                var post = await _context.Attachfiles
                    .FirstOrDefaultAsync(p => p.post_id == item.post_id);
                if (post != null)
                {
                    item.IsFile = true;
                }
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
                .FirstOrDefaultAsync(p => p.post_id == id);
            if (post == null)
            {
                return NotFound();
            }

            var category = await _context.Categories
                .FirstOrDefaultAsync(p => p.Category_id == post.Category_id);
            _context.Add(post);

            // 현재 페이지에 해당하는 post_id값을 가지고 있는 파일들 배열로 가져오기
            var attachfiles = await _context.Attachfiles
                .Where(p => p.post_id == id)
                .ToListAsync();

            post.ViewCount++;
            _context.Posts.Update(post);
            await _context.SaveChangesAsync();

            // 배열에 요소 넣기
            List<AttachfileDto> attachfileDto = new List<AttachfileDto>();
            foreach (var attachfile in attachfiles)
            {
                attachfileDto.Add(new AttachfileDto
                {
                    File_id = attachfile.File_id,
                    File_name = attachfile.File_name,
                    File_path = attachfile.File_path,
                });
            }

            // PostDto에 배열에 요소 넣은 모델 넣기
            PostDto PostDto = new PostDto();
            PostDto.post_id = post.post_id;
            PostDto.title = post.title;
            PostDto.contents = post.contents;
            PostDto.CreatedDatetime = post.CreatedDatetime;
            PostDto.UpdatedDatetime = post.UpdatedDatetime;
            PostDto.ViewCount = post.ViewCount;
            PostDto.Category_id = post.Category_id;
            PostDto.Category_Value = category.CategoryValue;
            PostDto.Attachfiles = attachfileDto;

            // 첨부파일이 없을 때 허용
            if (attachfiles == null)
            {
                return View(PostDto);
            }

            return View(PostDto);
        }

        // 파일 다운로드
        public async Task<ActionResult?> Download(int Downloadfile)
        {
            // View에서 File_id값을 넘겨받은걸로 파일 객체 가져오기
            var fileId = await _context.Attachfiles.FirstOrDefaultAsync(p => p.File_id == Downloadfile);
            if (fileId != null)
            {
                string fileName = fileId.File_name;
                string filePath = fileId.File_path;
                byte[] bytes = System.IO.File.ReadAllBytes(filePath);
                return File(bytes, "application/octet-stream", fileName);
            }

            return null;
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
        public async Task<IActionResult> Create([FromForm] Post post, [FromForm] Category category, ICollection<IFormFile>? uploadfile = null)
        {
            if (ModelState.IsValid)
            {
                post.Category_id = Convert.ToInt16(category.CategoryValue);
                _context.Add(post);
                await _context.SaveChangesAsync();
            }

            var files = HttpContext.Request.Form.Files;
            var uploadPath = "d:\\upload";
            try
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var convertFileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + file.FileName;
                    var filePath = System.IO.Path.Combine(uploadPath, convertFileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                    }
                    // Attachfile 모델에 파일 데이터 저장
                    var attachFile = new Attachfile
                    {
                        File_data = System.IO.File.ReadAllBytes(filePath),
                        File_name = file.FileName,
                        File_path = uploadPath + '/' + convertFileName,
                        post_id = post.post_id
                    };

                    _context.Attach(attachFile);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
            return RedirectToAction(nameof(Index));
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

        [HttpGet]
        public IActionResult GetFiles(int id)
        {
            var files = _context.Attachfiles.Where(p => p.post_id == id).ToList();
            var result = files.Select(p => new
            {
                file_id = p.File_id,
                file_name = p.File_name,
                file_size = p.File_data.Length
            });
            Console.WriteLine(result);
            return Json(result);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [FromForm] Post post, [FromForm] Category category, [FromForm] string? File_id = null, ICollection<IFormFile>? attachfile = null)
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
                    var postFromDb = await _context.Posts.FirstOrDefaultAsync(p => p.post_id == post.post_id);
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
            }
            var files = HttpContext.Request.Form.Files;
            var uploadPath = "d:\\upload";
            try
            {
                for (var i = 0; i < files.Count; i++)
                {
                    var file = files[i];
                    var convertFileName = DateTime.Now.ToString("yyyyMMddHHmmss_") + file.FileName;
                    var filePath = System.IO.Path.Combine(uploadPath, convertFileName);
                    using (var stream = System.IO.File.Create(filePath))
                    {
                        await file.CopyToAsync(stream);
                    }
                    // Attachfile 모델에 파일 데이터 저장
                    var attachFile = new Attachfile
                    {
                        File_data = System.IO.File.ReadAllBytes(filePath),
                        File_name = file.FileName,
                        File_path = uploadPath + '/' + convertFileName,
                        post_id = post.post_id
                    };

                    _context.Attach(attachFile);
                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
            if (File_id != null)
            {
                string[] fileIds = File_id.Split(',');
                var array = fileIds.Skip(1).ToArray();
                foreach (var deleteFileId in array)
                {
                    var intFileId = int.Parse(deleteFileId);
                    var filePaths = await _context.Attachfiles
                        .Where(p => p.File_id == intFileId)
                        .Select(p => p.File_path)
                        .ToListAsync();
                    foreach (var filePath in filePaths)
                    {
                        if (System.IO.File.Exists(filePath))
                        {
                            System.IO.File.Delete(filePath);
                        }
                    }
                    var deleteAttachFileId = await _context.Attachfiles.FindAsync(intFileId);
                    if (deleteAttachFileId != null)
                    {
                        _context.Remove(deleteAttachFileId);
                    }
                    await _context.SaveChangesAsync();
                }
            }
            return RedirectToAction(nameof(Index));
        }

        // GET: Posts/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Posts == null)
            {
                return NotFound();
            }

            var post = await _context.Posts
                .FirstOrDefaultAsync(p => p.post_id == id);
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
            // model을 이용한 실제 파일 삭제
            // Attachfiles모델에 지우려는 post_id가 가지고 있는 file_path를 배열로 선택
            var filePaths = await _context.Attachfiles
                .Where(p => p.post_id == id)
                .Select(p => p.File_path)
                .ToListAsync();
            // 배열로 된 file_path를 한개씩 나눠서 실제 파일이 존재한다면 삭제
            foreach (var filePath in filePaths)
            {
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            // File_id 삭제
            // Attachfiles모델에 지우려는 post_id가 가지고 있는 file_id를 배열로 선택
            var attachfiles = await _context.Attachfiles
                .Where(p => p.post_id == id)
                .ToListAsync();
            // 배열로 된 file_id를 한개씩 나눠서 삭제
            foreach (var attachfile in attachfiles)
            {
                _context.Attachfiles.Remove(attachfile);
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
            return _context.Posts.Any(p => p.post_id == id);
        }
    }
}