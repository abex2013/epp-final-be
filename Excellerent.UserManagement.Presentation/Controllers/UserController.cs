using Excellerent.SharedModules.DTO;
using Excellerent.Usermanagement.Domain.Interfaces.ServiceInterfaces;
using Microsoft.AspNetCore.Mvc;
using AutoMapper;
using Excellerent.Usermanagement.Domain.Entities;
using Excellerent.UserManagement.Presentation.Models.GetModels;
using Excellerent.UserManagement.Presentation.Models.PostModel;
using Excellerent.UserManagement.Presentation.Models.PutModels;
using Excellerent.UserManagement.Presentation.Models.Validations;
using Excellerent.UserManagement.Presentation.Validations;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;
using Excellerent.Usermanagement.Domain.Models;
using Excellerent.UserManagement.Presentation.Filters;
using Excellerent.UserManagement.Presentaion.AuthTokenServices;
using Excellerent.UserManagement.Presentation.Helper;
using System.Security.Policy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Excellerent.UserManagement.Presentation.AccoutService;

namespace Excellerent.UserManagement.Presentation.Controllers
{
    [ApiVersion("1.0")]
    [Route("api/v{version:apiVersion}/[controller]")]
    [ApiController]

    [AllowAnonymous]
    public class UserController : Controller
    {
        private IUserService userService { get; }

        private readonly IAuthentService _authentServices;
        private IAuthTokenService _authService { get; }
        private IMapper mapper { get; }

        private readonly IMapper _mapper;
        private UserValidator validator { get; }

        private readonly ChangePasswordValidator _cpValidator;

        private UserPutModelValidator userPutModelvalidator { get; }
        private IOptions<MailSenderOptions> _smtpOption;
        private IConfiguration _config;
        private UserCredentialValidations _userCredentialValidation;
        private readonly IMapper _mapper2;
     
        
        public UserController(IUserService userService, IAuthTokenService authService,
            IMapper mapper, IAuthentService authentServices, IOptions<MailSenderOptions> smtpOption,
            IConfiguration configuration)
        {
            this.userService = userService;
            this.mapper = mapper;
            validator = new UserValidator();
            userPutModelvalidator = new UserPutModelValidator();
            _userCredentialValidation = new UserCredentialValidations();
            _authService = authService;
            _smtpOption = smtpOption;
            _config = configuration;
            _authentServices = authentServices;

            _cpValidator = new ChangePasswordValidator();

            var config = new MapperConfiguration(cfg => cfg.CreateMap<UserLogInPostModel, UserEntity>());
            _mapper = new Mapper(config);
            var config2 = new MapperConfiguration(cfg => cfg.CreateMap<UserEntity, LoginViewModel>());
            _mapper2 = new Mapper(config2);
        }

        [Authorize]
        [TypeFilter(typeof(EPPAutorizeFilter), Arguments = new object[] { "View_User" })]
        [HttpGet]
        public async Task<ResponseDTO> GetAll()
        {
            var users = await userService.GetUsers();
            var mappedUsers = users.Select(u => mapper.Map<UserGetModel>(u));
            return new ResponseDTO(ResponseStatus.Success, "", mappedUsers);
        }

        [Authorize]
        [TypeFilter(typeof(EPPAutorizeFilter), Arguments = new object[] { "View_User" })]
        [HttpGet("{id}")]
        public async Task<ResponseDTO> Get(Guid id)
        {
            var user = await userService.GetUser(id);
            return new ResponseDTO(ResponseStatus.Success, "", mapper.Map<UserGetModel>(user));
        }



        [HttpPost]
        public async Task<ResponseDTO> Post(UserPostModel user)
        {
            var validator = this.validator.Validate(user);
            if (!validator.IsValid)
                return new ResponseDTO(ResponseStatus.Error, validator.ToString(), null);

            var userExist = await userService.GetUserByEmployeeId(user.EmployeeId);
            if (userExist != null && userExist.Any())
                return new ResponseDTO(ResponseStatus.Error, "Employee already registered as user", null);
            
            var entity = mapper.Map<UserEntity>(user);
            string plainPassword = PasswordHelper.GenerateDefaultPassword();
            entity.Password = PasswordCryptographyPbkdf2.HashPassword(plainPassword);
            var dto = await userService.CreateUser(entity, user.GroupIds);
            if(dto.ResponseStatus == ResponseStatus.Success)
            {
                entity.Password = plainPassword;
                NotifyUser(entity, UserNotificationType.AccountCreated);

            }
            return dto;
        }

        private async void NotifyUser(UserEntity user, UserNotificationType type)
        {
            MailSenderOptions sender = this._smtpOption.Value;
            //sender.Password = _config["smtp:Password"];
           await MailHelper.SendAccountNotificationMail(sender, user, type);

        }

        [Authorize]
        [TypeFilter(typeof(EPPAutorizeFilter), Arguments = new object[] { "Update_User" })]
        [HttpPut]
        public async Task<ResponseDTO> Put(UserPutModel user)
        {
            var validator = this.userPutModelvalidator.Validate(user);
            if (!validator.IsValid)
                return new ResponseDTO(ResponseStatus.Error, validator.ToString(), null);
            return await userService.Update(mapper.Map<UserEntity>(user));
        }

       [Authorize]
        [TypeFilter(typeof(EPPAutorizeFilter), Arguments = new object[] { "View_User" })]
        [HttpGet("GetUsersForDashboard")]
        public async Task<PredicatedResponseDTO> GetAllUsersDashboard(string username, int pageIndex, int pageSize)
        {
            return await userService.GetUserDashBoardList(username, pageIndex, pageSize);
        }


        [AllowAnonymous]
        [HttpPost("logIn")]
        public async Task<IActionResult> Login([FromBody] UserLogInPostModel user)
        {
            var userLogInData = _mapper.Map<UserEntity>(user);
            var validationResult = _userCredentialValidation.Validate(userLogInData);
            if (!validationResult.IsValid)
                return BadRequest(new ResponseDTO { Data = null, Message = validationResult.ToString(), ResponseStatus = ResponseStatus.Error, Ex = null });

            UserEntity userData = await userService.Authenticate(user.Email, user.Password);

            if (userData == null || !PasswordCryptographyPbkdf2.VerifyPassword(userData.Password,user.Password)) 
                return Unauthorized(new ResponseDTO { Data = null, Message = "Unauthorized", ResponseStatus = ResponseStatus.Error, Ex = null });

            DateTime lastActivityDate = userData?.LastActivityDate != null ? Convert.ToDateTime(userData.LastActivityDate) : userData.CreatedDate;
            if (userData.IsPasswordGenerated && (DateTime.Now - lastActivityDate).Days > 5)
                return Unauthorized(new ResponseDTO { Data = null, Message = "Your password is expired. Please contact the admin.", ResponseStatus = ResponseStatus.Error, Ex = null });

            var token = _authentServices.Authenticate(userData);

            var userRes = _mapper2.Map<LoginViewModel>(userData);
            userRes.Token = token;
            if (userData.IsPasswordGenerated)
            {
                return Ok(new ResponseDTO { Data = userRes, Message = "Logged in successfully, you need to change your password to continue", ResponseStatus = ResponseStatus.Info });
            }
            return Ok(new ResponseDTO { Data = userRes, Message = "Logged in successfully", ResponseStatus = ResponseStatus.Success });
        }
    

    [HttpGet("GetEmployeesNotInUsers")]
        public async Task<ResponseDTO> GetEmployeesNotInUser()
        {
            return await this.userService.GetEmployeesNotInUsers();
        }


        [HttpGet("LoadUsersNotAssginedToGroup")]
        public async Task<ResponseDTO> LoadUsersNotAssginedToGroup(Guid groupId) 
        {
             return await this.userService.LoadUsersNotGroupedInGroup(groupId);
        }

        [AllowAnonymous]
        [HttpGet("UserAuthToken")]
        public async Task<IActionResult> AuthToken(string email)
        {
            var user = await userService.ValidateUser(email);

            if (user == null)
                return Unauthorized(new ResponseDTO { Data = null, Message = "Unauthorized", ResponseStatus = ResponseStatus.Error, Ex = null });

            var token = _authService.AuthToken(user);
            AuthTokenResponseModel authTokenResponseModel = new AuthTokenResponseModel();

            authTokenResponseModel.Token = token;
            authTokenResponseModel.EmployeeGuid = user.EmployeeId;
            authTokenResponseModel.Guid = user.Guid;
            authTokenResponseModel.Email = user.Email;


            return Ok(new ResponseDTO { Data = authTokenResponseModel, Message = "Logged in and successfully generated token", ResponseStatus = ResponseStatus.Success });
        }

        [HttpDelete]
        public async Task<ActionResult<ResponseDTO>> RemoveUser(Guid userGuid)
        {
            return await userService.RemoveUser(userGuid);
         }


        [HttpPut("ChangePassword")]
        public async Task<IActionResult> ChangePassword(ChangePasswordPostDto cppd)
        {
            var validator = this._cpValidator.Validate(cppd);
            if (!validator.IsValid)
                return Unauthorized(new ResponseDTO { Data = null, Message = validator.ToString(), ResponseStatus = ResponseStatus.Error, Ex = null });

            var userID = await userService.GetUserGuidByEmail(cppd.Email);
            var user = await userService.GetUser(userID);

            if (user == null || !PasswordCryptographyPbkdf2.VerifyPassword(user.Password, cppd.OldPassword))
                return Unauthorized(new ResponseDTO { Data = null, Message = "Unauthorized", ResponseStatus = ResponseStatus.Error, Ex = null });

            string newPasswordHashed = PasswordCryptographyPbkdf2.HashPassword(cppd.Password);
            await userService.ChangePassword(user, newPasswordHashed);
            return Ok(new ResponseDTO { Data = null, Message = "Password updated sucessfully", ResponseStatus = ResponseStatus.Success });

        }
        [HttpPut("ResetPassword")]
        public async Task<ActionResult<ResponseDTO>> ResetPassword(string Email)
        {
            if (Email == string.Empty)
                return BadRequest(new ResponseDTO { Data = null, Message = "Email is required", ResponseStatus = ResponseStatus.Error, Ex = null });
            
            var user = await userService.GetUserByEmail(Email);
            if(user == null)
                return Unauthorized(new ResponseDTO { Data = null, Message = "Unknown user", ResponseStatus = ResponseStatus.Error, Ex = null });

            string plainPassword = PasswordHelper.GenerateDefaultPassword();
            string hashedPassword = PasswordCryptographyPbkdf2.HashPassword(plainPassword);
            var dto = await userService.ResetPassword(user.Email, hashedPassword);
            if (dto.ResponseStatus == ResponseStatus.Success)
            {
                user.Password = plainPassword;
                NotifyUser(user, UserNotificationType.PasswordReset);

            }
            return Ok(dto);

        }
        
        [HttpGet("getPasswordFromSecret")]
        public ActionResult getPasswordFromSecret()
        {
            return Ok(($"Password from secrete: {_config["smtp:Password"]}"));
        }
        [HttpGet("getAccountFromConfig")]
        public ActionResult getAccountFromConfig()
        {
            return Ok(this._smtpOption.Value);
        }
    }
}
