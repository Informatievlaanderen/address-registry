# AddressMatch

This guide is intended to better understand the flow of the `AdresMatch` component.

## Usage

The starting point is the creation of an `AddressMatchMatchingAlgorithm`.

```csharp
var warningLogger = new ValidationMessageWarningLogger();
var maxNumberOfResults = 10;
var addressMatch = new AddressMatchMatchingAlgorithm<AdresMatchScorableItem>(
    kadRrService,
    new ManualAddressMatchConfig(responseOptions.Value.SimilarityThreshold, responseOptions.Value.MaxStreetNamesThreshold),
    latestQueries,
    new GemeenteMapper(responseOptions.Value),
    new StreetNameMapper(responseOptions.Value, latestQueries),
    new AdresMapper(responseOptions.Value, latestQueries),
    maxNumberOfResults,
    warningLogger);
```

To get results out the algorithm, `Process` is called:

```csharp
public AddressMatchQueryComponents Map(AddressMatchRequest request)
    => new AddressMatchQueryComponents
    {
        MunicipalityName = request.Gemeentenaam,
        HouseNumber = request.Huisnummer,
        BoxNumber = request.Busnummer,
        Index = request.Index,
        KadStreetNameCode = request.KadStraatcode,
        NisCode = request.Niscode,
        PostalCode = request.Postcode,
        RrStreetCode = request.RrStraatcode,
        StreetName = request.Straatnaam
    };

var result = addressMatch.Process(new AddressMatchBuilder(Map(addressMatchRequest))).Take(maxNumberOfResults);
```

## Internal Workings

### AddressMatchBuilder

This gets passed in to `Process` and is a container to be passed around to all other matchers.

This stores:

* The query parameters.
* Various intermediary results.

### AddressMatchMatchingAlgorithm

This simply combines 4 matchers:

```csharp
  new RrAddressMatcher<TResult>(kadRrService, addressMapper, maxNumberOfResults, warnings),
  new MunicipalityMatcher<TResult>(latestQueries, config, municipalityMapper, warnings),
  new StreetNameMatcher<TResult>(latestQueries, kadRrService, config, streetNameMapper, warnings),
  new AddressMatcher<TResult>(latestQueries, addressMapper, warnings)
```

The `Process` simply goes to the base class. This base class (`MatchingAlgorithm`) iterates over every step, trying to find a match.

If no match has found, it returns a step to the previous matcher to get the result from there.

In short the logic is:

> Do a match!
> Should we continue?
> Do another match!
> Should we continue?
> Do another match!
> Should we continue?
> No, we're done.
> Was the last match succesfull?
> Yes! (If not, then we trace back the previous matcher till there was a match)
> Build the result for this match!

### Matchers

All matchers derive from `MatcherBase` which sole job is to provide some abstract methods, template pattern style.

It enforces people to first run `DoMatch` before they can call `Proceed`.

The order matchers are executed is:

* `DoMatchInternal`
* Pass the result to `ShouldProceed` and `IsValidMatch` and store it.

An `AddressMatchBuilder` is passed from matcher to matcher.

### RrAddressMatcher

The first matcher checks on _RijksRegister_ addresses.

It will only match if a housenumber and _rijksregister_ street code has been passed.

If those are passed, it will query the _rijksregister_ addresses and find an **exact** match on:

* _Rijksregister_ Housenumber
* _Rijksregister_ Index
* Streetcode
* Postalcode

It will then map the result with the building registry addresses and store it in the `AddressMatchBuilder` container.

If it found addresses, it will stop the matching flow and return to the user.

### MunicipalityMatcher

The second matcher evaluates municipalities.

It starts by getting a list of all municipalities.

* Then it takes the default name of each municipality (NL/FR/DE/EN) and checks for an **exact** case insensitive match with the `MunicipalityName` query.
* Then it takes the NIS Code of each municipality and checks for an **exact** match with the `NisCode` query.
* Then it stores the combined results in the `AddressMatchBuilder` container. It wraps the result in another container to add future streetnames to.

If there is more than 1 result, it means the `MunicipalityName` and `NisCode` parameters pointed to a different municipality and a warning is added.

* If a `PostalCode` query is passed, it looks up the municipalities by **exact** match on postal code and adds them to the `AddressMatchBuilder` as well.

If there are results, but they don't match the NIS codes found earlier, it adds a warning.

If there are results at this point, it means there is an exact match and it stops the matching.

If the `MunicipalityName` query was empty, it stops the match at this point too.

If there are no results yet, and `MunicipalityName` was passed in, it looks for partial matches:

* It checks for an **exact** case and diacritics insensitive match on the `MunicipalityName` query and the `PostalName` (from bPost).
* If it finds any, it adds them to the `AddressMatchBuilder` as well.

If there are result at this point, it counts as a perfect match too and the matching stops.

If there are no results yet, we move on to fuzzy matching:

* Given the default municipality name it does a **fuzzy** match with the `MunicipalityName` query.
* It adds the results to the `AddressMatchBuilder`.

After the entire match flow, it will proceed onto the next matcher if it has found any result. Otherwise the matching stops and returns to the user.

### StreetNameMatcher

This is the first matcher to add scoring to the results.

If the `KadStreetNameCode` query is given it will:

* For each municipality it has previously found get all the streets with an **exact** match on `KadStreetNameCode` and `NisCode`.
* Add these streets to the `AddressMatchBuilder` container per municipality.
* If the `StreetName` query was given, and none of the streets that are found match the name, it adds a warning.

If the `RrStreetCode` and `PostalCode` queries is given it will:

* Find the _rijksregister_ street by **exact** match on `RrStreetCode` and `PostalCode`.
* Add the streetname to the `AddressMatchBuilder` container based on the `PostalCode` municipality.
* If the `StreetName` query was given, and none of the streets that are found match the name, it adds a warning.

If the `StreetName` query is given it will:

* For every municipality that was previously found, fetch all streetnames.
* Look for an **exact** case insensitive match on the streetname.
* If it did not find a match, look for an **exact** case insensitive match on the streetname with common abreviations replaced.
* If it did not find a match, look for a **fuzzy** match on the streetname.
* If it did not find a match, look for a **fuzzy** match on the streetname with common abreviations replaced.
* If it did not find a match, look for a streetnames which partially contain the `StreetName` query, case insensitive.
* If it did not find a match, look for a streetnames which are part of the `StreetName` query, case insensitive.

If there are still no results at this point, it will add a warning.

If there are too many results, it will not return them and add a warning.

After the entire match flow, it will proceed onto the next matcher if it has found any result. Otherwise the matching stops and returns to the user.

For all the results that have been found calculate the score.

### AddressMatcher

This is another matcher which adds scoring to the results.

If `BoxNumber` query is given:

* It takes the given `HouseNumber` and `BoxNumber` queries and adds it to the results.
* This will result in an **exact** match on `HouseNumber` and `BoxNumber` only.

If `BoxNumber` query is not given:

* Take `StreetName`, `HouseNumber` and `Index` queries and `Sanitize` them.

With the results of this, it will fetch the BuildingRegistry stored addresses and return them.

This is the last matcher, it's now or nothing :)

### Score calculation

To calculate the score for each item:

* It builds a list of strings representing each result.
* For each result it then fuzzy matches it against each representation and takes the average value.

### Sanitize Logic

This is a monster of a method, the amount of business logic in this is huge.

It begins with some cleanup:

* It starts by left padding the given `HouseNumber` query with `0`s to get to a length of 8.
* Then it takes the `Index` query, removes the preceding zeroes and checks if it is a number. If it is a number, it will left-pad it with `0`'s till 4. Otherwise it will right-pad it with `0`'s till 4.

Then it checks whether the `StreetName` query is supplied and tries to parse things out of it:

* First it removes all commas, spaces and the streetname itself.
* Then it checks if the street starts with a number and stores it as the new housenumber.
* Then it checks if the street starts with a number + a character, but not `e` and stores it as the new housenumber and _rijksregister_ index.
* Then it checks if the street ends with a number and stores it as the new housenumber.
* Then it checks if the street ends with a number + a character, but not `e` and stores it as the new housenumber and _rijksregister_ index.

After this cleanup, we start checking the new housenumber for results which didn't have a _rijksregister_ index.

* If it is completely numeric, do an **exact** match on the number, without preceding zeroes. e.g.: `42`.
* If it is all numbers, followed by spaces and a single character, do an **exact** match on the number (without preceding zeroes) + the character. e.g.: `42B` (_bisnummer_).
* If it is not exactly 1 character and ends with `bis`, do an **exact** match on the number (without preceding zeroes) + `BIS`. e.g.: `42BIS`.
* If any of these matched, the parsing of streetname stops.

Otherwise we move on to the case which has a _rijksregister_ index with a text part, optionally appended with `0`'s.

As input we have a list with special words:

```csharp
private static readonly HashSet<string> IndexWordsBoxNumber = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "BUS", "bte", "BT", "bu", "bs" };
private static readonly HashSet<string> IndexWordsAppartementNumber = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "AP", "APP", "apt" };
private static readonly HashSet<string> IndexWordsFloorNumber = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "et", "eta", "éta", "VER", "VDP", "VD", "Vr", "Vrd", "V", "Etg", "ét", "et", "ev", "eme", "ème", "STE", "de", "dev", "e", "E", "é", "links", "rechts" };
private static readonly HashSet<string> IndexWordsFloor = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase) { "RC", "RCAR", "GV", "Rch", "RDC", "HAL", "rez", "RCH", "GV", "GVL", "GLV", "GEL", "GL" };
private static readonly HashSet<string> IndexWordsWithoutBisNumber = new HashSet<string>(IndexWordsBoxNumber.Concat(IndexWordsAppartementNumber).Concat(IndexWordsFloorNumber).Concat(IndexWordsFloor), StringComparer.InvariantCultureIgnoreCase);
```

* If the index contains none of these words, do an **exact** match on the housenumber (without preceding zeroes) + _rijksregister_ index. e.g.: `42WTF`.
* Otherwise do an **exact** match on housenumber, without preceding zeroes. e.g.: `42`.
* The parsing of streetname stops if this case was valid.

Now we move on to the case which has a _rijksregister_ index with a text part, and appended with a numeric part.

As input we again have the same special words lists.

* If the index contains none of these words, do an **exact** match on the housenumber (without preceding zeroes) + _rijksregister_ index and on the boxnumber (numeric part of the _rijksregister_ without preceding zeroes). e.g.: `42WTF` + box `5`.
* If the index contains any of the `BoxNumber` words, do an **exact** match on the housenumber (without preceding zeroes) and on the boxnumber (numeric part of the _rijksregister_ without preceding zeroes). e.g.: `42` + box `5`.
* Otherwise do an **exact** match on the housenumber (without preceding zeroes) only. e.g.: `42`.
* The parsing of streetname stops if this case was valid.

Next case is some special CRAB case with a _rijksregister_ index which starts with a number and does not contain a _part 3_.

* If this is the case, do an **exact** match on the housenumber (without preceding zeroes) and on the boxnumber (numeric part of the _rijksregister_ without preceding zeroes). e.g.: `42` + box `5`.
* The parsing of streetname stops if this case was valid.

Next case is another special CRAB case with a _rijksregister_ index which starts with a number, a numeric _part 3_ and no _part 4_.

* If this is the case, do an **exact** match on the housenumber (without preceding zeroes). e.g.: `42`.
* The parsing of streetname stops if this case was valid.

We're getting into the more special things. Next one is _rijksregister_ index which starts with a number, a non-numeric _part 3_ and a numeric _part 4_.

As input we again have the same special words lists.

* If the index contains none of these words, do an **exact** match on the housenumber (without preceding zeroes) + non-numeric part of _rijksregister_ index and on the boxnumber (numeric part of the _rijksregister_ without preceding zeroes). e.g.: `42WTF` + box `5`.
* If the index contains any of the `FloorNumber` words, do an **exact** match on the housenumber (without preceding zeroes) only. e.g.: `42`.
* If the index contains any of the `Appartement` words, do an **exact** match on the housenumber (without preceding zeroes) + _rijksregister_ index. e.g.: `42A`.
* If the index contains any of the `BoxNumber` words, do an **exact** match on the housenumber (without preceding zeroes) + _rijksregister_ index and on the boxnumber (numeric part of the _rijksregister_ without preceding zeroes). e.g.: `42A` + box `5`.
* If any of these matched, the parsing of streetname stops.

The next case is a _rijksregister_ index which starts with a number, has a non numeric _part 3_ and no _part 4_.

* If the index contains any of the `FloorNumber` words, do an **exact** match on the housenumber (without preceding zeroes) only. e.g.: `42`.
* Otherwise do an **exact** match on the housenumber (without preceding zeroes) + `_` +  _rijksregister_ index and on the boxnumber (numeric part of the _rijksregister_ without preceding zeroes). e.g.: `42A` + box `5`.
* The parsing of streetname stops if this case was valid.

At this point we have parsed everything we can out of the streetname for a single address. The only thing left is parsing possible housenumber ranges.

Valid range notations for values passed in the `HouseNumber` query:

* `000-000`
* `XXX-XXX`
* `000/000`
* `XXX/XXX`
* `000+000`
* `XXX+XXX`
* `0 0`

Valid range notations for values passed in the `StreetName` query:

* `000-000`

After parsing the possible ranges:

* If the second part is all characters do an **exact** match on the first part as the housenumber and an **exact** match on the second part as boxnumber. e.g.: `42-B`
* If the second part is characters and numbers, ignore it and do an **exact** match on the first part as the housenumber. e.g.: `42-R2D2`
* If the first and second part are numeric, but the first part is bigger than the second part, do an **exact** match on the first part as the housenumber and an **exact** match on the second part as boxnumber. e.g.: `42-7`
* For all other cases do an **exact** match on the first part as the housenumber and do an **exact** match on the second part as another housenumber. e.g.: `42-44`.
* If any of these matched, the parsing of streetname stops.

Finally if all options are done, it does a final check having a numeric housenumber and no  _rijksregister_ index.

* If this is the case, it does an exact match on the housenumber.

At this point we have run out of options and we can't find anything.

## Notes

* What about the caching that is being done? Where is the cache updated?
