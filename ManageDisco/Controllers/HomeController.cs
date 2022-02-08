using ManageDisco.Context;
using ManageDisco.Helper;
using ManageDisco.Model;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace ManageDisco.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HomeController : ControllerBase
    {
        private readonly DiscoContext _db;
        private readonly IConfiguration _config;

        public HomeController(DiscoContext db,
            IConfiguration config)
        {
            _db = db;
            _config = config;
        }

        // GET: api/<HomeController>
        [HttpGet]
        [Route("Info")]
        public async Task<IActionResult> GetHome()
        {
            Home homeData = new Home();
            
            List<Task> tasks = new List<Task>()
            {
                new Task(() =>
                {
                    homeData.Events =  _db.Events
                        .Where(x => x.Date.CompareTo(DateTime.Today) == 0 || x.Date.CompareTo(DateTime.Today) > 0)
                        .Select(x => new EventPartyImages(){ 
                            Id = x.Id,
                            Name = x.Name,
                            Description = x.Description,
                            Date = x.Date                           
                        })
                        .OrderByDescending(x => x.Date).ThenBy(x => x.Name)
                        .ToList();

                    var ftpUser = _config["Ftp:User"];
                    var ftpPass = _config["Ftp:Pass"];

                    homeData.Events.ForEach(x =>
                    {
                        var photos = _db.EventPhoto
                        .Where(e => (e.EventPhotoEventId == x.Id && e.EventPhotoType.EventPhotoDescription == EventPhotoDescriptionValues.EVENT_IMAGE_TYPE_COVER) /*||
                                (e.EventPhotoEventId == x.Id && e.EventPhotoType.EventPhotoDescription == EventPhotoDescriptionValues.EVENT_IMAGE_TYPE_PREVIEW)*/)
                        .OrderBy(e => e.EventPhotoType.EventPhotoTypeId)
                        .ToList();
                       
                       photos.ForEach(f =>
                       {
                           string base64Value = Convert.ToBase64String(HelperMethods.GetBytesFromStream(
                               HelperMethods.GetFileStreamToFtp(f.EventPhotoImagePath, ftpUser, ftpPass)));

                           x.LinkImage.Add(base64Value);
                       });                       
                    });
                })
            };

          
            tasks.ForEach(t =>
            {
               t.Start();               
            });
            Task.WaitAll(tasks.ToArray());
           
            return Ok(homeData);
        }


        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }


        [HttpPost]
        public void Post([FromBody] string value)
        {
        }


        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }


        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}
