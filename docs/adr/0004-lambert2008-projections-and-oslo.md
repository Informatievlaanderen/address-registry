# 4. Read positions in the reference system they were persisted in

Date: 2026-08-04

## Status

Accepted

## Context

[ADR 0003](0003-lambert2008-gml-input-backoffice.md) made the BackOffice API accept Lambert 2008
(EPSG 3812) input while still normalizing everything to the event store's reference system, which is
Lambert 72 (EPSG 31370) for as long as `FeatureToggles:UseLambert2008EventStore` is off.

The event store conversion itself is still to come. When it lands, `ExtendedWkbGeometry` on the events
will carry SRID 3812 instead of 31370, and everything reading those events has to cope. The conversion
will be a full convert including removed addresses, so any mix of the two reference systems downstream
is temporary — but it does exist while the conversion is in flight, and consumers have to survive it.

Positions are persisted as EWKB, which carries its own SRID. So a reader never has to *assume* a
reference system — it only has to stop hardcoding one.

This ADR covers the read side: `Projections.Legacy`, `Projections.AddressMatch`, `Api.Oslo`,
`Projections.Elastic`, `Projections.Integration`, `Projections.Wfs`, `Projections.Wms` and
`Projections.Extract`.

## Decision

### `WKBReaderFactory.CreateForEwkb`

`GrAr.Common`'s `WKBReaderFactory.CreateForEwkb` throws when the bytes carry no SRID, and positions
written before the event store wrote EWKB do not. `AddressRegistry.WKBReaderFactory.CreateForEwkb` wraps
it and falls back to the Lambert 72 reader in that case, which matches what `ExtendedWkbGeometry.CreateEWkb`
already does for SRID-less input. This is the single place where "no SRID means Lambert 72" is decided,
and every consumer below reads through it.

Note that `CreateForLegacy()` also happens to read Lambert 2008 EWKB correctly today — both factories use
a floating precision model and `WKBReader` takes the SRID from the bytes. That is an accident of the
current precision models, not a contract, which is why call sites were moved off it.

### Projections.Legacy and Projections.AddressMatch

Unchanged, deliberately. `AddressDetailProjectionsV2WithParent` and `AddressSyndicationProjections`
assign `message.Message.ExtendedWkbGeometry.ToByteArray()` straight into `Position` / `PointPosition`
and never parse the bytes, so the projection is already reference-system agnostic: it stores whatever
the event store writes, SRID included, and hands the decision to whoever reads the column.

`Projections.AddressMatch` needs nothing for the same reason. Its
`AddressDetailV2WithParent/AddressDetailProjectionsV2WithParent` is the legacy one with a different
`ConnectedProjectionName` — the two files are otherwise identical, and its
`AddressDetailItemV2WithParent` is the legacy row minus a few columns. Its only consumer,
`Api.Oslo/AddressMatch/V2/AddressMapper`, reads the position through
`Address.V2.AddressMapper.GetAddressPoint`, so it inherits the version 2 behaviour below and keeps
answering in Lambert 72. That is the only geometry in the whole address match API — the matching and
scoring are text-only, and `AdresPositie` is carried straight through to the response — so nothing else
there needs looking at. `AddressMatchV2Tests` pins the delegation end to end, since reading the position
any other way is the one thing that would break it.

That both are agnostic is a property worth keeping rather than a coincidence, so each is pinned down by
an `AddressDetailItemV2WithParentLambert2008Tests`, which replays Lambert 2008 events and asserts the
stored bytes are byte-for-byte the event's and still read back as SRID 3812. The address match
projection had no projection tests at all, so it also got an `AddressMatchProjectionTest` harness,
mirroring the legacy one.

Consequence: after the conversion the `Position` column of both holds a *mix* of reference systems —
31370 for everything projected before it, 3812 after — unless the projection is rebuilt. Every reader
must handle both regardless, so no rebuild is required for correctness.

### Api.Oslo version 2

Version 2 answers in Lambert 72 and nothing else — with the single exception of the syndication object,
below — so its `AddressMapper` reads the position with the SRID-aware reader and then calls
`EnsureLambert72()`. A position that is already Lambert 72 is returned untouched; a Lambert 2008 one is
transformed.

The transform output is rounded to 2 decimals, but *only* on the transformed path. Positions are
persisted at centimetre precision and the transform is accurate to that, so rounding drops floating point
noise (`198794.27000000083`) and makes an 08 → 72 position read identically to how the same position
reads while the event store still holds Lambert 72. A position that was already Lambert 72 is not
rounded, because the syndication response exposes raw coordinates (`GeoJSONPoint.Coordinates`) and
rounding those would change today's output for any position stored with more than 2 decimals.

`GetGml`'s `srsName` stays hardcoded on 31370. It is no longer an assumption: everything reaching it went
through `ReadPositionAsLambert72`. It also keeps the `https` scheme, which is part of the version 2
contract and differs from the `http` scheme `ConvertToGml` emits (see ADR 0003).

### Api.Oslo version 2 syndication: the caller picks, through `objectCrs`

The syndication feed under `Address/V2/Sync` — version 2 is the only version that has one — is the one
version 2 response whose reference system is not fixed. Its object carries the position as a
`SpatialTools.Point`, whose `GmlPoint` has **no `srsName` member at all**: a bare `<pos>`, plus raw
`GeoJSONPoint.Coordinates`. So the feed cannot say which reference system it is in, and letting the object
silently follow the event store would move every consumer's coordinates ~500 km with nothing in the payload
to signal it. This one cannot be solved downstream.

A new filter, `objectCrs`, on both `AddressSyndicationFilter` and `AddressSyndicationPersistentLocalIdFilter`,
makes the choice the caller's:

- `3812` → the object's position is emitted in Lambert 2008: transformed if the store still holds
  Lambert 72, passed through once it holds Lambert 2008.
- **anything else — an unrecognised value, an empty one, or no filter at all → Lambert 72**: passed through
  while the store holds Lambert 72, transformed back once it holds Lambert 2008.

The default is what keeps the promise below that version 2 consumers never see Lambert 2008 unless they ask
for it. An unrecognised value falls back rather than returning 400, so the feed never breaks on a typo; the
cost is that `objectCrs=EPSG:3812` silently yields Lambert 72, since only the exact string `3812` (trimmed)
selects Lambert 2008. `ObjectCrs.ToSrid` is the single place that mapping lives and
`GivenObjectCrsFilter.ThenOnlyTheExactValue3812SelectsLambert2008` pins the accepted spellings.

`AddressMapper` gained a `GetAddressPoint(byte[], int srid)` overload, and `ReadPositionAsLambert72` became
a call to a general `ReadPosition(point, srid)`. The existing parameterless-SRID overloads are unchanged and
still pin to Lambert 72, so the detail and list responses — and `AddressMatch`, which reads through
`GetAddressPoint` — are untouched by this and keep their existing tests.

**Only the object is reprojected.** The embedded `event` is the event store's own payload, emitted verbatim
at every position, whatever `objectCrs` says. A feed replayed for auditing therefore still shows what was
actually stored, including the conversion event itself.

### Api.Oslo version 3

Version 3 already handled Lambert 2008 — it emits the Lambert 72 equivalent followed by the position as
persisted. Two things changed, neither of them to the response:

- it reads through `WKBReaderFactory.CreateForEwkb` instead of `CreateForLegacy`;
- it branches on `geometry.IsLambert72()` rather than re-parsing the raw bytes with `TryReadSrid` and
  comparing against `SystemReferenceId.SridLambert2008`, so the SRID is read once, from the geometry
  the response is built from.

### Projections.Elastic

The index holds two fields per position:

| Field | Mapping | Used by |
|---|---|---|
| `AddressPosition.GeometryAsWkt` | `text` | nothing — written, never read |
| `AddressPosition.GeometryAsWgs84` | `geo_point` | search and geo queries |

#### The indexed WKT becomes EWKT

`GeometryAsWkt` was `point.AsText()`, plain WKT, which does not say which of the two Lambert systems the
coordinates are in — the only way to tell was to look at the numbers and recognise that Lambert 2008
eastings are ~500 km larger. It now holds EWKT via `PositionExtensions.ToEwkt()`:
`SRID=31370;POINT (140252.76 198794.27)` or `SRID=3812;POINT (640249.09 698793.29)`.

The alternative was to normalize every position to Lambert 72 on the way into the index, which would
remove the mix entirely. That was rejected: it would transform on every write for a field nothing reads,
and it would make the index disagree with what the event store actually holds, which is exactly the
thing you want to be able to see while a conversion is in flight.

A position whose EWKB carries no SRID is labelled Lambert 72, matching `CreateForEwkb`.

This changes the value of an existing field. It is safe because nothing in this repo reads
`GeometryAsWkt`, it is mapped as `text` rather than parsed, and the conversion re-indexes everything
anyway. The current data in the WKT without the SRID will have to be looked at as Lambert 72.

#### The WGS84 geo point is projected from whichever system the position is in

`CoordinateTransformer.ToWgs84Text` now picks the source coordinate system from the position:
`IsLambert08()` projects from a new EPSG 3812 definition, everything else from the existing Lambert 72
one. Both go straight to WGS84.

Lambert 2008 is **not** routed through Lambert 72 first. It is already on ETRS89, which needs no datum
shift to WGS84, so projecting it directly loses nothing; bouncing it off Lambert 72 would push it through
a datum transform it does not need.

The EPSG 3812 definition was validated by projecting the same physical point three ways at three
locations across Flanders: directly from Lambert 2008, and from Lambert 72 with the official BD72 →
WGS84 seven-parameter shift applied. The two agree to within 15 cm.

That validation surfaced a pre-existing defect worth recording. `Lambert72Wkt` carries **no `TOWGS84`
element**, so ProjNet projects BD72 coordinates as though they were already on the WGS84 datum. Every
`geo_point` indexed from Lambert 72 — which today is all of them — is therefore about **90 m** off
(~86 m east, ~30 m north, consistently). The Lambert 2008 path is the accurate one.

This is left alone here on purpose. Adding `TOWGS84` to `Lambert72Wkt` would move every existing
`geo_point` by that same 90 m in one go, which is a decision about the search index, not about the
Lambert 2008 conversion, and it deserves its own change. `GivenPositionInEitherReferenceSystem` asserts
the ~90 m disagreement as a tripwire so that whoever does fix it sees why the test exists.

Without the SRID-aware branch at all, a Lambert 2008 position would have been projected as if its
coordinates were Lambert 72 and landed roughly 500 km away — silently, since the value would still be a
well-formed `geo_point`. That is the failure this section actually prevents.

#### The reader is created per position

Both projections dropped their cached `_wkbReader` for `WKBReaderFactory.CreateForEwkb(bytes)` per
position, so the reference system comes from the EWKB rather than from a reader chosen at construction.
That costs about 1 µs per position against 0.3 µs for a cached reader — irrelevant next to the Elastic
round trip that follows it.

### Projections.Integration

This one writes to PostGIS rather than SQL Server, into a `geometry` column with a GIST index, and is
consumed entirely outside this repository — nothing in this codebase reads it.

The 29 `WKBReaderFactory.CreateForLegacy().Read(…)` call sites across `AddressLatestItemProjection`,
`AddressLatestItemProjectionsV2` and `AddressVersionProjection` were replaced by a single
`PositionReader.ReadPosition(…)`, which reads through `CreateForEwkb`. Npgsql's NetTopologySuite plugin
writes the geometry's SRID into the column, so the row ends up carrying the reference system the event
store wrote and `ST_SRID` can be branched on.

**The column is deliberately allowed to hold both.** That is a bigger commitment than it was for Elastic,
because PostGIS raises `ERROR: Operation on mixed SRID geometries` from `ST_Within`, `ST_Intersects`,
`ST_DWithin` and friends whenever the two operands disagree — and the column is plain `geometry` with no
SRID constraint, so Postgres accepts the mix silently and the breakage surfaces later, in the consumer's
queries. Normalizing to a single reference system on write was considered and rejected for three reasons
specific to how this database is operated:

- the consuming repository's views compare against a reference geometry that is held in both Lambert 72
  and Lambert 2008, so they can pick the matching one per row;
- the conversion runs under a freeze of external viewing and editing, so no consumer is reading through
  the mixed window unprepared;
- the rebuild is fast, so the window is short.

Two things about that window are worth writing down.

**The GIST index stays valid.** `gist_geometry_ops_2d` indexes each row's 2D bounding box in raw
coordinate space and ignores SRID entirely, so Lambert 72 and Lambert 2008 entries coexist without
corrupting it. No reindex is needed and inserts do not fail. The SRID error comes from the predicate
functions, never from the index.

**Branching costs the index, though.** PostgreSQL does not guarantee left-to-right evaluation of `AND`
and `OR` operands — the planner reorders by cost — so a guard like
`ST_SRID(g) = 31370 AND ST_Within(g, ref72)` can still hit the error. `CASE` is the documented way to
force evaluation order and is therefore the correct construct, but it also hides the `&&` that
`ST_Within` normally expands to, which is what the GIST index accelerates. Expect a sequential scan for
the duration of the mixed window, and time the view refresh against a mixed table before the freeze
rather than during it.

### Projections.Wfs

The WFS projection writes `[wfs.address].[AddressWfsV2]` — a SQL Server table with a `sys.geometry`
`Position` column and a spatial index — and two `SCHEMABINDING` views over it, `[wfs].[AdresView]` and
`[geolocation].[AddressOsloGeolocationView]`, which an external team serves through geowebserver for
WMS/WFS/OGC-API. Letting that table go mixed is not on the table: SQL Server's spatial methods return
`NULL` rather than erroring on an SRID mismatch, the spatial index has a `BOUNDING_BOX` in Lambert 72
coordinates that Lambert 2008 rows fall entirely outside of, and the geolocation view builds its GML
with a hardcoded `srsName` of EPSG 31370.

So instead of one table that changes meaning, there are two that do not:

- **V2 is pinned to Lambert 72.** `ParsePosition` reads SRID-aware and then `EnsureLambert72()`, rounding
  only on the transformed path, exactly as Api.Oslo version 2 does. Once the event store holds Lambert
  2008 this starts transforming, and the table, its index and both views carry on unchanged. Consumers
  of V2 see nothing at all.
- **V3 is a new projection pinned to Lambert 2008**, writing `[wfs.address].[AddressWfsV3]` with its own
  `[wfs].[AdresViewV3]` and `[geolocation].[AddressOsloGeolocationViewV3]`. `ParsePosition` is
  `EnsureLambert08(2)`, which transforms and rounds today and becomes a pass-through once the event store
  is converted. The geolocation view is identical to the V2 one except for the source table and an
  `srsName` of EPSG 3812.

The two run side by side until the geoserver consumers have moved to the V3 views, after which V2, its
table and its views are deleted in one go.

`AddressWfsV3Projections`, `AddressWfsV3Item`, `AddressWfsV3Extensions` and the V3
`HouseNumberLabelUpdater` were produced mechanically from their V2 counterparts (a rename of
`AddressWfsV2` to `AddressWfsV3`), so the ~900 lines of event handling are identical by construction
rather than by review; `ParsePosition` and the projection name are the only hand edits. That keeps the
eventual deletion of V2 a clean delete, at the cost of a duplicate that has to be kept in step while
both exist.

The test suites were copied the same way and for the same reason: `AddressWfsV3ProjectionTests` and
`AddressWfsItemV3HouseNumberLabelTests` are the V2 files renamed, so V3 is covered to the same depth
instead of being a large untested copy. The label tests are a byte-for-byte rename. The projection tests
needed one change: their position expectations read the event with a Lambert 72 reader, so in V3 they go
through an `ExpectedPosition` helper that also applies `EnsureLambert08(2)`. That does mirror what the
projection does, which is why the reference system is asserted against *fixed* coordinates in
`GivenPositionInEitherReferenceSystem` — one of those per version — rather than relied on here.

Worth knowing when reading those tests: `EnsureLambert08` only transforms geometries that actually fall
inside Flanders, and relabels everything else. The fixture generates positions from random `uint`s, so
most of the copied assertions compare unchanged coordinates; only the handful using real Lambert 72
points (`GeometryHelpers.*GmlPointGeometry`) actually move.

The V3 spatial index needs its own `BOUNDING_BOX`. The V2 one,
`(22279.17, 153050.23, 258873.3, 244022.31)`, was converted by transforming all four corners — a
conformal projection does not map a rectangle to a rectangle — and padding the resulting envelope out to
the next 100 m, giving `(522200, 653000, 758900, 744100)`.

Both projections dropped the `WKBReader` from their constructor. It was injected as
`WKBReaderFactory.CreateForLegacy()` from `ApiModule`, which is exactly the assumption being removed;
the reader now comes from the EWKB per position.

### Projections.Wms

The same shape as WFS and handled the same way, one version number further along: the current projection
is already `AddressWmsItemV3` writing `[wms.address].[AddressWmsV3]`, so its Lambert 2008 counterpart is
**V4**.

- **V3 is pinned to Lambert 72** — same `ParsePosition` as WFS version 2.
- **V4 is a new projection pinned to Lambert 2008**, writing `[wms.address].[AddressWmsV4]`.

WMS has five views rather than two: `[wms].[AdresView]` over the table, plus `AdresVoorgesteld`,
`AdresInGebruik`, `AdresGehistoreerd` and `AdresAfgekeurd`, which select from `AdresView` and filter on
status. All five get a `V4` counterpart. Because the status views are `SCHEMABINDING` over `AdresViewV4`,
the migration creates `AdresViewV4` first and drops it last; the four status view definitions are emitted
from a `(view, status)` table rather than copied out five times. The `[wms].[GetFullAddress]` scalar
function is reference-system agnostic and is reused as is.

The spatial index gets the same recomputed `BOUNDING_BOX` as WFS V3, `(522200, 653000, 758900, 744100)`,
since both started from the same Lambert 72 box.

Source and tests were copied mechanically, exactly as for WFS. One extra rename beyond the type names:
the context extension methods are called `FindAddressDetailV3` / `FindAndUpdateAddressDetailV3`, so those
became `…V4` — otherwise the V4 file would have been calling V3-named helpers on itself.

With this, no `WKBReaderFactory.CreateForLegacy()` is left in `ApiModule`.

### Projections.Extract and Api.Extract

The extract is pinned to Lambert 72 with the same `ParsePosition` as WFS version 2, and gets no second
version. It is expected to be retired before the event store is converted; this change is only so that it
keeps producing correct shapefiles rather than silently 500-km-offset ones if it outlives the conversion.

`Api.Extract` needs nothing, which is worth spelling out because it looks like it should. It writes the
`.prj` as `ProjectedCoordinateSystem.Belge_Lambert_1972` and it builds the shapefile from
`ShapeRecordContent` and the `MinimumX`/`MaximumY` bounding box — all of which the projection has already
computed. It never touches a coordinate, so pinning the projection is what keeps the `.prj` honest.

That is also why this one cannot be solved downstream: a shape record carries no SRID, so nothing after
the projection could tell which reference system the bytes are in.

The extract had no projection tests; it now has a `GivenPositionInEitherReferenceSystem` like the others,
asserting the stored bounding box is Lambert 72 for events in either reference system.

## Consequences

- While the event store holds Lambert 72, every API response is byte-for-byte what it was, and the only
  change to an indexed document is the `SRID=31370;` prefix on `GeometryAsWkt`. All the new behaviour is
  on the 3812 path, which no production data reaches yet.
- Version 2 consumers never see Lambert 2008, before or after the conversion, **unless they ask for it on
  the syndication feed with `objectCrs=3812`**. Callers that do not pass the filter are unaffected in either
  direction. `AddressSyndicationFilter` is populated from the `X-Filtering` header, so exposing `objectCrs`
  as a query parameter needs the same gateway mapping that `embed` and `from` already rely on — that part
  lives outside this repository.
- Version 3 consumers get a second `geometrie` entry once the conversion happens. The swagger example
  (`AddressDetailOsloResponseExamples`) already shows both, so this is what was documented all along —
  but it does mean the array length changes for consumers that assumed one entry.
- Consumers reading `GeometryAsWkt` — none today — must handle the EWKT prefix, and get to see the
  reference system instead of guessing it.
- **Geo search moves by ~90 m per address as the conversion progresses.** Addresses already reprojected
  from Lambert 2008 get their accurate WGS84 position; those not yet reprojected keep the ~90 m error
  described above. During the conversion the index therefore holds both, and two neighbouring addresses
  can sit 90 m apart in a geo query until both have been converted. The end state is that all of them are
  correct, so this is a one-off correction rather than a regression — but it is visible while it runs,
  and it is a reason to run the conversion as one pass rather than trickle it.
- The PostGIS `geometry` column holds a mix of SRIDs while the conversion runs. Consumers must branch on
  `ST_SRID`, under the caveats above, and should expect the loss of index-assisted spatial filtering for
  that window.
- WFS gains a second table and two more views for the duration of the migration, WMS a second table and
  five more views; the geoserver team moves over at its own pace and the old versions are deleted once
  nobody reads them.
- Still to do for the conversion: the lambda's `GmlHelpers.ToExtendedWkbGeometry()` per ADR 0003, and the
  event store conversion itself — both covered by
  [ADR 0005](0005-lambert2008-event-store-transformation.md), which also records what each projection does
  with the transformation event. `Projections.Feed` already handled both directions before this ADR.
