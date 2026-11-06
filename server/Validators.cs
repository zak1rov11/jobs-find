using FluentValidation;

namespace JobsApi;

public class JobRequestValidator : AbstractValidator<JobRequest>
{
    public JobRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().WithMessage("Укажите должность")
            .MaximumLength(100).WithMessage("Должность не длиннее 100 символов");
        RuleFor(x => x.Company).NotEmpty().WithMessage("Укажите компанию")
            .MaximumLength(100).WithMessage("Компания не длиннее 100 символов");
        RuleFor(x => x.SalaryFrom).InclusiveBetween(0, 1_000_000_000)
            .WithMessage("Зарплата «от» — от 0 до 1 000 000 000");
        RuleFor(x => x.SalaryTo).InclusiveBetween(0, 1_000_000_000)
            .WithMessage("Зарплата «до» — от 0 до 1 000 000 000");
        // ОСОБОЕ ПРАВИЛО: «зарплата от» не больше «зарплата до»
        RuleFor(x => x.SalaryTo).GreaterThanOrEqualTo(x => x.SalaryFrom)
            .WithMessage("Зарплата «до» не может быть меньше зарплаты «от»");
        RuleFor(x => x.SphereId).GreaterThan(0).WithMessage("Выберите сферу");
    }
}

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.UserName).NotEmpty().WithMessage("Укажите логин")
            .Length(3, 30).WithMessage("Логин от 3 до 30 символов");
        RuleFor(x => x.Password).NotEmpty().WithMessage("Укажите пароль")
            .MinimumLength(6).WithMessage("Пароль не короче 6 символов");
    }
}
