namespace AddressRegistry.Tests.BackOffice.Validators
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using AddressRegistry.Api.BackOffice.Abstractions.Requests;
    using AddressRegistry.Api.BackOffice.Validators;
    using FluentAssertions;
    using FluentValidation.TestHelper;
    using Infrastructure;
    using StreetName;
    using Xunit;

    public class CorrectAddressBoxNumbersRequestValidatorTests
    {
        private readonly CorrectAddressBoxNumbersRequestValidator _sut;
        private readonly TestBackOfficeContext _backOfficeContext;

        public CorrectAddressBoxNumbersRequestValidatorTests()
        {
            _backOfficeContext = new FakeBackOfficeContextFactory().CreateDbContext();
            _sut = new CorrectAddressBoxNumbersRequestValidator(_backOfficeContext, FakeBoxNumberValidator.InstanceInterneBijwerker);
        }

        public static IEnumerable<object[]> EmptyBusnummers()
        {
            yield return [null];
            yield return [new List<CorrectAddressBoxNumbersRequestItem>()];
        }

        [Theory]
        [MemberData(nameof(EmptyBusnummers))]
        public void WhenBoxNumbersIsEmpty_ThenReturnsExpectedFailure(List<CorrectAddressBoxNumbersRequestItem> items)
        {
            var result = _sut.TestValidate(new CorrectAddressBoxNumbersRequest
            {
                Busnummers = items
            });

            result.Errors.Count.Should().Be(1);
            result.ShouldHaveValidationErrorFor(nameof(CorrectAddressBoxNumbersRequest.Busnummers))
                .WithErrorCode("BusnummersLijstLeeg")
                .WithErrorMessage("De lijst van busnummers mag niet leeg zijn.");
        }

        [Fact]
        public async Task WhenAddressIdsAreNotUnique_ThenReturnsExpectedFailure()
        {
            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(1), new StreetNamePersistentLocalId(1));

            var result = _sut.TestValidate(new CorrectAddressBoxNumbersRequest
            {
                Busnummers = [
                    new CorrectAddressBoxNumbersRequestItem
                    {
                        AdresId = "http://adres/1",
                        Busnummer = "A"
                    },
                    new CorrectAddressBoxNumbersRequestItem
                    {
                        AdresId = "http://adres/1",
                        Busnummer = "B"
                    }
                ]
            });

            result.Errors.Should().HaveCount(2);
            result.ShouldHaveValidationErrorFor("Busnummers[0]")
                .WithErrorCode("AdresIdReedsInLijstBusnummers")
                .WithErrorMessage("Het adres 'http://adres/1' zit reeds in lijst van busnummers.");
            result.ShouldHaveValidationErrorFor("Busnummers[1]")
                .WithErrorCode("AdresIdReedsInLijstBusnummers")
                .WithErrorMessage("Het adres 'http://adres/1' zit reeds in lijst van busnummers.");
        }

        [Fact]
        public async Task WhenBoxNumbersAreNotUnique_ThenReturnsExpectedFailure()
        {
            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(1), new StreetNamePersistentLocalId(1));
            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(2), new StreetNamePersistentLocalId(1));

            var result = _sut.TestValidate(new CorrectAddressBoxNumbersRequest
            {
                Busnummers = [
                    new CorrectAddressBoxNumbersRequestItem
                    {
                        AdresId = "http://adres/1",
                        Busnummer = "A"
                    },
                    new CorrectAddressBoxNumbersRequestItem
                    {
                        AdresId = "http://adres/2",
                        Busnummer = "A"
                    }
                ]
            });

            result.Errors.Should().HaveCount(2);
            result.ShouldHaveValidationErrorFor("Busnummers[0]")
                .WithErrorCode("BusnummerReedsInLijstBusnummers")
                .WithErrorMessage("Het busnummer 'A' zit reeds in lijst van busnummers.");
            result.ShouldHaveValidationErrorFor("Busnummers[1]")
                .WithErrorCode("BusnummerReedsInLijstBusnummers")
                .WithErrorMessage("Het busnummer 'A' zit reeds in lijst van busnummers.");
        }

        [Fact]
        public async Task WhenInvalidAddressIds_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new CorrectAddressBoxNumbersRequest
            {
                Busnummers = [
                    new CorrectAddressBoxNumbersRequestItem
                    {
                        AdresId = "1",
                        Busnummer = "A"
                    }
                ]
            });

            result.Errors.Should().HaveCount(1);
            result.ShouldHaveValidationErrorFor("Busnummers[0]")
                .WithErrorCode("AdresIdIsOnbestaand")
                .WithErrorMessage("Onbestaand adres '1'.");
        }

        [Fact]
        public async Task WhenInvalidBoxNumber_ThenReturnsExpectedFailure()
        {
            await _backOfficeContext.AddAddressPersistentIdStreetNamePersistentId(new AddressPersistentLocalId(1), new StreetNamePersistentLocalId(1));

            var result = _sut.TestValidate(new CorrectAddressBoxNumbersRequest
            {
                Busnummers = [
                    new CorrectAddressBoxNumbersRequestItem
                    {
                        AdresId = "http://adres/1",
                        Busnummer = "_"
                    }
                ]
            });

            result.Errors.Should().HaveCount(1);
            result.ShouldHaveValidationErrorFor("Busnummers[0]")
                .WithErrorCode("AdresOngeldigBusnummerformaat")
                .WithErrorMessage("Ongeldig busnummerformaat: _.");
        }

        [Fact]
        public async Task WhenNonExistingAddresses_ThenReturnsExpectedFailure()
        {
            var result = _sut.TestValidate(new CorrectAddressBoxNumbersRequest
            {
                Busnummers = [
                    new CorrectAddressBoxNumbersRequestItem
                    {
                        AdresId = "http://adres/1",
                        Busnummer = "A"
                    },
                    new CorrectAddressBoxNumbersRequestItem
                    {
                        AdresId = "http://adres/2",
                        Busnummer = "B"
                    }
                ]
            });

            result.Errors.Should().HaveCount(2);
            result.ShouldHaveValidationErrorFor("Busnummers[0]")
                .WithErrorCode("AdresIdIsOnbestaand")
                .WithErrorMessage("Onbestaand adres 'http://adres/1'.");
            result.ShouldHaveValidationErrorFor("Busnummers[1]")
                .WithErrorCode("AdresIdIsOnbestaand")
                .WithErrorMessage("Onbestaand adres 'http://adres/2'.");
        }
    }
}
