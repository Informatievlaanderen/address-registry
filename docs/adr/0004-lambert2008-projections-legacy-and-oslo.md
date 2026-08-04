# 4. Read positions in the reference system they were persisted in (Projections.Legacy, Api.Oslo)

Date: 2026-08-04

## Status

Accepted

## Context

[ADR 0003](0003-lambert2008-gml-input-backoffice.md) made the BackOffice API accept Lambert 2008 (EPSG 3812)
input while still normalizing everything to the event store's reference system, which is Lambert 72
(EPSG 31370) for as long as `FeatureToggles:UseLambert2008EventStore` is off.

The event store conversion itself is still to come. When it lands, `ExtendedWkbGeometry` on the events
will carry SRID 3812 instead of 31370, and everything reading those events has to cope. This ADR covers
the first two consumers: `Projections.Legacy` and `Api.Oslo`.

Positions are persisted as EWKB, which carries its own SRID. So a reader never has to *assume* a
reference system — it only has to stop hardcoding one.

## Decision

### Projections.Legacy

Unchanged, deliberately. `AddressDetailProjectionsV2WithParent` and `AddressSyndicationProjections`
assign `message.Message.ExtendedWkbGeometry.ToByteArray()` straight into `Position` / `PointPosition`
and never parse the bytes, so the projection is already reference-system agnostic: it stores whatever
the event store writes, SRID included, and hands the decision to whoever reads the column.

That is a property worth keeping rather than a coincidence, so it is pinned down by
`AddressDetailItemV2WithParentLambert2008Tests`, which replays Lambert 2008 events and asserts the
stored bytes are byte-for-byte the event's and still read back as SRID 3812.

Consequence: after the conversion the `Position` column holds a *mix* of reference systems — 31370 for
everything projected before it, 3812 after — unless the projection is rebuilt. Every reader must handle
both regardless, so no rebuild is required for correctness.

### `WKBReaderFactory.CreateForEwkb`

`GrAr.Common`'s `WKBReaderFactory.CreateForEwkb` throws when the bytes carry no SRID, and positions
written before the event store wrote EWKB do not. `AddressRegistry.WKBReaderFactory.CreateForEwkb` wraps
it and falls back to the Lambert 72 reader in that case, which matches what `ExtendedWkbGeometry.CreateEWkb`
already does for SRID-less input. This is the single place where "no SRID means Lambert 72" is decided.

Note that `CreateForLegacy()` also happens to read Lambert 2008 EWKB correctly today — both factories use
a floating precision model and `WKBReader` takes the SRID from the bytes. That is an accident of the
current precision models, not a contract, which is why call sites were moved off it.

### Api.Oslo version 2

Version 2 answers in Lambert 72 and nothing else, so its `AddressMapper` reads the position with the
SRID-aware reader and then calls `EnsureLambert72()`. A position that is already Lambert 72 is returned
untouched; a Lambert 2008 one is transformed.

The transform output is rounded to 2 decimals, but *only* on the transformed path. Positions are
persisted at centimetre precision and the transform is accurate to that, so rounding drops floating point
noise (`198794.27000000083`) and makes an 08 → 72 position read identically to how the same position
reads while the event store still holds Lambert 72. A position that was already Lambert 72 is not
rounded, because the syndication response exposes raw coordinates (`GeoJSONPoint.Coordinates`) and
rounding those would change today's output for any position stored with more than 2 decimals.

`GetGml`'s `srsName` stays hardcoded on 31370. It is no longer an assumption: everything reaching it went
through `ReadPositionAsLambert72`. It also keeps the `https` scheme, which is part of the version 2
contract and differs from the `http` scheme `ConvertToGml` emits (see ADR 0003).

### Api.Oslo version 3

Version 3 already handled Lambert 2008 — it emits the Lambert 72 equivalent followed by the position as
persisted. Two things changed, neither of them to the response:

- it reads through `WKBReaderFactory.CreateForEwkb` instead of `CreateForLegacy`;
- it branches on `geometry.IsLambert72()` rather than re-parsing the raw bytes with `TryReadSrid` and
  comparing against `SystemReferenceId.SridLambert2008`, so the SRID is read once, from the geometry
  the response is built from.

## Consequences

- While the event store holds Lambert 72, every response is byte-for-byte what it was. All the new
  behaviour is on the 3812 path, which no production data reaches yet.
- Version 2 consumers never see Lambert 2008, before or after the conversion.
- Version 3 consumers get a second `geometrie` entry once the conversion happens. The swagger example
  (`AddressDetailOsloResponseExamples`) already shows both, so this is what was documented all along —
  but it does mean the array length changes for consumers that assumed one entry.
- Still to do for the conversion: `Projections.Integration`, `Projections.Elastic` (`CoordinateTransformer`
  hardcodes the Lambert 72 WKT), `Projections.Wfs` / `Projections.Wms` (constructed with
  `WKBReaderFactory.CreateForLegacy()`), `Api.Extract` (shapefiles are written as
  `Belge_Lambert_1972`), and the lambda's `GmlHelpers.ToExtendedWkbGeometry()` per ADR 0003.
  `Projections.Feed` already handles both directions.
