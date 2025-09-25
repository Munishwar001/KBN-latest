using KBN.Models.DIDModel;
using KBN.Models.SubscriberModels;
using KBN.RepoHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KBN.Controllers
{
    [Authorize]
    public class SubscriberController : Controller
    {
        public readonly SubscriberHelper _subscriberHelper; 
        public SubscriberController(SubscriberHelper subscriberHelper) 
        {
            _subscriberHelper  = subscriberHelper;
        }
        public IActionResult Index()
        {
            return View();
        }
          
        public IActionResult GetData(Subscriber? filter = null, int? pageNumber = 0, int? pageSize = 5) //For Getting All the DID Data
        {
            try
            {
                //var data = _DIDHelper.GetData(filter: filter, pageNumber: pageNumber, pageSize: pageSize);
                var data = _subscriberHelper.GetData(filter: filter, pageNumber: pageNumber, pageSize: pageSize);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        public IActionResult Add() // For giving the Add Form 
        {
            try
            {
                return PartialView();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Add([FromBody] Subscriber sb)
        {
            try
            {  if(sb == null)
                {
                    return BadRequest(new { success = false, mesage = "Enable to get data" });
                }
                 var result = _subscriberHelper.Add(sb);
                return Json(new { success = result.success , message = result.error });
            }
            catch(Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        public IActionResult Delete(int id) //For Delete Data in DB
        {
            try
            {
                var result = _subscriberHelper.Delete(id);

                return Json(new
                {
                    success = result,
                    errors = "Deleted Successfully"
                });
            }
            catch (Exception ex)
            {
                return Json(new
                {
                    success = false,
                    errors = new Dictionary<long, string> { { 0, ex.Message } }
                });
            }
        }

        public IActionResult Update(int id)
        {
            try
            {
                var currentData = _subscriberHelper.getUpdateList(id);
                return PartialView(currentData);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Update([FromBody] Subscriber sb)
        {
            try
            {
                var result = _subscriberHelper.UpdateData(sb);
                return Json(new { success = result.success, message = result.error });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
