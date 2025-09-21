using AutoMapper;
using FluentValidation;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using ShareBook.Domain;
using ShareBook.Domain.Common;
using ShareBook.Domain.DTOs;
using ShareBook.Domain.Enums;
using ShareBook.Domain.Exceptions;
using ShareBook.Helper.Crypto;
using ShareBook.Repository;
using ShareBook.Repository.Repository;
using ShareBook.Repository.UoW;
using ShareBook.Service.Generic;
using ShareBook.Service.Recaptcha;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ShareBook.Service
{
    public class UserService : BaseService<User>, IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IUserEmailService _userEmailService;
        private readonly IRecaptchaService _recaptchaService;
        private readonly IConfiguration _config;

        private readonly IMapper _mapper;


        #region Public

        public UserService(IUserRepository userRepository, IBookRepository bookRepository,
            IUnitOfWork unitOfWork,
            IValidator<User> validator,
            IMapper mapper,
            IUserEmailService userEmailService,
            IRecaptchaService recaptchaService, IConfiguration config) : base(userRepository, unitOfWork, validator)
        {
            _userRepository = userRepository;
            _userEmailService = userEmailService;
            _bookRepository = bookRepository;
            _mapper = mapper;
            _recaptchaService = recaptchaService;
            _config = config;
        }

        public async Task<Result<User>> AuthenticationByEmailAndPasswordAsync(User user)
        {
            var result = Validate(user, x => x.Email, x => x.Password);

            string decryptedPass = user.Password;

            user = await _repository.FindAsync(e => e.Email == user.Email);

            if (user == null)
            {
                result.Messages.Add("Não encontramos esse email no Sharebook. Você já se cadastrou?");
                return result;
            }

            if (user.IsBruteForceLogin())
            {
                result.Messages.Add("Login bloqueado por 30 segundos, para proteger sua conta.");
                return result;
            }

            // persiste última tentativa de login ANTES do SUCESSO ou FALHA pra ter métrica de
            // verificação de brute force.
            user.LastLogin = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user);

            if (!IsValidPassword(user, decryptedPass))
            {
                result.Messages.Add("Email ou senha incorretos");
                return result;
            }

            if (!user.Active)
            {
                result.Messages.Add("Usuário com acesso temporariamente suspenso.");
                return result;
            }

            if (!user.ParentAproved)
            {
                result.Messages.Add($"Usuário menor de idade. Aguardando consentimento dos pais. Foi enviado um email para {user.ParentEmail} em {user.CreationDate?.ToString("dd/MM/yyyy")}.");
                return result;
            }

            result.Value = UserCleanup(user);
            return result;
        }

        public async Task<Result<User>> InsertAsync(RegisterUserDTO userDto)
        {
            User user = _mapper.Map<User>(userDto);
            Result resultRecaptcha = _recaptchaService.SimpleValidationRecaptcha(userDto?.RecaptchaReactive);

            var result = await ValidateAsync(user);
            if (!resultRecaptcha.Success && resultRecaptcha.Messages != null)
                result.Messages.AddRange(resultRecaptcha.Messages);

            if (!result.Success)
                return result;

            // Senha forte não é mais obrigatória.

            if (await _repository.AnyAsync(x => x.Email == user.Email))
                throw new ShareBookException("Usuário já possui email cadastrado.");

            // LGPD - CONSENTIMENTO DOS PAIS.
            if (userDto.Age < 12)
                await ParentAprovalStartFlowAsync(userDto, user);

            user.Email = user.Email.ToLowerInvariant();
            if (result.Success)
            {
                user = GetUserEncryptedPass(user);
                result.Value = UserCleanup(await _repository.InsertAsync(user));
            }
            return result;
        }

        private async Task ParentAprovalStartFlowAsync(RegisterUserDTO userDto, User user)
        {
            user.ParentAproved = false;
            user.ParentHashCodeAproval = Guid.NewGuid().ToString();

            if (string.IsNullOrEmpty(userDto.ParentEmail))
                throw new ShareBookException("Menor de idade. Obrigatório informar o email do pai ou responsável.");

            await _userEmailService.SendEmailRequestParentAprovalAsync(userDto, user);
        }

        public override async Task<Result<User>> UpdateAsync(User user)
        {
            user.Id = new Guid(Thread.CurrentPrincipal?.Identity?.Name);
            Result<User> result = Validate(user, x =>
               x.Email,
                x => x.Linkedin,
                x => x.Name,
                x => x.Phone,
                x => x.Id);

            if (!result.Success) return result;

            var userAux = await _repository.FindAsync(new IncludeList<User>(x => x.Address), user.Id);

            if (userAux == null) result.Messages.Add("Usuário não existe.");

            if (await _repository.AnyAsync(u => u.Email == user.Email && u.Id != user.Id))
                result.Messages.Add("Email já existe.");

            if (result.Success)
            {
                userAux.Change(user.Email, user.Name, user.Linkedin, user.Instagram, user.Phone, user.AllowSendingEmail);
                userAux.ChangeAddress(user.Address);

                result.Value = UserCleanup(await _repository.UpdateAsync(userAux));
            }

            return result;
        }

        public override async Task<User> FindAsync(object keyValue)
        {
            var includes = new IncludeList<User>(x => x.Address);
            return await _repository.FindAsync(includes, keyValue);
        }

        public async Task<Result<User>> ValidOldPasswordAndChangeUserPasswordAsync(User user, string newPassword)
        {
            var resultUserAuth = this.AuthenticationByIdAndPassword(user);

            if (resultUserAuth.Success)
                await ChangeUserPasswordAsync(resultUserAuth.Value, newPassword);

            return resultUserAuth;
        }

        public async Task<Result<User>> ChangeUserPasswordAsync(User user, string newPassword)
        {
            var result = await ValidateAsync(user);

            // Senha forte não é mais obrigatória. Apenas validação de tamanho.
            if (newPassword.Length < 6 || newPassword.Length > 32)
                throw new ShareBookException("A senha deve ter entre 6 e 32 letras.");

            user.ChangePassword(newPassword);
            user = GetUserEncryptedPass(user);
            user = await _userRepository.UpdatePasswordAsync(user);
            result.Value = UserCleanup(user);

            return result;
        }

        public async Task<Result> GenerateHashCodePasswordAndSendEmailToUserAsync(string email)
        {
            var result = new Result();
            var user = await _repository.FindAsync(e => e.Email == email);

            if (user == null)
            {
                result.Messages.Add("E-mail não encontrado.");
                return result;
            }

            user.GenerateHashCodePassword();
            await _repository.UpdateAsync(user);
            await _userEmailService.SendEmailForgotMyPasswordToUserAsync(user);
            result.SuccessMessage = "E-mail enviado com as instruções para recuperação da senha.";
            return result;
        }

        public async Task<Result> ConfirmHashCodePasswordAsync(string hashCodePassword)
        {
            var result = new Result();

            var userConfirmedHashCodePassword = await _repository.FindAsync(e => e.HashCodePassword.Equals(hashCodePassword));

            if (userConfirmedHashCodePassword == null)
                result.Messages.Add("Hash code não encontrado.");
            else if (result.Success && !userConfirmedHashCodePassword.HashCodePasswordIsValid(hashCodePassword))
                result.Messages.Add("Chave errada ou expirada. Por favor gere outra chave");
            else
                result.Value = UserCleanup(userConfirmedHashCodePassword);

            return result;
        }

        public IList<User> GetFacilitators(Guid userIdDonator)
        {
            var database = _config["DatabaseProvider"].ToLower();

            string query;

            if (database == "sqlserver")
            {
                query = @"
                    SELECT TOP 100
                        u.Id,
                        CONCAT(u.Name, ' (', COUNT(b.Id), ')') as Name
                    FROM Users u
                    LEFT JOIN Books b
                        ON b.UserIdFacilitator = u.Id
                       AND b.UserId = {0}
                    WHERE u.Profile = 0
                    GROUP BY u.Id, u.Name
                    ORDER BY COUNT(b.Id) DESC, u.Name";
            }
            else // Postgres ou SQLite
            {
                query = @"
                    SELECT
                        u.""Id"",
                        (u.""Name"" || ' (' || COUNT(b.""Id"") || ')') as ""Name""
                    FROM ""Users"" u
                    LEFT JOIN ""Books"" b
                        ON b.""UserIdFacilitator"" = u.""Id""
                       AND b.""UserId"" = {0}
                    WHERE u.""Profile"" = 0
                    GROUP BY u.""Id"", u.""Name""
                    ORDER BY COUNT(b.""Id"") DESC, u.""Name""
                    LIMIT 100";
            }

            var parameters = new object[] { userIdDonator };

            return _repository.FromSql(query, parameters)
                .Select(x => new User
                {
                    Id = x.Id,
                    Name = x.Name
                }).ToList();
        }


        public IList<User> GetAdmins()
        {
            return _userRepository.Get()
                .Where(u => u.Profile == Domain.Enums.Profile.Administrator)
                .ToList();
        }

        #endregion Public

        #region Private

        private Result<User> AuthenticationByIdAndPassword(User user)
        {
            // TODO: Migrate to async
            var result = Validate(user, x => x.Id, x => x.Password);

            string decryptedPass = user.Password;

            user = _repository.Get()
                .Where(e => e.Id == user.Id)
                .FirstOrDefault();

            if (user == null || !IsValidPassword(user, decryptedPass))
            {
                result.Messages.Add("Senha incorreta");
                return result;
            }

            result.Value = UserCleanup(user);
            return result;
        }

        public bool IsValidPassword(User user, string decryptedPass)
        {
            return user.Password == Hash.Create(decryptedPass, user.PasswordSalt);
        }

        private User GetUserEncryptedPass(User user)
        {
            user.PasswordSalt = Salt.Create();
            user.Password = Hash.Create(user.Password, user.PasswordSalt);
            return user;
        }

        private User UserCleanup(User user)
        {
            user.Password = string.Empty;
            user.PasswordSalt = string.Empty;
            return user;
        }

        public async Task<IList<User>> GetBySolicitedBookCategoryAsync(Guid bookCategoryId) =>
            await _userRepository.Get().Where(u => u.AllowSendingEmail && u.BookUsers.Any(bu => bu.Book.CategoryId == bookCategoryId)).ToListAsync();

        public async Task<UserStatsDTO> GetStatsAsync(Guid? userId)
        {
            var user = await _userRepository.FindAsync(userId);
            var books = await _bookRepository.Get().Where(b => b.UserId == userId).ToListAsync();

            if (user == null) throw new ShareBookException(ShareBookException.Error.NotFound, "Usuário não encontrado.");

            var stats = new UserStatsDTO
            {
                CreationDate = user.CreationDate,
                TotalLate = books.Count(b => b.ChooseDate < DateTime.Today && b.Status == BookStatus.AwaitingDonorDecision),
                TotalOk = books.Count(b => b.Status == BookStatus.WaitingSend || b.Status == BookStatus.Sent || b.Status == BookStatus.Received),
                TotalCanceled = books.Count(b => b.Status == BookStatus.Canceled),
                TotalWaitingApproval = books.Count(b => b.Status == BookStatus.WaitingApproval),
                TotalAvailable = books.Count(b => b.Status == BookStatus.Available),
            };
            return stats;
        }

        public async Task ParentAprovalAsync(string parentHashCodeAproval)
        {
            var user = await _repository.Get()
                .Where(u => u.ParentHashCodeAproval == parentHashCodeAproval)
                .FirstOrDefaultAsync();

            if (user == null)
                throw new ShareBookException(ShareBookException.Error.NotFound, "Nenhum usuário encontrado.");

            if (user.ParentAproved)
                throw new ShareBookException(ShareBookException.Error.NotFound, "O acesso já foi liberado anteriormente. Tudo certo.");

            user.ParentAproved = true;
            await _userRepository.UpdateAsync(user);

            await _userEmailService.SendEmailParentAprovedNotifyUserAsync(user);
        }


        #endregion Private
    }
}