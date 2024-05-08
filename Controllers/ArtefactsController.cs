using Aspose.Words;
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Google.Cloud.Firestore;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ZachPerini_6._3A_HA.Models;
using ZachPerini_6._3A_HA.Repositories;
using Microsoft.AspNetCore.Authentication;
using System.Diagnostics;

namespace ZachPerini_6._3A_HA.Controllers
{
    public class ArtefactsController : Controller
    {

        //now that we registered the repository witht the services connection
        //we can ask for it to be recieved here and consumed using DEPENDENY INJECTION
        //this is done to promote code efficiency

        private ArtefactsRepository _artefactsRepository;
        private BucketsRepository _bucketsRepository;
        private IConfiguration _config;
        private PubSubRepository _pubsubrepository;
        private SharedUserRepository _shareduserRepository;
        public ArtefactsController(ArtefactsRepository artefactRepository, BucketsRepository bucketsRepository, IConfiguration config, PubSubRepository pubsubrepository, SharedUserRepository shareduserRepository)
        {
            _artefactsRepository = artefactRepository;
            _bucketsRepository = bucketsRepository;
            _config = config;
            _pubsubrepository = pubsubrepository;
            _shareduserRepository = shareduserRepository;
        }

        [Authorize]
        public IActionResult MembersLanding()
        {
            return View();
        }

        [Authorize]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(); //erases the cookie
            return RedirectToAction("Index"); //redirects the user to the home page
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //list of artefact documents:
        public async Task<IActionResult> Index()
        {
            var list = await _artefactsRepository.GetArtefacts();
            return View(list);
        }

        //shared with me artefacts page:
        [Authorize]
        public async Task<IActionResult> SharedArtefacts(string email)
        {
            var list = await _shareduserRepository.GetSharedArtefactsForUser(email);
            return View(list);
        }

        //This is called to load the page where the user will be typing the inputs
        [HttpGet]
        [Authorize]
        public IActionResult Create() { return View(); }

        //This is called when the user has finished typing the data and clicked on submit
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(Artefact artefact, IFormFile file, SharedUser user)
        {
            if (file == null || file.Length == 0)
            {
                // Handle the case where no file was uploaded
                // You can return a specific error message or redirect to an error page
                return Content("No file was uploaded.");
            }

            string bucketName = _config["bucket"];
            string globalId = Guid.NewGuid().ToString();
            string uniqueFilename = globalId + Path.GetExtension(file.FileName);
            string recipient = User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value;

            using (MemoryStream userFile = new MemoryStream())
            {
                await file.CopyToAsync(userFile);

                // Upload the file to the cloud storage repository
                await _bucketsRepository.UploadFile(uniqueFilename, userFile);

                // Grant access to the recipient
                await _bucketsRepository.GrantAccess(uniqueFilename, recipient);

                // Set artefact properties
                artefact.file = $"https://storage.cloud.google.com/{bucketName}/{uniqueFilename}";
                artefact.Id = globalId;
                artefact.DateUploaded = Timestamp.FromDateTime(DateTime.UtcNow);
                artefact.Author = User.Identity.Name;
                artefact.status = "0";

                // Add the artefact to the repository
                _artefactsRepository.AddArtefact(artefact);

                // Publish the artefact ID with the email as an attribute
                await _pubsubrepository.Publish(globalId, User.FindFirst("http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress").Value);

                // Redirect to the index page
                return RedirectToAction("Index");
            }
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> UpdateStatus(string artefactId)
        {
            var list = await _artefactsRepository.GetArtefacts();
            var myArtefactToEdit = list.SingleOrDefault(x => x.Id == artefactId);
            if (myArtefactToEdit.Author != User.Identity.Name)
            {
                return RedirectToAction("Index");
            }
            myArtefactToEdit.status = "1";

            // Save changes to the database
            await _artefactsRepository.ChangeStatus(myArtefactToEdit);
            return Ok();
        }



        [Authorize]
        public async Task<IActionResult> Delete(string artefactId)
        {

            await _artefactsRepository.DeleteArtefact(artefactId);
            return RedirectToAction("Index");
        }
    }
}
