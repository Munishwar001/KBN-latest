using Humanizer;
using KBN.Models.DIDModel;
using KBN.RepoHelper;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace KBN.Controllers
{
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

        public IActionResult GetData() //For Getting All the DID Data
        {
            try
            {
                List<DID_Assigning> data = _DIDHelper.GetData();
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
                return PartialView();
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost]
        public IActionResult Add(List<RangeViewModel> data) //For Add Data in DB
        {
            try
            {
                var result = _DIDHelper.AddData(data);

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
