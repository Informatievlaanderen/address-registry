# 3. Accept Lambert 2008 GML input in the BackOffice API

Date: 2026-07-30

## Status

Accepted

## Context

The BackOffice API only accepts positions as GML-3 with `srsName` Lambert 72 (EPSG 31370),
because `Be.Vlaanderen.Basisregisters.GrAr.Edit.Validators.GmlPointValidator` matches the
srsName against the hardcoded `GmlConstants.SrsNameAttribute` (31370).

We want callers to be able to send Lambert 2008 (EPSG 3812) as well, without changing what
the event store persists. The event store migration to Lambert 2008 will happen later, together
with the lambda changes, so the API must be able to flip its target reference system by configuration.

## Decision

The BackOffice API accepts both Lambert 72 and Lambert 2008 GML and **normalizes the position to
the reference system of the event store before the SQS message is created**. Everything downstream
(SQS message, lambda, aggregate, projections) keeps seeing a single reference system.

Which reference system that is, is decided by the `UseLambert2008EventStore` feature toggle:

| `FeatureToggles:UseLambert2008EventStore` | event store SRID | Lambert 72 input | Lambert 2008 input |
|---|---|---|---|
| `false` (default, current) | 31370 | kept as is | converted to 31370 |
| `true` (after event store migration) | 3812 | converted to 3812 | kept as is |

"Kept as is" is literal: the incoming GML string is passed through untouched when it is already in
the event store's reference system, so today's behaviour is byte-for-byte unchanged while the toggle is off.

Conversion uses `Be.Vlaanderen.Basisregisters.GrAr.CrsTransform.LambertTransformation`
(`TransformFromLambert72To08` / `TransformFromLambert08To72`) and is re-serialized with
`GeometryExtensions.ConvertToGml(false)`, which formats point coordinates as `F2` — the same precision
positions already use. A 72 → 08 → 72 round trip is exact at that precision.

### srsName scheme

Strictly the OGC def URI is `http://www.opengis.net/def/crs/EPSG/0/…`, which is what
`SystemReferenceId.SrsNameLambert72` / `SrsNameLambert2008` hold, and what every GML producer in this
repo emits (`AddressFeedProjections` and the Oslo `AddressMapper` all call `ConvertToGml(false)`).
The `https` form is nevertheless in wide use — it is what the swagger examples and
`GrAr.Edit`'s `GmlConstants.SrsNameAttribute` use.

So:
- **Input:** both schemes are accepted, for both EPSG codes, matched case-insensitively.
- **Output:** the converted path emits the `http` form, matching `SystemReferenceId`.
  The pass-through path emits whatever the caller sent, since it does not touch the string at all.

The scheme is cosmetic to consumers: `GMLReader` derives the SRID from the trailing EPSG code and
reads both schemes to identical EWKB.

### Why the normalizer is load-bearing

`GMLReader` honours `srsName` — reading a Lambert 2008 GML with a Lambert 72 reader yields a geometry
with SRID 3812, not 31370. But `GmlHelpers.ToExtendedWkbGeometry()` then *force-sets* the SRID to
31370. So a Lambert 2008 position reaching the lambda unconverted would be silently relabelled as
Lambert 72 while keeping its 2008 coordinates — a position roughly 500 km off, persisted without any
error. Accepting Lambert 2008 input is therefore only safe in combination with normalization in the API;
the two changes cannot be released separately.

For a garbage or absent `srsName`, `GMLReader` falls back to the reader's factory SRID (31370) rather
than failing, which is why the srsName is parsed and whitelisted explicitly instead of being left to
the reader.

### Components

| Component | Project                                      | Responsibility |
|---|----------------------------------------------|---|
| `TryReadSridGml` / `ReadGeometry` | `Grar.Common` / `Api.BackOffice.Abstractions | Read the `srsName` attribute and map it to a SRID. Accepts 31370 and 3812, with either `http` or `https` scheme. |
| `GmlPointValidator.IsValidPoint` | `Api.BackOffice.Abstractions`                | Replaces `GrAr.Edit`'s `GmlPointValidator` for requests; validates a GML point in either supported reference system. |
| `UseLambert2008EventStoreToggle` | `Api.BackOffice.Abstractions`                | Exposes `EventStoreSrid`. |
| `GmlPositionNormalizer.ToEventStoreSrs` | `Api.BackOffice`                             | Converts the request's GML to `EventStoreSrid`. |
| `FeatureToggleOptions` | `Api.BackOffice`                             | Binds the `FeatureToggles` configuration section. |

`srsName` is read with an `XmlReader` rather than a substring match, so
`missingSrSNameAttribute="…/31370"` is not mistaken for a valid srsName.

An unsupported or missing `srsName` is rejected by the validator
(`AdresPositieformaatValidatie`) before the normalizer is reached; the normalizer still throws
`InvalidOperationException` as a guard.

### Scope

All three actions that take a GML position are done: `ProposeAddress`, `ChangeAddressPosition` and
`CorrectAddressPosition`. Each got the same two changes:
- its request validator uses `GmlPositionValidator.IsValidPoint` instead of `GmlPointValidator`;
- its controller action takes a `[FromServices] GmlPositionNormalizer` and normalizes `request.Positie`
  directly after `ValidateAndThrowAsync`, before the SQS request is built.

Normalization sits after validation (so the GML is known to be a valid point) and before the ETag check
on the two correcting actions, which means a request that ends in a `412` is normalized needlessly —
a transform on an already-parsed string, not worth restructuring for.

Also: toggle + normalizer registered in `ApiModule`, `FeatureToggles:UseLambert2008EventStore` defaults
to `false` in `appsettings.json`, and the swagger description of `Positie` on all three requests
mentions both reference systems.

`ProposeAddressesForMunicipalityMerger` derives its position from the merged address and takes no GML
input, so it needs nothing. No address-registry code references `GrAr.Edit`'s `GmlPointValidator` anymore.

Not touched:
- **The lambda.** `GmlHelpers.ToExtendedWkbGeometry()` still hardcodes Lambert 72 and is left alone
  (marked with a TODO). It only stays correct because the API normalizes to the event store SRS first —
  so flipping the toggle to `true` *requires* the lambda change to land in the same step.
- **Aggregate / projections / extracts / feed.** Unchanged, still Lambert 72.

## Consequences

- While the toggle is off, behaviour for Lambert 72 callers is unchanged, and Lambert 2008 callers
  become supported. Nothing about the event store changes.
- Lambert 2008 input loses precision beyond 2 decimals (centimetre level), same as Lambert 72 input today.
- Flipping the toggle is **not** independently safe: it must be released together with the lambda and
  event store migration. It is a coordination switch, not a runtime kill switch.
- Controller tests that previously fed an AutoFixture-generated `Positie` now need a real GML point,
  since the normalizer runs on every request. That is faithful to production, where validation guarantees
  a valid point before the normalizer is reached.
