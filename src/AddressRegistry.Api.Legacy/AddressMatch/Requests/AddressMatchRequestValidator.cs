namespace AddressRegistry.Api.Legacy.AddressMatch.Requests
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using FluentValidation;
    using FluentValidation.Validators;

    public class AddressMatchRequestValidator : AbstractValidator<AddressMatchRequest>
    {
        private const string MINIMUM_ONE_NL = "Gelieve minstens een van volgende velden op te geven, '{PropertyNames}'.";
        private const string MINIMUM_ONE_ARGS = "PropertyNames";
        private const string MAXIMUM_ONE_NL = "Gelieve hoogstens een van volgende velden op te geven, '{PropertyNames}'.";
        private const string MAXIMUM_ONE_ARGS = "PropertyNames";
        private const string REQUIRED_NL = "'{PropertyName}' is verplicht.";
        private const string MAX_LENGTH_NL = "'{PropertyName}' mag maximaal {MaxLength} karakters lang zijn. U gaf {TotalLength} karakters op.";
        private const string NUMERIC_NL = "'{PropertyName}' hoort numeriek te zijn.";
        private const string WHEN_PROP1_THEN_PROP2_NL_FORMAT = "Als '{0}' is opgegeven dan is '{{PropertyName}}' verplicht.";
        private const string MISMATCH_PROP1_ARGS = "PropertyName1";
        private const string MISMATCH_PROP2_ARGS = "PropertyName2";

        public AddressMatchRequestValidator()
        {
            When(r => !string.IsNullOrEmpty(r.Postcode),
                () => RuleFor(r => r.Postcode).Must(BeNumeric).WithMessage(NUMERIC_NL).WithErrorCode("11"));

            When(r => !string.IsNullOrEmpty(r.Niscode),
                () => RuleFor(r => r.Niscode).Must(BeNumeric).WithMessage(NUMERIC_NL).WithErrorCode("12"));

            When(r => !string.IsNullOrEmpty(r.KadStraatcode),
                () => RuleFor(r => r.KadStraatcode).Must(BeNumeric).WithMessage(NUMERIC_NL).WithErrorCode("13"));

            When(r => !string.IsNullOrEmpty(r.RrStraatcode),
                () => RuleFor(r => r.RrStraatcode).Must(BeNumeric).WithMessage(NUMERIC_NL).WithErrorCode("14"));

            When(r => !string.IsNullOrEmpty(r.RrStraatcode),
                () => RuleFor(r => r.RrStraatcode).MaximumLength(4).WithMessage(MAX_LENGTH_NL).WithErrorCode("18"));

            When(r => !string.IsNullOrEmpty(r.RrStraatcode),
                () => RuleFor(r => r.Postcode).NotEmpty().WithMessage(string.Format(WHEN_PROP1_THEN_PROP2_NL_FORMAT, "RrStraatCode")).WithErrorCode("19"));

            RuleFor(r => r)
                .Must(HaveMinimumOne(r => r.Gemeentenaam, r => r.Niscode, r => r.Postcode)).WithMessage(MINIMUM_ONE_NL).WithErrorCode("15")
                .Must(HaveMinimumOne(r => r.KadStraatcode, r => r.RrStraatcode, r => r.Straatnaam)).WithMessage(MINIMUM_ONE_NL).WithErrorCode("16")
                .Must(HaveMaximumOne(r => r.KadStraatcode, r => r.RrStraatcode)).WithMessage(MAXIMUM_ONE_NL).WithErrorCode("17")
                .Must(HaveMaximumOne(r => r.Busnummer, r => r.Index)).WithMessage(MAXIMUM_ONE_NL).WithErrorCode("20");

            RuleFor(r => r.Niscode).MaximumLength(5).WithMessage(MAX_LENGTH_NL).WithErrorCode("18");
            RuleFor(r => r.Straatnaam).MaximumLength(80).WithMessage(MAX_LENGTH_NL).WithErrorCode("18");
            RuleFor(r => r.Huisnummer).MaximumLength(40).WithMessage(MAX_LENGTH_NL).WithErrorCode("18");
            RuleFor(r => r.Index).MaximumLength(40).WithMessage(MAX_LENGTH_NL).WithErrorCode("18");
            RuleFor(r => r.Gemeentenaam).MaximumLength(40).WithMessage(MAX_LENGTH_NL).WithErrorCode("18");
        }

        private bool BeNumeric(string input) => int.TryParse(input, out var number);

        private Func<AddressMatchRequest, AddressMatchRequest, PropertyValidatorContext, bool> HaveMinimumOne(params Expression<Func<AddressMatchRequest, string>>[] propertySelectors) =>
            (request, request2, ct) =>
            {
                ct.MessageFormatter.AppendArgument(MINIMUM_ONE_ARGS, string.Join(", ", propertySelectors.Select(selector => (selector.Body as MemberExpression)?.Member?.Name)));
                return propertySelectors.Any(selector => !string.IsNullOrEmpty(selector.Compile().Invoke(request)));
            };

        private Func<AddressMatchRequest, AddressMatchRequest, PropertyValidatorContext, bool> HaveMaximumOne(params Expression<Func<AddressMatchRequest, string>>[] propertySelectors) =>
            (request, request2, ct) =>
            {
                ct.MessageFormatter.AppendArgument(MAXIMUM_ONE_ARGS, string.Join(", ", propertySelectors.Select(selector => (selector.Body as MemberExpression)?.Member?.Name)));
                return propertySelectors.Count(selector => !string.IsNullOrEmpty(selector.Compile().Invoke(request))) <= 1;
            };
    }
}
