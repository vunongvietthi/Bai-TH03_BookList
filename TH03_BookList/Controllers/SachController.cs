using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc;
using TH03_BookList.Models;

namespace TH03_BookList.Controllers
{
    public class SachController : Controller
    {
        private readonly AppDbContext _context;

        public SachController(AppDbContext context)
        {
            _context = context;
        }

        #region "ChuDe"

        // GET: Sach/ChuDe
        public ActionResult ChuDe()
        {
            List<ChuDe> dsChuDe = _context.ChuDes.ToList();
            return View(dsChuDe);
        }

        // GET: Sach/ChuDeEdit/5  (id=0 => Thêm mới, id>0 => Sửa)
        public ActionResult ChuDeEdit(int id)
        {
            ChuDe? chuDe = new ChuDe();
            chuDe.Mcd = 0;

            if (id == 0) // Nếu là thêm chủ đề
                return View(chuDe);

            // Nếu là sửa chủ đề
            chuDe = _context.ChuDes.Find(id);
            if (chuDe == null)
            {
                return RedirectToAction(nameof(ChuDe));
            }
            return View(chuDe);
        }

        // POST: Sach/ChuDeEdit
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChuDeEdit(ChuDe chude)
        {
            if (ModelState.IsValid)
            {
                if (chude.Mcd == 0) // Thêm mới
                {
                    _context.ChuDes.Add(chude);
                    TempData["SuccessMessage"] = "Thêm mới thành công!";
                }
                else // Cập nhật
                {
                    _context.ChuDes.Update(chude);
                }
                _ = await _context.SaveChangesAsync();
                return RedirectToAction("ChuDe");
            }
            return View(chude);
        }

        // POST: Sach/ChuDeDelete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult ChuDeDelete(int id, IFormCollection collection)
        {
            try
            {
                var chude = _context.ChuDes.Find(id);
                if (chude != null)
                {
                    _context.ChuDes.Remove(chude);
                    _context.SaveChanges();
                    TempData["SuccessMessage"] = "Xóa chủ đề thành công!";
                }
                return RedirectToAction(nameof(ChuDe));
            }
            catch
            {
                return View();
            }
        }

        #endregion
        #region "QueryDemo"

        // GET: Sach/QueryDemo
        public IActionResult QueryDemo(int? id)
        {
            List<SachQuery> queries = new List<SachQuery>
    {
        new SachQuery { Id = 1, QueryName = "Lấy tất cả danh mục chủ đề" },
        new SachQuery { Id = 2, QueryName = "Lấy danh mục chủ đề có sách" },
        new SachQuery { Id = 3, QueryName = "Lấy danh mục sách có tt NXB, không mô tả" },
        new SachQuery { Id = 4, QueryName = "Lấy danh mục sách có tt NXB, tác giả, không mô tả" },
        new SachQuery { Id = 5, QueryName = "Lấy danh mục sách mới" },
        new SachQuery { Id = 7, QueryName = "Lấy danh mục sách bán chạy" },
        new SachQuery { Id = 9, QueryName = "Lấy danh mục quảng cáo còn hạn" }
    };

            // Giữ lại giá trị được chọn trên Dropdown sau khi reload
            ViewBag.Queries = new SelectList(queries, "Id", "QueryName", id);

            if (id.HasValue && id.Value == 1)
                return View(GetChuDesAsync());
            if (id.HasValue && id.Value == 2)
                return View(GetChuDesCoSach());
            if (id.HasValue && id.Value == 3)
                return View(GetSachs_NXB());
            if (id.HasValue && id.Value == 4)
                return View(GetSachs_NXB_TG());
            if (id.HasValue && id.Value == 5)
                return View(GetSachMoi());
            if (id.HasValue && id.Value == 7)
                return View(GetSachBanChay());
            if (id.HasValue && id.Value == 9)
                return View(GetQuangCaoConHan());

            return View(null);
        }

        // Lấy tất cả danh mục chủ đề
        private List<Dictionary<string, object>> GetChuDesAsync()
        {
            var query = _context.ChuDes.Select(c => new
            {
                c.Mcd,
                c.TenChuDe
            }).ToList();

            return ToDictionaryList(query);
        }

        // Lấy danh sách các chủ đề đã có sách, kèm số lượng sách
        private List<Dictionary<string, object>> GetChuDesCoSach()
        {
            var query = _context.ChuDes.Select(c => new
            {
                TenChuDe = c.TenChuDe,
                TongSoSach = c.Saches.Count(),
                TongLuotBan = c.Saches.Sum(s => (int?)s.SoLuongBan) ?? 0
            }).Where(x => x.TongSoSach > 0).ToList();

            return ToDictionaryList(query);
        }

        // Lấy danh mục sách có thông tin NXB, không có mô tả
        private List<Dictionary<string, object>> GetSachs_NXB()
        {
            var query = _context.Saches.Select(s => new
            {
                s.Ms,
                s.TenSach,
                s.DonGia,
                TenNhaXuatBan = s.MnxbNavigation != null ? s.MnxbNavigation.TenNhaXuatBan : null
            }).ToList();

            return ToDictionaryList(query);
        }

        // Lấy danh mục sách có thông tin NXB, tác giả, không có mô tả
        private List<Dictionary<string, object>> GetSachs_NXB_TG()
        {
            var query = _context.Saches.Select(s => new
            {
                s.Ms,
                s.TenSach,
                s.DonGia,
                TenNhaXuatBan = s.MnxbNavigation != null ? s.MnxbNavigation.TenNhaXuatBan : null,
                TacGia = string.Join(", ", s.ThamGia.Select(tg => tg.MtgNavigation.TenTacGia))
            }).ToList();

            return ToDictionaryList(query);
        }

        // Lấy danh mục sách mới (10 sách cập nhật gần nhất)
        private List<Dictionary<string, object>> GetSachMoi()
        {
            var query = _context.Saches
                .OrderByDescending(s => s.NgayCapNhat)
                .Take(10)
                .Select(s => new
                {
                    s.Ms,
                    s.TenSach,
                    s.NgayCapNhat
                }).ToList();

            return ToDictionaryList(query);
        }

        // Lấy danh mục sách bán chạy (10 sách bán nhiều nhất)
        private List<Dictionary<string, object>> GetSachBanChay()
        {
            var query = _context.Saches
                .OrderByDescending(s => s.SoLuongBan)
                .Take(10)
                .Select(s => new
                {
                    s.Ms,
                    s.TenSach,
                    s.SoLuongBan
                }).ToList();

            return ToDictionaryList(query);
        }

        // Lấy danh mục quảng cáo còn hạn
        private List<Dictionary<string, object>> GetQuangCaoConHan()
        {
            var query = _context.QuangCaos
                .Where(q => q.NgayHetHan >= DateTime.Now)
                .Select(q => new
                {
                    q.Stt,
                    q.TenCty,
                    q.NgayBatDau,
                    q.NgayHetHan
                }).ToList();

            return ToDictionaryList(query);
        }

        // Hàm dùng chung: chuyển kết quả truy vấn (kiểu vô danh) sang Dictionary
        private List<Dictionary<string, object>> ToDictionaryList<T>(List<T> vmList)
        {
            var resultList = vmList.Select(item => item!.GetType()
                .GetProperties()
                .ToDictionary(
                    p => p.Name,
                    p => p.GetValue(item, null) ?? "NULL")
                ).ToList();
            return resultList;
        }

        #endregion
    }
}