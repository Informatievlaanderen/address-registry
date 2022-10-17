namespace AddressRegistry.Api.Legacy.AddressMatch.Requests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using FluentValidation;

    public class AddressMatchRequestValidator : AbstractValidator<AddressMatchRequest>
    {
        private const string MinimumOne = "Gelieve minstens één van de volgende velden op te geven: {PropertyNames}.";
        private const string MinimumOneArg = "PropertyNames";
        private const string MaximumOne = "Gelieve hoogstens één van de volgende velden op te geven: {PropertyNames}.";
        private const string MaximumOneArg = "PropertyNames";
        private const string MaxLength = "'{PropertyName}' mag maximaal {MaxLength} karakters lang zijn. U gaf {TotalLength} karakters op.";
        private const string Numeric = "'{PropertyName}' hoort numeriek te zijn.";
        private const string WhenProp1ThenProp2 = "Als '{0}' is opgegeven dan is '{{PropertyName}}' verplicht.";

        public AddressMatchRequestValidator()
        {
            When(r => !string.IsNullOrEmpty(r.Postcode), () =>
                RuleFor(r => r.Postcode).Must(BeNumeric).WithMessage(Numeric).WithErrorCode("11"));

            When(r => !string.IsNullOrEmpty(r.Niscode), () =>
                RuleFor(r => r.Niscode).Must(BeNumeric).WithMessage(Numeric).WithErrorCode("12"));

            When(r => !string.IsNullOrEmpty(r.KadStraatcode), () =>
                RuleFor(r => r.KadStraatcode).Must(BeNumeric).WithMessage(Numeric).WithErrorCode("13"));

            When(r => !string.IsNullOrEmpty(r.RrStraatcode), () =>
                RuleFor(r => r.RrStraatcode).Must(BeNumeric).WithMessage(Numeric).WithErrorCode("14"));

            When(r => !string.IsNullOrEmpty(r.RrStraatcode), () =>
                RuleFor(r => r.RrStraatcode).MaximumLength(4).WithMessage(MaxLength).WithErrorCode("18"));

            When(r => !string.IsNullOrEmpty(r.RrStraatcode), () =>
                RuleFor(r => r.Postcode).NotEmpty().WithMessage(string.Format(WhenProp1ThenProp2, "RrStraatCode")).WithErrorCode("19"));

            RuleFor(r => r)
                .Must(HaveMinimumOne(r => r.Gemeentenaam, r => r.Niscode, r => r.Postcode)).WithMessage(MinimumOne).WithErrorCode("15")
                .Must(HaveMinimumOne(r => r.KadStraatcode, r => r.RrStraatcode, r => r.Straatnaam)).WithMessage(MinimumOne).WithErrorCode("16")
                .Must(HaveMaximumOne(r => r.KadStraatcode, r => r.RrStraatcode)).WithMessage(MaximumOne).WithErrorCode("17")
                .Must(HaveMaximumOne(r => r.Busnummer, r => r.Index)).WithMessage(MaximumOne).WithErrorCode("20");

            RuleFor(r => r.Niscode).MaximumLength(5).WithMessage(MaxLength).WithErrorCode("18");
            RuleFor(r => r.Postcode).MaximumLength(4).WithMessage(MaxLength).WithErrorCode("18");
            RuleFor(r => r.Straatnaam).MaximumLength(80).WithMessage(MaxLength).WithErrorCode("18");
            RuleFor(r => r.Huisnummer).MaximumLength(40).WithMessage(MaxLength).WithErrorCode("18");
            RuleFor(r => r.Busnummer).MaximumLength(40).WithMessage(MaxLength).WithErrorCode("18");
            RuleFor(r => r.Index).MaximumLength(40).WithMessage(MaxLength).WithErrorCode("18");
            RuleFor(r => r.Gemeentenaam).MaximumLength(40).WithMessage(MaxLength).WithErrorCode("18");
        }

        private static bool BeNumeric(string input) => int.TryParse(input, out var _);

        private static Func<AddressMatchRequest, AddressMatchRequest, ValidationContext<AddressMatchRequest>, bool> HaveMinimumOne(params Expression<Func<AddressMatchRequest, string>>[] propertySelectors) =>
            (request, request2, ct) =>
            {
                ct.MessageFormatter.AppendArgument(MinimumOneArg, string.Join(", ", propertySelectors.Select(selector => $"'{(selector.Body as MemberExpression)?.Member?.Name}'")));
                return propertySelectors.Any(selector => !string.IsNullOrEmpty(selector.Compile().Invoke(request)));
            };

        private Func<AddressMatchRequest, AddressMatchRequest, ValidationContext<AddressMatchRequest>, bool> HaveMaximumOne(params Expression<Func<AddressMatchRequest, string>>[] propertySelectors) =>
            (request, request2, ct) =>
            {
                ct.MessageFormatter.AppendArgument(MaximumOneArg, string.Join(", ", propertySelectors.Select(selector => $"'{(selector.Body as MemberExpression)?.Member?.Name}'")));
                return propertySelectors.Count(selector => !string.IsNullOrEmpty(selector.Compile().Invoke(request))) <= 1;
            };
    }
}
