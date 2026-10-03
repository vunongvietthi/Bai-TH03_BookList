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
        public IActionResult QueryDemo(int? id, int mcd = 1)
        {
            List<SachQuery> queries = new List<SachQuery>
    {
        new SachQuery { Id = 1, QueryName = "1. Danh mục chủ đề (mã, tên)" },
        new SachQuery { Id = 2, QueryName = "2. Danh mục chủ đề (mã, tên, số lượng sách)" },
        new SachQuery { Id = 3, QueryName = "3. Danh mục chủ đề có sách (mã, tên)" },
        new SachQuery { Id = 4, QueryName = "4. Danh mục sách thuộc một chủ đề cụ thể" },
        new SachQuery { Id = 5, QueryName = "5. Danh mục 5 sách mới (mã, tên, ảnh)" },
        new SachQuery { Id = 6, QueryName = "6. Danh mục 5 sách bán chạy (mã, tên, ảnh)" },
        new SachQuery { Id = 7, QueryName = "7. Danh mục quảng cáo còn hạn" },
        new SachQuery { Id = 8, QueryName = "8. Danh mục tác giả của cuốn sách mã 2" },
        new SachQuery { Id = 9, QueryName = "9. Danh sách đơn hàng đã giao" }
    };

            // Giữ lại giá trị được chọn trên Dropdown sau khi reload
            ViewBag.Queries = new SelectList(queries, "Id", "QueryName", id);

            if (id.HasValue && id.Value == 1)
                return View(GetChuDe());
            if (id.HasValue && id.Value == 2)
                return View(GetChuDeVaSoLuongSach());
            if (id.HasValue && id.Value == 3)
                return View(GetChuDeCoSach());
            if (id.HasValue && id.Value == 4)
                return View(GetSachTheoChuDe(mcd));
            if (id.HasValue && id.Value == 5)
                return View(GetSachMoi());
            if (id.HasValue && id.Value == 6)
                return View(GetSachBanChay());
            if (id.HasValue && id.Value == 7)
                return View(GetQuangCaoConHan());
            if (id.HasValue && id.Value == 8)
                return View(GetTacGiaCuaSach(2));
            if (id.HasValue && id.Value == 9)
                return View(GetDonHangDaGiao());

            return View();
        }

        // 1. Chủ đề: mã, tên
        private List<Dictionary<string, object>> GetChuDe()
        {
            var query = _context.ChuDes
                .Select(c => new { c.Mcd, c.TenChuDe })
                .ToList();
            return ToDictionaryList(query);
        }

        // 2. Chủ đề: mã, tên, số lượng sách
        private List<Dictionary<string, object>> GetChuDeVaSoLuongSach()
        {
            var query = _context.ChuDes
                .Select(c => new { c.Mcd, c.TenChuDe, SoLuongSach = c.Saches.Count() })
                .ToList();
            return ToDictionaryList(query);
        }

        // 3. Chủ đề có sách
        private List<Dictionary<string, object>> GetChuDeCoSach()
        {
            var query = _context.ChuDes
                .Where(c => c.Saches.Any())
                .Select(c => new { c.Mcd, c.TenChuDe })
                .ToList();
            return ToDictionaryList(query);
        }

        // 4. Sách thuộc một chủ đề cụ thể
        private List<Dictionary<string, object>> GetSachTheoChuDe(int mcd)
        {
            var query = _context.Saches
                .Where(s => s.Mcd == mcd)
                .Select(s => new { s.Ms, s.TenSach, s.DonGia, s.HinhMinhHoa })
                .ToList();
            return ToDictionaryList(query);
        }

        // 5. 5 sách mới nhất theo ngày cập nhật
        private List<Dictionary<string, object>> GetSachMoi()
        {
            var query = _context.Saches
                .OrderByDescending(s => s.NgayCapNhat)
                .Take(5)
                .Select(s => new { s.Ms, s.TenSach, s.HinhMinhHoa })
                .ToList();
            return ToDictionaryList(query);
        }

        // 6. 5 sách bán chạy theo tổng số lượng trong chi tiết đơn hàng
        private List<Dictionary<string, object>> GetSachBanChay()
        {
            var query = _context.Saches
                .Where(s => s.CtDatHangs.Any())
                .OrderByDescending(s => s.CtDatHangs.Sum(ct => ct.SoLuong))
                .Take(5)
                .Select(s => new { s.Ms, s.TenSach, s.HinhMinhHoa })
                .ToList();
            return ToDictionaryList(query);
        }

        // 7. Quảng cáo còn hạn
        private List<Dictionary<string, object>> GetQuangCaoConHan()
        {
            var now = DateTime.Now;
            var query = _context.QuangCaos
                .Where(q => q.NgayBatDau <= now && q.NgayHetHan >= now)
                .Select(q => new { q.Stt, q.TenCty, q.HinhMinhHoa, q.Href, q.NgayBatDau, q.NgayHetHan })
                .ToList();
            return ToDictionaryList(query);
        }

        // 8. Tác giả tham gia viết một cuốn sách
        private List<Dictionary<string, object>> GetTacGiaCuaSach(int ms)
        {
            var query = _context.ThamGia
                .Where(t => t.Ms == ms)
                .Select(t => new { t.MtgNavigation.Mtg, t.MtgNavigation.TenTacGia })
                .ToList();
            return ToDictionaryList(query);
        }

        // 9. Đơn hàng đã giao
        private List<Dictionary<string, object>> GetDonHangDaGiao()
        {
            var query = _context.DonDatHangs
                .Where(d => d.DaGiaoHang == true)
                .Select(d => new { d.Sdh, d.Mkh, d.NgayDatHang, d.NgayGiaoHang, d.TriGia })
                .ToList();
            return ToDictionaryList(query);
        }

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