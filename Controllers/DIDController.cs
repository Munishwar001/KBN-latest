using Humanizer;
using KBN.Models.DIDModel;
using KBN.RepoHelper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Security.Claims;

namespace KBN.Controllers
{
    [Authorize]
    public class DIDController : Controller
    {    

        public readonly DIDHelper _DIDHelper;

         public DIDController(DIDHelper DIDHelper)
        {
            _DIDHelper = DIDHelper;
        }
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult GetData(RangeViewModel? filter  = null,int? pageNumber = 0 , int? pageSize = 5 ) //For Getting All the DID Data
        {
            try
            {
                var data = _DIDHelper.GetData(filter: filter, pageNumber: pageNumber, pageSize: pageSize);
                 
                return PartialView(data);
            }
            catch (Exception ex)
            {  
                return BadRequest(ex.Message);
            }
        }

        public IActionResult Add() // For Getting the Add DID Form 
        {
            try
            {
                string loggedInUserEmail = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
                ViewBag.currentUser = loggedInUserEmail;
                return PartialView();
            }
            catch (Exception ex)    
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Add([FromBody] DIDRequest request) //For Add Data in DB
        {
            try
            {  
                string RecordBy = request.RecordBy;
                List<RangeViewModel> data = request.Data;
                var result = _DIDHelper.AddData(RecordBy ,data);

                return Json(new
                {
                    success = result.Success,
                    errors = result.Errors
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

        [HttpPost]
        public IActionResult Delete(long did) //For Add Data in DB
        {
            try
            {
                var result = _DIDHelper.DeleteDID(did);

                return Json(new
                {
                    success = result,
                    errors = "server Error"
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
        public IActionResult update(long did) //For update data form 
        {
            try
            {
                var data = _DIDHelper.GetUpdateDiD(did);
                return PartialView(data);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult update(RangeViewModel data) //For update data form 
        {
            try
            {
                var result  = _DIDHelper.updateData(data);
                return Json(new
                {
                    success = result.Success,
                    errors = result.Message
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
    }
}
