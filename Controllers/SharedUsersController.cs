using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using ZachPerini_6._3A_HA.Models;
using ZachPerini_6._3A_HA.Repositories;

namespace ZachPerini_6._3A_HA.Controllers
{
    public class SharedUsersController : Controller
    {
        SharedUserRepository _sharedUsersRepository;
        BucketsRepository _bucketsRepository;
        private IConfiguration _config;
        private PubSubRepository _pubsubrepository;
        ArtefactsRepository _artefactsRepository;

        private string fileName;

        public SharedUsersController(ArtefactsRepository artefactsRepository, BucketsRepository bucketsRepository, IConfiguration config, PubSubRepository pubsubrepository, SharedUserRepository sharedUsersRepository)
        {
            _artefactsRepository = artefactsRepository;
            _bucketsRepository = bucketsRepository;
            _config = config;
            _pubsubrepository = pubsubrepository;
            _sharedUsersRepository = sharedUsersRepository;
        }

        public async Task<IActionResult> Index(string artefactId)
        {
            var list = await _sharedUsersRepository.GetUsers(artefactId);
            return View(list);
            //return View();
        }

        [HttpGet]
        [Authorize]
        public IActionResult Create(string artefactId)
        {
            var model = new SharedUser();
            model.Artefact_Id = artefactId;
            
            return View(model);
        }


        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(SharedUser user)
        {
            try
            {
                string fileName = user.Artefact_Id + ".pdf";
                user.Id = Guid.NewGuid().ToString();
                await _bucketsRepository.GrantAccess(fileName, user.Email);
                _sharedUsersRepository.AddUser(user);
                return View(user);
            }
            catch (Exception ex)
            {
                // Log the exception
                //_logger.LogError(ex, "An error occurred while processing Create action");

                // Store the error message in TempData
                TempData["ErrorMessage"] = "An error occurred while processing your request";

                // Redirect to the same view to display the error message
                return RedirectToAction(nameof(Create));
            }
        }
    }
}
